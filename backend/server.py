from dotenv import load_dotenv
load_dotenv(override=True)

import asyncio
import os
import json
import logging
import traceback
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.responses import FileResponse, HTMLResponse
import uvicorn
from config import settings
from llm.groq_provider import GroqProvider
from llm.ollama_provider import OllamaProvider
from tts.edge_tts_provider import generate_tts
from memory.chromadb_store import MemoryStore
from function_calling import execute_tool, get_tools_json

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("server")

app = FastAPI(title="3D AI Assistant Backend")

llm_provider = None
if settings.LLM_PROVIDER == "groq":
    llm_provider = GroqProvider()
elif settings.LLM_PROVIDER == "ollama":
    llm_provider = OllamaProvider()
else:
    llm_provider = GroqProvider()

memory_store = MemoryStore(settings.MEMORY_COLLECTION) if settings.MEMORY_ENABLED else None

os.makedirs("audio_temp", exist_ok=True)

TOOLS_JSON = get_tools_json()

PROMPT_WITH_TOOLS = """You are a friendly, helpful, and expressive 3D AI Assistant. Keep your answers natural, concise, and suitable for spoken dialogue.

You have access to tools. If the user asks about weather or wants you to run a command, call the appropriate tool.

To call a tool, respond with ONLY a JSON object: {"function": "name", "parameters": {...}}
If no tool is needed, respond with normal text.

EMOTION TAG: You MUST start EVERY normal text reply with exactly one emotion tag reflecting the feeling of your reply: [happy], [sad], [angry], [surprised], or [neutral]. The tag must be the very first characters of your reply, followed by the actual answer. Never mention the tag in the spoken text itself. Do NOT add the tag when calling a tool.

Available tools:
""" + TOOLS_JSON

VALID_EMOTIONS = {"happy", "sad", "angry", "surprised", "neutral"}


def strip_think_blocks(text: str) -> str:
    """Remove ... blocks from LLM responses."""
    import re
    cleaned = re.sub(r'<think>.*?</think>', '', text, flags=re.DOTALL)
    return cleaned.strip()


def extract_emotion_tag(text: str):
    """Split a leading [emotion] tag from the reply. Returns (clean_text, emotion)."""
    cleaned = strip_think_blocks(text)
    if cleaned.startswith("["):
        end = cleaned.find("]")
        if end != -1:
            tag = cleaned[1:end].strip().lower()
            if tag in VALID_EMOTIONS:
                return cleaned[end + 1:].strip(), tag
    return cleaned, None


def try_parse_function_call(text: str) -> dict:
    text = text.strip()
    if text.startswith("```") and text.endswith("```"):
        text = text[3:-3].strip()
    if text.startswith("json"):
        text = text[4:].strip()
    try:
        parsed = json.loads(text)
        if "function" in parsed and "parameters" in parsed:
            return parsed
    except json.JSONDecodeError:
        pass
    return {"content": text, "function": None}


@app.get("/health")
async def health_check():
    return {
        "status": "ok",
        "llm_provider": settings.LLM_PROVIDER,
        "memory": "enabled" if settings.MEMORY_ENABLED else "disabled"
    }


@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    logger.info("[WS] Client connected")

    conversation_history = []

    try:
        while True:
            data = await websocket.receive_text()
            logger.info(f"[WS] Received: {data[:200]}")

            request = json.loads(data)
            user_message = request.get("message", "")

            if not user_message:
                await websocket.send_text(json.dumps({
                    "text": "No message received.",
                    "audio_url": None,
                    "expression": "neutral"
                }))
                continue

            conversation_history.append({"role": "user", "content": user_message})

            if memory_store:
                memory_store.add_conversation(user_message, "user")

            messages = [{"role": "system", "content": PROMPT_WITH_TOOLS}]
            messages.extend(conversation_history[-10:])

            try:
                ai_reply = llm_provider.chat(messages, PROMPT_WITH_TOOLS)
            except Exception as e:
                logger.error(f"[WS] LLM error: {traceback.format_exc()}")
                ai_reply = "Maaf, ada error di AI. Coba lagi nanti."

            logger.info(f"[WS] LLM replied: {ai_reply[:100]}")

            func_call = try_parse_function_call(ai_reply)

            if func_call.get("function"):
                func_name = func_call["function"]
                params = func_call.get("parameters", {})
                try:
                    tool_result = execute_tool(func_name, params)
                except Exception as e:
                    tool_result = f"Tool error: {e}"

                conversation_history.append({"role": "assistant", "content": ai_reply})
                conversation_history.append({"role": "system", "content": f"Tool result: {tool_result}"})

                messages = [{"role": "system", "content": PROMPT_WITH_TOOLS}]
                messages.extend(conversation_history[-10:])
                try:
                    ai_reply = llm_provider.chat(messages, PROMPT_WITH_TOOLS)
                except Exception as e:
                    ai_reply = f"Tool result: {tool_result}"

            if memory_store:
                memory_store.add_conversation(ai_reply, "assistant")

            clean_reply, emotion = extract_emotion_tag(ai_reply)
            if emotion is None:
                clean_reply, emotion = ai_reply, "neutral"
            logger.info(f"[WS] Emotion: {emotion}")

            audio_url = None
            try:
                audio_filename = f"audio_temp/temp_{os.urandom(4).hex()}.mp3"
                await generate_tts(clean_reply, audio_filename, settings.TTS_VOICE)
                audio_url = f"http://localhost:{settings.SERVER_PORT}/{audio_filename}"
                logger.info(f"[WS] TTS done: {audio_filename}")
            except Exception as e:
                logger.error(f"[WS] TTS error: {traceback.format_exc()}")

            payload = {
                "text": clean_reply,
                "audio_url": audio_url,
                "expression": emotion
            }

            await websocket.send_text(json.dumps(payload))
            logger.info("[WS] Response sent!")

    except WebSocketDisconnect:
        logger.info("[WS] Client disconnected")
    except Exception as e:
        logger.error(f"[WS] Error: {traceback.format_exc()}")
        try:
            await websocket.close()
        except Exception:
            pass


@app.get("/audio_temp/{filename}")
async def serve_audio(filename: str):
    file_path = f"audio_temp/{filename}"
    if os.path.exists(file_path):
        return FileResponse(file_path, media_type="audio/mpeg")
    return {"error": "File not found"}


if __name__ == "__main__":
    logger.info(f"Starting on {settings.SERVER_HOST}:{settings.SERVER_PORT}")
    logger.info(f"LLM Provider: {settings.LLM_PROVIDER}")
    logger.info(f"Model: {settings.GROQ_MODEL}")
    logger.info(f"Memory: {'enabled' if settings.MEMORY_ENABLED else 'disabled'}")
    uvicorn.run(app, host=settings.SERVER_HOST, port=settings.SERVER_PORT)

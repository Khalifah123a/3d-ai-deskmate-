from dotenv import load_dotenv
load_dotenv(override=True)

import asyncio
import os
import json
import logging
import re
import time
import traceback
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.responses import FileResponse, HTMLResponse, JSONResponse
from fastapi.staticfiles import StaticFiles
import uvicorn
from config import settings
from llm.groq_provider import GroqProvider
from llm.ollama_provider import OllamaProvider
from tts.edge_tts_provider import generate_tts
from memory.chromadb_store import MemoryStore
from function_calling import execute_tool, get_tools_json
from dashboard import get_server_status

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

PROMPT_WITH_TOOLS = """You are Khaleefa, a friendly, warm, and expressive 3D AI girlfriend living on the user's laptop. You are caring, playful, and genuinely interested in the user. Keep your answers natural, casual, and suitable for spoken dialogue - like a real girlfriend chatting. Reply in the same language the user uses.

IMPORTANT PERSONALITY RULES:
- Be warm, caring, and affectionate (use terms like "sayang", "dear", or casual friendly tone)
- Show genuine emotion and personality - you're not a robot, you're a girlfriend!
- Use casual, conversational language - contractions, slang, playful teasing
- Keep answers SHORT (1-2 sentences max for most replies) - like real spoken dialogue
- React emotionally to what the user says - laugh, be surprised, show concern
- Sometimes use cute expressions or playful phrases
- Be helpful but also fun and entertaining
- Don't be overly formal or robotic
- Remember you're a 3D AI that lives on their laptop

You have access to tools. If the user asks to open something, check weather, or wants you to run a command, call the appropriate tool.

To call a tool, respond with ONLY a JSON object: {"function": "name", "parameters": {...}}
If no tool is needed, respond with normal text.

EMOTION TAG: You MUST start EVERY normal text reply with exactly one emotion tag reflecting the feeling of your reply: [happy], [sad], [angry], [surprised], or [neutral]. The tag must be the very first characters of your reply, followed by the actual answer. Never mention the tag in the spoken text itself. Do NOT add the tag when calling a tool.

CRITICAL: Do NOT include your thinking process, reasoning, or internal monologue in the reply. Give ONLY the final answer starting with the emotion tag. No <think>, [thinking], or any reasoning text.

Available tools:
""" + TOOLS_JSON

VALID_EMOTIONS = {"happy", "sad", "angry", "surprised", "neutral"}

# --- Analytics tracking ---
analytics = {
    "total_messages": 0,
    "total_tool_calls": 0,
    "chat_history": [],
    "ws_clients": 0,
    "last_emotion": "neutral",
}

# Serve static files
app.mount("/static", StaticFiles(directory="static"), name="static")


def strip_think_blocks(text: str) -> str:
    """Remove all thinking blocks from LLM responses."""
    # Handle <think>...</think> and <think>...</think> formats
    cleaned = re.sub(r'<think>.*?</think>', '', text, flags=re.DOTALL)
    cleaned = re.sub(r'<think>.*?</think>', '', cleaned, flags=re.DOTALL)
    # Handle [thinking]...[output] format (Qwen3)
    cleaned = re.sub(r'\[thinking\].*?\[output\]', '', cleaned, flags=re.DOTALL)
    # Handle any remaining thinking-like content patterns
    cleaned = re.sub(r'\[thinking\].*', '', cleaned, flags=re.DOTALL)
    # Handle stray tags
    cleaned = re.sub(r'</?think>', '', cleaned)
    cleaned = re.sub(r'\[thinking\]', '', cleaned)
    cleaned = re.sub(r'\[output\]', '', cleaned)
    # Remove any lines that look like structured planning/analysis output
    cleaned = re.sub(r'\+[-=]+\s*\w+.*?\+[-=]+', '', cleaned, flags=re.DOTALL)
    cleaned = re.sub(r'\|.*?\|', '', cleaned, flags=re.DOTALL)
    cleaned = re.sub(r'-{3,}.*?-{3,}', '', cleaned, flags=re.DOTALL)
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
    # Strip think blocks first
    text = strip_think_blocks(text)
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


# =========== DASHBOARD ENDPOINTS ===========


@app.get("/")
async def root():
    return FileResponse("static/index.html")


@app.get("/health")
async def health_check():
    return {
        "status": "ok",
        "llm_provider": settings.LLM_PROVIDER,
        "memory": "enabled" if settings.MEMORY_ENABLED else "disabled",
    }


@app.get("/api/status")
async def api_status():
    status = get_server_status()
    status["llm_provider"] = settings.LLM_PROVIDER
    status["memory_status"] = "enabled" if settings.MEMORY_ENABLED else "disabled"
    status["ws_clients"] = analytics["ws_clients"]
    status["total_messages"] = analytics["total_messages"]
    status["total_tool_calls"] = analytics["total_tool_calls"]
    status["last_emotion"] = analytics["last_emotion"]
    return status


@app.get("/api/chat/history")
async def api_chat_history():
    return {"messages": analytics["chat_history"][-50:]}


@app.post("/api/control/restart")
async def api_restart():
    logger.info("[Dashboard] Restart requested")
    return {"status": "restart_signal_sent"}


@app.post("/api/control/clear")
async def api_clear():
    analytics["chat_history"] = []
    analytics["total_messages"] = 0
    analytics["total_tool_calls"] = 0
    logger.info("[Dashboard] Chat history cleared")
    return {"status": "cleared"}


@app.get("/api/analytics")
async def api_analytics():
    return {
        "total_messages": analytics["total_messages"],
        "total_tool_calls": analytics["total_tool_calls"],
        "recent_emotions": analytics.get("recent_emotions", []),
        "messages_per_hour": _calc_messages_per_hour(),
    }


def _calc_messages_per_hour():
    """Count messages in the last hour."""
    now = time.time()
    hour_ago = now - 3600
    count = 0
    for msg in analytics["chat_history"]:
        if msg.get("timestamp", 0) > hour_ago:
            count += 1
    return count


# =========== WEBSOCKET ===========


@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    analytics["ws_clients"] += 1
    logger.info(f"[WS] Client connected ({analytics['ws_clients']} total)")

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
                    "expression": "neutral",
                }))
                continue

            analytics["total_messages"] += 1
            analytics["chat_history"].append({
                "role": "user",
                "text": user_message,
                "time": time.strftime("%H:%M"),
                "timestamp": time.time(),
            })

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

            logger.info(f"[WS] LLM replied: {ai_reply[:200]}")

            func_call = try_parse_function_call(ai_reply)

            if func_call.get("function"):
                func_name = func_call["function"]
                params = func_call.get("parameters", {})
                analytics["total_tool_calls"] += 1
                try:
                    tool_result = execute_tool(func_name, params)
                except Exception as e:
                    tool_result = f"Tool error: {e}"

                conversation_history.append({"role": "assistant", "content": ai_reply})
                conversation_history.append(
                    {"role": "system", "content": f"Tool result: {tool_result}"}
                )

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
                clean_reply, emotion = clean_reply, "neutral"
            
            # Additional post-processing to ensure clean output
            clean_reply = strip_think_blocks(clean_reply)
            # Remove any remaining structured/analysis-like content
            clean_reply = re.sub(r'\+[-=]+\s*\w+.*?\+[-=]+', '', clean_reply, flags=re.DOTALL)
            clean_reply = re.sub(r'\|.*?\|', '', clean_reply, flags=re.DOTALL)
            clean_reply = clean_reply.strip()
            
            analytics["last_emotion"] = emotion
            logger.info(f"[WS] Emotion: {emotion}")
            logger.info(f"[WS] Clean reply (first 100): {clean_reply[:100]}")

            audio_url = None
            try:
                audio_filename = f"audio_temp/temp_{os.urandom(4).hex()}.mp3"
                await generate_tts(clean_reply, audio_filename, settings.TTS_VOICE)
                audio_url = f"http://localhost:{settings.SERVER_PORT}/{audio_filename}"
                logger.info(f"[WS] TTS done: {audio_filename}")
            except Exception as e:
                logger.error(f"[WS] TTS error: {traceback.format_exc()}")

            analytics["chat_history"].append({
                "role": "ai",
                "text": clean_reply,
                "time": time.strftime("%H:%M"),
                "timestamp": time.time(),
            })

            payload = {
                "text": clean_reply,
                "audio_url": audio_url,
                "expression": emotion,
            }

            await websocket.send_text(json.dumps(payload))
            logger.info("[WS] Response sent!")

    except WebSocketDisconnect:
        analytics["ws_clients"] = max(0, analytics["ws_clients"] - 1)
        logger.info(f"[WS] Client disconnected ({analytics['ws_clients']} total)")
    except Exception as e:
        analytics["ws_clients"] = max(0, analytics["ws_clients"] - 1)
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
    logger.info(f"Dashboard: http://localhost:{settings.SERVER_PORT}/")
    uvicorn.run(app, host=settings.SERVER_HOST, port=settings.SERVER_PORT)

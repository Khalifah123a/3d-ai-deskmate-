---
type: spec
feature: 3d-ai-backend
status: draft
priority: P0
depends_on:
tags:
  - 3d-ai
  - backend
  - fastapi
---

# 3D AI Backend — Spesifikasi

> Backend server for the 3D AI Girlfriend Assistant, handling WebSocket connections, LLM processing, and TTS generation.

## Tujuan

Provide a reliable, low-latency backend for real-time voice interaction with a 3D AI character.

## Persyaratan Fungsional

- [ ] RF-1: WebSocket server accepting connections from Unity client
- [ ] RF-2: Process user messages and generate AI responses
- [ ] RF-3: Convert AI responses to speech audio
- [ ] RF-4: Store conversation history in vector database
- [ ] RF-5: Support function calling for system commands

## Persyaratan Non-Fungsional

- [ ] Performa: Response time < 500ms (excluding TTS)
- [ ] Keamanan: API keys in environment variables only
- [ ] Reliability: Graceful handling of LLM/TTS failures

## Desain API

```
WebSocket /ws
→ Receive: {"event": "user_message", "message": "..."}
→ Send: {"event": "ai_response", "text": "...", "audio_url": "..."}
```

```
GET /health
→ 200 {"status": "ok", "llm_provider": "groq"}
```

```
GET /audio_temp/{filename}
→ 200 MP3 file
```

## Skema Database (Memory)

```python
# ChromaDB Collection
collection = "ai_assistant_conversations"
documents = [text]
metadatas = [{"role": "user"|"assistant", "timestamp": "..."}]
ids = ["user_0", "assistant_1", ...]
```

## Edge Cases

- [ ] WebSocket disconnect → Log and cleanup
- [ ] LLM timeout → Return fallback message
- [ ] TTS failure → Return text-only response
- [ ] Empty message → Ignore

## Kriteria Penerimaan

- [ ] `python server.py` starts without errors
- [ ] WebSocket connection from client succeeds
- [ ] LLM response received within 500ms
- [ ] Audio file generated successfully
- [ ] Memory stores conversation correctly

## Referensi

- [[backend-development]]
- [[02 - Backend Bridge]]
- [[01 - Architecture]]

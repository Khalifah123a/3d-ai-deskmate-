---
type: adr
status: proposed
date: 2026-08-13
related: "[[01 - Architecture]]"
---

# ADR-001: 3D AI Assistant Architecture

## Context

We need to build a 3D AI girlfriend assistant with real-time voice interaction, lip-sync animation, and system control capabilities.

### Options Considered

| Option | Pros | Cons |
|--------|------|------|
| **Unity + Python** | Best 3D rendering, VRM support, rich ecosystem | Requires Unity installation |
| **Three.js + Python** | Browser-based, no install | Limited 3D capabilities, no VRM |
| **Godot + Python** | Open-source, lightweight | Smaller community, fewer 3D tools |

## Decision

**Unity + Python FastAPI** architecture:

- **3D Engine**: Unity 6.0 (6000.0.80f1) for VRM character rendering
- **Backend**: Python FastAPI for WebSocket server
- **LLM**: Groq API (primary) + Ollama (local fallback)
- **TTS**: Edge-TTS (free, natural voices)
- **Memory**: ChromaDB for vector storage
- **Lip-Sync**: uLipSync for real-time animation

## Consequences

### Positive
- ✅ Best-in-class 3D rendering with Unity
- ✅ Native VRM support via UniVRM
- ✅ Fast LLM responses via Groq
- ✅ Free TTS with natural voices
- ✅ Scalable memory with ChromaDB

### Negative
- ❌ Requires Unity installation for client
- ❌ Python backend adds complexity
- ❌ WebSocket adds latency vs direct API

### Follow-up
- [ ] Investigate Unity Cloud Build for distribution
- [ ] Benchmark WebSocket latency
- [ ] Test Ollama performance on local hardware

## Alternatives Rejected

| Opsi | Alasan ditolak |
|------|----------------|
| Three.js | Limited 3D, no VRM support |
| Godot | Smaller ecosystem, fewer tools |
| Pure Python | No 3D rendering capabilities |

---

**Status**: Proposed → Accepted (2026-08-13)

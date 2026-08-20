# 3D AI DeskMate

Unity 3D AI Assistant with VRM avatar, real-time chat, TTS, and idle expressions.

## Quick Start

1. Copy the backend: `.\scripts\sync-backend.ps1`
2. Install backend dependencies: `cd backend && pip install -r requirements.txt`
3. Set Groq API key: `copy .env.example .env` then edit `.env`
4. Start backend: `.\scripts\start-backend.ps1`
5. Open Unity and press Play

## Project Structure

- `Assets/Scripts/` - Unity C# scripts
- `Assets/Models/` - VRM avatar
- `backend/` - FastAPI WebSocket server (synced from Obsidian)
- `docs/` - Architecture and status docs
- `scripts/` - Helper scripts

## Backend

The backend provides AI chat with emotion-aware responses and voice.

```powershell
# Install
pip install -r backend/requirements.txt

# Start
python backend/server.py
```

## VRM Avatar

Uses UniVRM `0.131.2`. Avatar face is locked forward.

## High Impact Features (Latest)

- `VRMLookAtHead` enabled after stabilization (2s delay)
- `VRMSpringBone` enabled after stabilization (2s delay)
- Chat history with color coding (user/AI messages)
- Loading/status indicators in UI
- Improved idle animations (weight shift, micro-expressions)

## Known Limitations

- `VRMLookAtBoneApplyer` remains disabled to prevent vertex distortion
- Character faces away from camera by default (Unity camera setup)

## License

Private project. All rights reserved.
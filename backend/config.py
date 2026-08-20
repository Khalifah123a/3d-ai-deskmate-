from pydantic_settings import BaseSettings
from typing import Optional

class Settings(BaseSettings):
    LLM_PROVIDER: str = "groq"
    GROQ_API_KEY: str = ""
    GROQ_MODEL: str = "llama-3.1-8b-instant"
    OLLAMA_BASE_URL: str = "http://localhost:11434/v1"
    OLLAMA_MODEL: str = "llama3:2"
    
    TTS_VOICE: str = "en-US-AvaNeural"
    TTS_RATE: str = "+0%"
    TTS_VOLUME: str = "+0%"
    
    SERVER_HOST: str = "0.0.0.0"
    SERVER_PORT: int = 8000
    
    MEMORY_ENABLED: bool = True
    MEMORY_COLLECTION: str = "ai_assistant_conversations"
    
    CHARACTER_NAME: str = "AI Assistant"
    CHARACTER_PERSONALITY: str = "Friendly, helpful, and expressive"
    SYSTEM_PROMPT: str = "You are a friendly, helpful, and expressive 3D AI Assistant. Keep your answers natural, concise, and suitable for spoken dialogue."

    class Config:
        env_file = ".env"
        env_file_encoding = 'utf-8'

settings = Settings()

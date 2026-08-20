from pydantic import BaseModel
from typing import List, Dict, Optional

class VisemeData(BaseModel):
    time: float
    viseme: str

class AIResponse(BaseModel):
    event: str = "ai_response"
    text: str
    audio_url: Optional[str] = None
    expression: Optional[str] = "neutral"
    viseme_data: Optional[List[VisemeData]] = None
    metadata: Optional[Dict[str, str]] = None

class UserMessage(BaseModel):
    event: str = "user_message"
    message: str

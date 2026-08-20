import os
from openai import OpenAI
from typing import List, Dict, Any, Optional
from dotenv import load_dotenv

load_dotenv(override=True)

from .base import LLMProvider

class GroqProvider(LLMProvider):
    def __init__(self):
        self.client = OpenAI(
            base_url="https://api.groq.com/openai/v1",
            api_key=os.getenv("GROQ_API_KEY")
        )
        self.model = os.getenv("GROQ_MODEL", "llama-3.1-8b-instant")

    def chat(self, messages: List[Dict[str, str]], system_prompt: str) -> str:
        response = self.client.chat.completions.create(
            model=self.model,
            messages=[
                {"role": "system", "content": system_prompt}
            ] + messages,
            temperature=0.7,
            max_tokens=1024
        )
        return response.choices[0].message.content

    def chat_with_tools(self, messages: List[Dict[str, str]], tools: List[Dict[str, Any]], tool_calls: Optional[List[Dict[str, Any]]] = None) -> Dict[str, Any]:
        # Groq doesn't natively support tool calls; simulate or use fallback
        # For this MVP, return simple chat response
        return {
            "content": self.chat(messages, system_prompt),
            "tool_calls": []
        }

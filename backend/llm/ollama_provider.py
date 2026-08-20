import os
from openai import OpenAI
from typing import List, Dict, Any, Optional
from .base import LLMProvider

class OllamaProvider(LLMProvider):
    def __init__(self):
        self.client = OpenAI(
            base_url=os.getenv("OLLAMA_BASE_URL", "http://localhost:11434/v1"),
            api_key="ollama"
        )
        self.model = os.getenv("OLLAMA_MODEL", "llama3:2")

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
        # Ollama doesn't support tool calls natively; return chat response
        return {
            "content": self.chat(messages, system_prompt),
            "tool_calls": []
        }

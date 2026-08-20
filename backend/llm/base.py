from abc import ABC, abstractmethod
from typing import Optional, Dict, Any

class LLMProvider(ABC):
    @abstractmethod
    def chat(self, messages: list, system_prompt: str) -> str:
        """
        Generate a response from the LLM.
        """
        pass

    @abstractmethod
    def chat_with_tools(self, messages: list, tools: list, tool_calls: Optional[list] = None) -> Dict[str, Any]:
        """
        Generate a response with tool calls.
        """
        pass

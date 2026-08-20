import chromadb
from chromadb.config import Settings
from typing import List, Dict, Optional

class MemoryStore:
    def __init__(self, collection_name: str = "ai_assistant_conversations"):
        self.client = chromadb.Client(Settings(anonymized_telemetry=False))
        self.collection = self.client.get_or_create_collection(name=collection_name)

    def add_conversation(self, text: str, role: str, metadata: Optional[Dict] = None):
        id = f"{role}_{self.collection.count()}"
        meta = metadata or {}
        meta["role"] = role
        self.collection.add(
            documents=[text],
            metadatas=[meta],
            ids=[id]
        )

    def query_conversation(self, query_text: str, n_results: int = 5) -> List[Dict]:
        results = self.collection.query(
            query_texts=[query_text],
            n_results=n_results
        )
        return results["metadatas"][0] if results["metadatas"] else []

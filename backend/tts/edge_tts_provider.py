import edge_tts
import os

async def generate_tts(text: str, output_file: str, voice: str = "en-US-AvaNeural") -> str:
    communicate = edge_tts.Communicate(text, voice)
    await communicate.save(output_file)
    return output_file

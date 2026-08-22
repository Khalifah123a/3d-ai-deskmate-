import edge_tts
import os
import re

# Voice mapping for different languages
VOICE_MAP = {
    "en": "en-US-JennyNeural",   # Mommy ASMR voice - soft, warm
    "id": "id-ID-GadisNeural",   # Indonesian female voice
}

async def generate_tts(text: str, output_file: str, voice: str = "en-US-JennyNeural") -> str:
    # Auto-detect language and switch voice
    detected_lang = detect_language(text)
    
    # If detected language has a voice, use it
    if detected_lang in VOICE_MAP:
        voice = VOICE_MAP[detected_lang]
    
    # Add SSML prosody for mommy ASMR effect (slower, softer)
    if detected_lang == "en":
        # Wrap in SSML with gentle prosody
        ssml_text = f"""<speak>
            <prosody rate="-15%" pitch="+5%">
                {text}
            </prosody>
        </speak>"""
        communicate = edge_tts.Communicate(ssml_text, voice)
    else:
        communicate = edge_tts.Communicate(text, voice)
    
    await communicate.save(output_file)
    return output_file


def detect_language(text: str) -> str:
    """Detect if text is Indonesian or English."""
    text = text.lower().strip()
    
    # Indonesian common words/patterns
    id_patterns = [
        r'\b(saya|aku|kamu|anda|kami|mereka)\b',
        r'\b(ada|adalah|akan|belum|bisa)\b',
        r'\b(dan|dengan|dari|di|ke)\b',
        r'\b(haloo?|hai|hey|pagi|siang|malam)\b',
        r'\b(baik|buruk|bagus|jelek|suka|cinta)\b',
        r'\b(maaf|tolong|terima\s+kasih|sama-sama)\b',
        r'\b(kenapa|apa|siapa|kapan|dimana|bagaimana)\b',
        r'\b(ini|itu|yang|untuk|pada|dalam)\b',
        r'\b(enggak|gak|nggak|ngga|gak|tidak|bukan)\b',
        r'\b(sedang|lagi|mau|ingin|butuh)\b',
    ]
    
    id_score = 0
    for pattern in id_patterns:
        if re.search(pattern, text):
            id_score += 1
    
    # If score >= 2, likely Indonesian
    if id_score >= 2:
        return "id"
    
    # Default to English
    return "en"

import json


def get_weather(city: str) -> str:
    """Get current weather for a city."""
    return f"Weather in {city}: Sunny, 28\u00b0C"


def execute_system_command(command: str) -> str:
    """Execute a system command or open an app."""
    if command.startswith("open_app "):
        app_name = command.split(" ", 1)[1]
        return f"Opening {app_name}..."
    return f"Command executed: {command}"


TOOLS_SCHEMA = [
    {
        "type": "function",
        "function": {
            "name": "get_weather",
            "description": "Get current weather for a city",
            "parameters": {
                "type": "object",
                "properties": {
                    "city": {"type": "string", "description": "City name"}
                },
                "required": ["city"]
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "execute_system_command",
            "description": "Execute a system command or open an app",
            "parameters": {
                "type": "object",
                "properties": {
                    "command": {"type": "string", "description": "Command to execute (e.g. 'open_app spotify')"}
                },
                "required": ["command"]
            }
        }
    }
]

TOOL_MAP = {
    "get_weather": get_weather,
    "execute_system_command": execute_system_command,
}


def execute_tool(func_name: str, params: dict) -> str:
    if func_name not in TOOL_MAP:
        return f"Unknown function: {func_name}"
    return TOOL_MAP[func_name](**params)


def get_tools_json() -> str:
    return json.dumps(TOOLS_SCHEMA, indent=2)

import json
import os
import subprocess
import platform


def get_weather(city: str) -> str:
    """Get current weather for a city."""
    return f"Weather in {city}: Sunny, 28°C"


def execute_system_command(command: str) -> str:
    """Execute a system command or open an app."""
    system = platform.system()
    try:
        if command.startswith("open_app "):
            app_name = command.split(" ", 1)[1].strip()
            if system == "Windows":
                subprocess.Popen(["start", app_name], shell=True)
                return f"✅ Membuka {app_name}..."
            elif system == "Linux":
                subprocess.Popen([app_name])
                return f"✅ Membuka {app_name}..."
            elif system == "Darwin":
                subprocess.Popen(["open", "-a", app_name])
                return f"✅ Membuka {app_name}..."
            return f"Membuka {app_name}..."

        if command.startswith("open_url "):
            url = command.split(" ", 1)[1].strip()
            if system == "Windows":
                subprocess.Popen(["start", url], shell=True)
            elif system == "Linux":
                subprocess.Popen(["xdg-open", url])
            elif system == "Darwin":
                subprocess.Popen(["open", url])
            return f"✅ Membuka {url}"

        if command.startswith("open_folder "):
            path = command.split(" ", 1)[1].strip()
            if system == "Windows":
                subprocess.Popen(["explorer", path])
            elif system == "Linux":
                subprocess.Popen(["xdg-open", path])
            elif system == "Darwin":
                subprocess.Popen(["open", path])
            return f"✅ Membuka folder {path}"

        # Run shell command
        result = subprocess.run(
            command, shell=True, capture_output=True, text=True, timeout=15
        )
        output = result.stdout.strip()
        if result.returncode != 0:
            output = result.stderr.strip() or "Command failed"
        return output[:500] if output else "✅ Command executed (no output)"

    except subprocess.TimeoutExpired:
        return "⏰ Command timed out (15s limit)"
    except Exception as e:
        return f"❌ Error: {str(e)}"


def get_system_info() -> str:
    """Get system information about the host machine."""
    info = {
        "os": platform.system(),
        "os_version": platform.version(),
        "machine": platform.machine(),
        "processor": platform.processor(),
        "python": platform.python_version(),
    }
    return json.dumps(info, indent=2)


def open_application(app_name: str) -> str:
    """Open an application by name."""
    return execute_system_command(f"open_app {app_name}")


def open_url(url: str) -> str:
    """Open a URL in the default browser."""
    return execute_system_command(f"open_url {url}")


def open_folder(path: str) -> str:
    """Open a folder in the file explorer."""
    return execute_system_command(f"open_folder {path}")


def list_files(path: str = ".") -> str:
    """List files in a directory."""
    try:
        files = os.listdir(path)
        return "\n".join(files[:30])
    except Exception as e:
        return f"❌ Error: {e}"


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
                "required": ["city"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "execute_system_command",
            "description": "Execute a system command or open an app (e.g. 'open_app spotify', 'open_url https://google.com', 'open_folder Documents')",
            "parameters": {
                "type": "object",
                "properties": {
                    "command": {
                        "type": "string",
                        "description": "Command to execute",
                    }
                },
                "required": ["command"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "get_system_info",
            "description": "Get system information about the host machine (OS, version, hardware)",
            "parameters": {
                "type": "object",
                "properties": {},
                "required": [],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "open_application",
            "description": "Open an application by name (e.g. 'spotify', 'chrome', 'notepad')",
            "parameters": {
                "type": "object",
                "properties": {
                    "app_name": {
                        "type": "string",
                        "description": "Application name to open",
                    }
                },
                "required": ["app_name"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "open_url",
            "description": "Open a URL in the default browser",
            "parameters": {
                "type": "object",
                "properties": {
                    "url": {"type": "string", "description": "URL to open"}
                },
                "required": ["url"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "open_folder",
            "description": "Open a folder in the file explorer",
            "parameters": {
                "type": "object",
                "properties": {
                    "path": {
                        "type": "string",
                        "description": "Folder path to open",
                    }
                },
                "required": ["path"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "list_files",
            "description": "List files in a directory",
            "parameters": {
                "type": "object",
                "properties": {
                    "path": {
                        "type": "string",
                        "description": "Directory path (default: current directory)",
                    }
                },
                "required": [],
            },
        },
    },
]

TOOL_MAP = {
    "get_weather": get_weather,
    "execute_system_command": execute_system_command,
    "get_system_info": get_system_info,
    "open_application": open_application,
    "open_url": open_url,
    "open_folder": open_folder,
    "list_files": list_files,
}


def execute_tool(func_name: str, params: dict) -> str:
    if func_name not in TOOL_MAP:
        return f"Unknown function: {func_name}"
    return TOOL_MAP[func_name](**params)


def get_tools_json() -> str:
    return json.dumps(TOOLS_SCHEMA, indent=2)

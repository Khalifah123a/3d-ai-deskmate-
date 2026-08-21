import psutil
import platform
from datetime import datetime

_start_time = datetime.now()


def get_server_status():
    """Get comprehensive server status."""
    return {
        "uptime_seconds": int((datetime.now() - _start_time).total_seconds()),
        "uptime_human": _format_uptime((datetime.now() - _start_time).total_seconds()),
        "cpu_percent": psutil.cpu_percent(interval=0.1),
        "memory": {
            "total_gb": round(psutil.virtual_memory().total / (1024**3), 1),
            "used_gb": round(psutil.virtual_memory().used / (1024**3), 1),
            "percent": psutil.virtual_memory().percent,
        },
        "disk": {
            "total_gb": round(psutil.disk_usage("/").total / (1024**3), 1),
            "used_gb": round(psutil.disk_usage("/").used / (1024**3), 1),
            "percent": psutil.disk_usage("/").percent,
        },
        "platform": platform.system(),
        "python_version": platform.python_version(),
        "hostname": platform.node(),
    }


def _format_uptime(seconds):
    days = int(seconds // 86400)
    hours = int((seconds % 86400) // 3600)
    minutes = int((seconds % 3600) // 60)
    if days > 0:
        return f"{days}d {hours}h {minutes}m"
    if hours > 0:
        return f"{hours}h {minutes}m"
    return f"{minutes}m"

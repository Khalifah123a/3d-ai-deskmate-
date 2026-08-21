let refreshInterval;

// Initial load
document.addEventListener("DOMContentLoaded", () => {
    fetchStatus();
    fetchChatHistory();
    refreshInterval = setInterval(fetchStatus, 5000);
});

async function fetchStatus() {
    try {
        const res = await fetch("/api/status");
        const data = await res.json();

        document.getElementById("uptime").textContent = data.uptime_human;
        document.getElementById("cpu").textContent = data.cpu_percent + "%";
        document.getElementById("memory").textContent =
            data.memory.used_gb + "/" + data.memory.total_gb + " GB (" + data.memory.percent + "%)";
        document.getElementById("disk").textContent =
            data.disk.used_gb + "/" + data.disk.total_gb + " GB (" + data.disk.percent + "%)";
        document.getElementById("llmProvider").textContent = data.llm_provider;
        document.getElementById("memoryDb").textContent = data.memory_status;
        document.getElementById("platform").textContent = data.platform;
        document.getElementById("wsClients").textContent = data.ws_clients;

        const badge = document.getElementById("statusBadge");
        badge.textContent = "Online";
        badge.className = "status-badge online";

        addLog("Status refreshed: CPU " + data.cpu_percent + "%");
    } catch (e) {
        const badge = document.getElementById("statusBadge");
        badge.textContent = "Offline";
        badge.className = "status-badge offline";
    }
}

async function fetchChatHistory() {
    try {
        const res = await fetch("/api/chat/history");
        const data = await res.json();
        const chatLog = document.getElementById("chatLog");

        if (data.messages && data.messages.length > 0) {
            chatLog.innerHTML = "";
            data.messages.slice(-20).forEach((msg) => {
                const div = document.createElement("div");
                div.className = "chat-msg " + (msg.role === "user" ? "user" : "ai");
                div.innerHTML = msg.text + '<div class="time">' + msg.time + '</div>';
                chatLog.appendChild(div);
            });
            chatLog.scrollTop = chatLog.scrollHeight;
        }
    } catch (e) {
        console.log("Could not fetch chat history");
    }
}

function restartServer() {
    if (confirm("Restart the backend server?")) {
        fetch("/api/control/restart", { method: "POST" });
        addLog("Server restart requested");
        setTimeout(() => location.reload(), 3000);
    }
}

function clearHistory() {
    if (confirm("Clear all chat history?")) {
        fetch("/api/control/clear", { method: "POST" });
        addLog("Chat history cleared");
        document.getElementById("chatLog").innerHTML =
            '<div class="empty">Chat history cleared</div>';
    }
}

function addLog(message) {
    const log = document.getElementById("activityLog");
    const entry = document.createElement("div");
    entry.className = "log-entry";
    const time = new Date().toLocaleTimeString();
    entry.textContent = "[" + time + "] " + message;
    log.appendChild(entry);
    log.scrollTop = log.scrollHeight;

    // Keep only last 50 entries
    while (log.children.length > 50) {
        log.removeChild(log.firstChild);
    }
}

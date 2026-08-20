using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class WebSocketClient : MonoBehaviour
{
    private ClientWebSocket _ws;
    private CancellationTokenSource _cts;
    private bool _isConnected = false;
    private int _reconnectAttempts;
    private const int MAX_DELAY = 10;

    public string serverUrl = "ws://localhost:8000/ws";

    private UIManager _uiManager;
    private AudioManager _audioManager;
    private ExpressionController _expressionController;

    void Start()
    {
        _uiManager = FindAnyObjectByType<UIManager>();
        _audioManager = FindAnyObjectByType<AudioManager>();
        StartCoroutine(ConnectLoop());
    }

    private System.Collections.IEnumerator ConnectLoop()
    {
        while (enabled && gameObject.activeInHierarchy)
        {
            if (_isConnected)
            {
                yield return new WaitForSeconds(2f);
                continue;
            }

            _reconnectAttempts++;
            int delay = Mathf.Min(_reconnectAttempts * 2, MAX_DELAY);
            Debug.Log("[WS] Connecting... attempt " + _reconnectAttempts);

            if (_uiManager != null)
                _uiManager.UpdateConnectionStatus(false);

            _ = DoConnect();

            yield return new WaitForSeconds(delay + 1f);
        }
    }

    private async Task DoConnect()
    {
        try
        {
            if (_ws != null)
            {
                try { _ws.Abort(); } catch { }
                _ws = null;
            }

            _cts = new CancellationTokenSource();
            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(15);

            await _ws.ConnectAsync(new Uri(serverUrl), _cts.Token);

            _isConnected = true;
            _reconnectAttempts = 0;
            Debug.Log("[WS] Connected!");

            if (_uiManager != null)
                _uiManager.UpdateConnectionStatus(true);

            await ReceiveLoop();
        }
        catch (OperationCanceledException) { Debug.Log("[WS] Connect cancelled"); }
        catch (Exception e)
        {
            Debug.LogWarning("[WS] Connect error: " + e.Message);
            _isConnected = false;
            if (_uiManager != null)
                _uiManager.UpdateConnectionStatus(false);
        }
    }

    private async Task ReceiveLoop()
    {
        var buffer = new byte[65536];

        try
        {
            while (_ws != null && _ws.State == WebSocketState.Open)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log("[WS] Received: " + json.Substring(0, Mathf.Min(json.Length, 200)));
                    ProcessMessage(json);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("[WS] Server closed");
                    break;
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
        catch (WebSocketException e) { Debug.LogWarning("[WS] Error: " + e.Message); }
        catch (Exception e) { Debug.LogWarning("[WS] Error: " + e.GetType().Name + ": " + e.Message); }

        _isConnected = false;
        if (_uiManager != null)
            _uiManager.UpdateConnectionStatus(false);
        Debug.Log("[WS] Disconnected, will reconnect...");
    }

    private void ProcessMessage(string json)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<ServerResponse>(json);
            if (response == null) return;

            if (_uiManager != null)
                _uiManager.DisplayAIResponse(response.text ?? "", response.audio_url);

            if (_expressionController == null)
                _expressionController = FindAnyObjectByType<ExpressionController>();

            if (_expressionController != null && !string.IsNullOrEmpty(response.expression))
                _expressionController.SetExpression(response.expression);
        }
        catch (Exception e)
        {
            Debug.LogError("[WS] Parse error: " + e.Message);
        }
    }

    public void SendToServer(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (_ws == null || _ws.State != WebSocketState.Open)
        {
            Debug.LogWarning("[WS] Not connected");
            return;
        }

        try
        {
            var payload = new { message = text };
            string json = JsonConvert.SerializeObject(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            _ = _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("[WS] Sent: " + json);
        }
        catch (Exception e)
        {
            Debug.LogError("[WS] Send error: " + e.Message);
            _isConnected = false;
        }
    }

    void OnApplicationQuit()
    {
        _isConnected = false;
        try { _cts?.Cancel(); } catch { }
        try { _ws?.Abort(); } catch { }
    }

    [Serializable]
    private class ServerResponse
    {
        public string text;
        public string audio_url;
        public string expression;
    }
}

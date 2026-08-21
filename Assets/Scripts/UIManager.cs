using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public InputField legacyInputField;
    public Button legacySendButton;
    public Text legacyResponseText;
    public Text legacyStatusText;
    public ScrollRect chatScroll;

    private WebSocketClient _webSocket;
    private AudioManager _audioManager;
    private VRMIdleAnimator _idleAnimator;
    private bool _isProcessing = false;
    private string _typingDots = "";
    private float _typingTimer;

    void Start()
    {
        _webSocket = FindAnyObjectByType<WebSocketClient>();
        _audioManager = FindAnyObjectByType<AudioManager>();
        _idleAnimator = FindAnyObjectByType<VRMIdleAnimator>();

        if (legacySendButton != null)
            legacySendButton.onClick.AddListener(SendChatMessage);

        if (legacyInputField != null)
            legacyInputField.Select();

        if (legacyResponseText != null)
            legacyResponseText.text = "";
    }

    void Update()
    {
        // Typing indicator animation
        if (_isProcessing)
        {
            _typingTimer += Time.deltaTime;
            if (_typingTimer >= 0.5f)
            {
                _typingTimer = 0f;
                _typingDots = (_typingDots.Length >= 3) ? "" : _typingDots + ".";
                UpdateStatus("Thinking" + _typingDots);
            }
        }

        // Send on Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!_isProcessing && legacyInputField != null && !string.IsNullOrEmpty(legacyInputField.text.Trim()))
                SendChatMessage();
        }
    }

    public void SendChatMessage()
    {
        if (legacyInputField == null || _webSocket == null) return;

        string message = legacyInputField.text.Trim();
        if (string.IsNullOrEmpty(message) || _isProcessing) return;

        DisplayUserMessage(message);
        _webSocket.SendToServer(message);
        legacyInputField.text = "";
        legacyInputField.Select();
        legacyInputField.ActivateInputField();
        UpdateStatus("Thinking" + _typingDots);
        _isProcessing = true;
        
        // Signal thinking to avatar
        if (_idleAnimator != null)
            _idleAnimator.SetThinking(true);
    }

    public void DisplayUserMessage(string text)
    {
        if (legacyResponseText != null)
        {
            legacyResponseText.supportRichText = true;
            legacyResponseText.text += "<color=#FFFFFF><b>You:</b> " + text + "</color>\n";
        }
        ScrollToBottom();
    }

    public void DisplayAIResponse(string text, string audioUrl)
    {
        if (legacyResponseText != null)
        {
            legacyResponseText.supportRichText = true;
            legacyResponseText.text += "<color=#7EC8E3><b>AI:</b> " + text + "</color>\n";
        }
        ScrollToBottom();

        if (!string.IsNullOrEmpty(audioUrl) && _audioManager != null)
            _audioManager.PlayAudio(audioUrl);

        UpdateStatus("Ready");
        _isProcessing = false;
        _typingDots = "";
        
        // Stop thinking animation
        if (_idleAnimator != null)
            _idleAnimator.SetThinking(false);
    }

    public void UpdateConnectionStatus(bool connected)
    {
        if (legacyStatusText != null)
        {
            legacyStatusText.text = connected ? "✓ Connected" : "✗ Disconnected";
            legacyStatusText.color = connected ? Color.green : Color.red;
        }
    }

    public void UpdateStatus(string status)
    {
        if (legacyStatusText != null)
            legacyStatusText.text = status;
    }

    void ScrollToBottom()
    {
        if (chatScroll != null)
        {
            Canvas.ForceUpdateCanvases();
            chatScroll.verticalNormalizedPosition = 0f;
        }
    }
}

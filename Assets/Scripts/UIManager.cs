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
    private ExpressionPresets _expressionPresets;
    private VoiceInputManager _voiceInput;
    private ChatPersistence _persistence;
    private bool _isProcessing = false;
    private string _typingDots = "";
    private float _typingTimer;

    void Start()
    {
        _webSocket = FindAnyObjectByType<WebSocketClient>();
        _audioManager = FindAnyObjectByType<AudioManager>();
        _idleAnimator = FindAnyObjectByType<VRMIdleAnimator>();
        _expressionPresets = FindAnyObjectByType<ExpressionPresets>();
        _voiceInput = FindAnyObjectByType<VoiceInputManager>();
        _persistence = FindAnyObjectByType<ChatPersistence>();

        if (legacySendButton != null)
            legacySendButton.onClick.AddListener(SendChatMessage);

        if (legacyInputField != null)
            legacyInputField.Select();

        // Restore chat history on startup
        if (legacyResponseText != null && _persistence != null && _persistence.MessageCount > 0)
        {
            legacyResponseText.supportRichText = true;
            legacyResponseText.text = _persistence.GetChatHistoryAsText();
            ScrollToBottom();
        }
        else if (legacyResponseText != null)
        {
            legacyResponseText.text = "";
        }
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

        if (_persistence != null)
            _persistence.AddMessage("user", text);
    }

    public void DisplayAIResponse(string text, string audioUrl)
    {
        if (legacyResponseText != null)
        {
            legacyResponseText.supportRichText = true;
            legacyResponseText.text += "<color=#7EC8E3><b>AI:</b> " + text + "</color>\n";
        }
        ScrollToBottom();

        if (_persistence != null)
            _persistence.AddMessage("ai", text);

        if (!string.IsNullOrEmpty(audioUrl) && _audioManager != null)
            _audioManager.PlayAudio(audioUrl);

        // React to AI response with expression presets
        if (_expressionPresets != null)
        {
            // Extract emotion from response if available
            string emotion = ExtractEmotionFromResponse(text);
            if (!string.IsNullOrEmpty(emotion))
                _expressionPresets.SetMoodFromEmotion(emotion);
            else
                _expressionPresets.ReactToKeyword(text);
        }

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
    
    private string ExtractEmotionFromResponse(string text)
    {
        // Try to extract emotion tag from response
        if (string.IsNullOrEmpty(text)) return null;
        
        string[] emotions = { "happy", "sad", "angry", "surprised", "neutral" };
        string lowerText = text.ToLower();
        
        foreach (string emotion in emotions)
        {
            if (lowerText.Contains("[" + emotion + "]") || lowerText.Contains(emotion))
                return emotion;
        }
        
        return null;
    }
    
    public void ToggleVoiceRecording()
    {
        if (_voiceInput == null)
        {
            _voiceInput = FindAnyObjectByType<VoiceInputManager>();
            if (_voiceInput == null)
            {
                Debug.Log("[UI] VoiceInputManager not found");
                return;
            }
        }
        
        if (_voiceInput.IsRecording)
            _voiceInput.StopRecording();
        else
            _voiceInput.StartRecording();
    }
}

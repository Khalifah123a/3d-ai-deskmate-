using UnityEngine;
using UnityEngine.UI;
using System;

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
    private HandGestures _handGestures;
    private DanceAnimations _danceAnimations;
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
        _handGestures = FindAnyObjectByType<HandGestures>();
        _danceAnimations = FindAnyObjectByType<DanceAnimations>();

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

        // Handle slash commands
        if (message.StartsWith("/"))
        {
            HandleSlashCommand(message.Substring(1).Trim());
            legacyInputField.text = "";
            return;
        }

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

    private void HandleSlashCommand(string command)
    {
        string[] parts = command.Split(' ');
        string cmd = parts[0].ToLower();
        string arg = parts.Length > 1 ? string.Join(" ", parts[1..]) : "";

        switch (cmd)
        {
            case "help":
                DisplaySlashHelp();
                break;
            case "gesture":
            case "g":
                HandleGesture(arg);
                break;
            case "dance":
            case "d":
                HandleDance(arg);
                break;
            case "expression":
            case "e":
                HandleExpression(arg);
                break;
            case "reset":
                ResetAll();
                break;
            default:
                DisplayUserMessage("/" + command + " — Perintah tidak dikenali. Ketik /help untuk daftar.");
                break;
        }
    }

    private void HandleGesture(string gestureName)
    {
        if (_handGestures == null)
        {
            DisplayUserMessage("[Gesture] HandGestures not found");
            return;
        }

        if (string.IsNullOrEmpty(gestureName))
        {
            DisplayUserMessage("[Gesture] Gunakan: /gesture [thumbsup | ok | point | wave | prayer | raised | clap | heart]");
            return;
        }

        _handGestures.TriggerGestureByName(gestureName);
        DisplayUserMessage("[Gesture] " + gestureName + " — done!");
    }

    private void HandleDance(string danceName)
    {
        if (_danceAnimations == null)
        {
            DisplayUserMessage("[Dance] DanceAnimations not found");
            return;
        }

        if (string.IsNullOrEmpty(danceName))
        {
            DisplayUserMessage("[Dance] Gunakan: /dance [bounce | happy | excited | groove | victory]");
            return;
        }

        DanceAnimations.DanceStyle style = DanceAnimations.DanceStyle.Happy;
        if (Enum.TryParse(danceName, true, out var parsed))
            style = parsed;

        _danceAnimations.StartDancing(style, 5f);
        DisplayUserMessage("[Dance] " + danceName + " — mulai!");
    }

    private void HandleExpression(string exprName)
    {
        if (_expressionPresets == null)
        {
            DisplayUserMessage("[Expr] ExpressionPresets not found");
            return;
        }

        if (string.IsNullOrEmpty(exprName))
        {
            DisplayUserMessage("[Expr] Gunakan: /expression [happy | sad | angry | surprised | neutral | confused | excited]");
            return;
        }

        _expressionPresets.SetMoodFromEmotion(exprName);
        DisplayUserMessage("[Expr] " + exprName + " — done!");
    }

    private void ResetAll()
    {
        if (_handGestures != null)
            _handGestures.TriggerGesture(HandGestures.GestureType.None);
        if (_danceAnimations != null)
            _danceAnimations.StopDancing();
        if (_idleAnimator != null)
            _idleAnimator.SetMood("neutral");
        if (_expressionPresets != null)
            _expressionPresets.SetMood(ExpressionPresets.Mood.Neutral);

        DisplayUserMessage("[System] Semua animasi di-reset ke neutral");
    }

    private void DisplaySlashHelp()
    {
        string help = "<color=#FFD700><b>/help</b></color> — Tampilkan bantuan\n" +
                      "<color=#FFD700><b>/gesture [name]</b></color> — Gesture tangan\n" +
                      "  • thumbsup, ok, point, wave, prayer, raised, clap, heart\n" +
                      "<color=#FFD700><b>/dance [style]</b></color> — Tarian\n" +
                      "  • bounce, happy, excited, groove, victory\n" +
                      "<color=#FFD700><b>/expression [mood]</b></color> — Ekspresi wajah\n" +
                      "  • happy, sad, angry, surprised, neutral, confused, excited\n" +
                      "<color=#FFD700><b>/reset</b></color> — Reset semua animasi";
        DisplayUserMessage(help);
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

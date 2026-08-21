using UnityEngine;
using VRM;
using System.Collections;
using System.Collections.Generic;

public class ExpressionPresets : MonoBehaviour
{
    private ExpressionController _expressionController;
    private VRMBlendShapeProxy _proxy;
    private Coroutine _idleCoroutine;
    
    // Mood states
    public enum Mood
    {
        Neutral,
        Happy,
        Excited,
        Thoughtful,
        Concerned,
        Sleepy
    }
    
    private Mood _currentMood = Mood.Neutral;
    private float _moodTimer;
    private float _nextMoodChange;
    
    // Keyword reactions
    private readonly Dictionary<string, Mood> _keywordMoods = new Dictionary<string, Mood>
    {
        { "halo", Mood.Happy },
        { "hai", Mood.Happy },
        { "terima kasih", Mood.Happy },
        { "bagus", Mood.Happy },
        { "hebat", Mood.Excited },
        { "keren", Mood.Excited },
        { "tolong", Mood.Concerned },
        { "masalah", Mood.Concerned },
        { "bantuan", Mood.Concerned },
        { "bosan", Mood.Sleepy },
        { "mengantuk", Mood.Sleepy },
        { "capek", Mood.Sleepy },
        { "pikir", Mood.Thoughtful },
        { "mengerti", Mood.Thoughtful },
        { "cerita", Mood.Thoughtful }
    };
    
    void Awake()
    {
        _expressionController = GetComponent<ExpressionController>();
        _proxy = GetComponent<VRMBlendShapeProxy>();
        
        if (_expressionController == null)
            _expressionController = FindAnyObjectByType<ExpressionController>();
        if (_proxy == null)
            _proxy = FindAnyObjectByType<VRMBlendShapeProxy>();
    }
    
    void Start()
    {
        _nextMoodChange = Random.Range(8f, 15f);
        StartIdleCycle();
    }
    
    void Update()
    {
        // Auto-change mood periodically
        _moodTimer += Time.deltaTime;
        if (_moodTimer >= _nextMoodChange && _idleCoroutine == null)
        {
            _moodTimer = 0f;
            _nextMoodChange = Random.Range(8f, 15f);
            RandomMoodChange();
        }
    }
    
    public void SetMood(Mood mood)
    {
        _currentMood = mood;
        
        if (_expressionController == null) return;
        
        switch (mood)
        {
            case Mood.Happy:
                _expressionController.SetExpression("happy", 0.6f);
                break;
            case Mood.Excited:
                _expressionController.SetExpression("happy", 0.8f);
                break;
            case Mood.Thoughtful:
                _expressionController.SetExpression("neutral", 0.3f);
                break;
            case Mood.Concerned:
                _expressionController.SetExpression("sad", 0.4f);
                break;
            case Mood.Sleepy:
                _expressionController.SetExpression("neutral", 0.2f);
                break;
            default:
                _expressionController.SetExpression("neutral", 0.3f);
                break;
        }
        
        Debug.Log("[Expr] Mood changed to: " + mood);
    }
    
    public void ReactToKeyword(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        
        string lowerMessage = message.ToLower();
        
        foreach (var kvp in _keywordMoods)
        {
            if (lowerMessage.Contains(kvp.Key))
            {
                SetMood(kvp.Value);
                Debug.Log("[Expr] Keyword detected: " + kvp.Key + " -> Mood: " + kvp.Value);
                return;
            }
        }
    }
    
    private void StartIdleCycle()
    {
        if (_idleCoroutine != null)
            StopCoroutine(_idleCoroutine);
        _idleCoroutine = StartCoroutine(IdleCycleRoutine());
    }
    
    private IEnumerator IdleCycleRoutine()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(_nextMoodChange);
            
            if (_currentMood == Mood.Neutral)
            {
                // Subtle mood variations for idle state
                int variation = Random.Range(0, 3);
                switch (variation)
                {
                    case 0:
                        SetMood(Mood.Happy);
                        yield return new WaitForSeconds(3f);
                        SetMood(Mood.Neutral);
                        break;
                    case 1:
                        SetMood(Mood.Thoughtful);
                        yield return new WaitForSeconds(4f);
                        SetMood(Mood.Neutral);
                        break;
                    case 2:
                        // Just stay neutral
                        break;
                }
            }
        }
    }
    
    private void RandomMoodChange()
    {
        if (_currentMood == Mood.Neutral)
        {
            int random = Random.Range(0, 3);
            switch (random)
            {
                case 0: SetMood(Mood.Happy); break;
                case 1: SetMood(Mood.Thoughtful); break;
                default: SetMood(Mood.Neutral); break;
            }
        }
    }
    
    public Mood GetCurrentMood() => _currentMood;
    
    public void SetMoodFromEmotion(string emotion)
    {
        switch (emotion.ToLower())
        {
            case "happy": case "joy": SetMood(Mood.Happy); break;
            case "angry": case "marah": SetMood(Mood.Concerned); break;
            case "sad": case "sedih": SetMood(Mood.Concerned); break;
            case "surprised": case "fun": SetMood(Mood.Excited); break;
            case "neutral": case "biasa": SetMood(Mood.Neutral); break;
            default: SetMood(Mood.Neutral); break;
        }
    }
}

using UnityEngine;
using VRM;
using System.Collections;
using System.Collections.Generic;

public class ExpressionPresets : MonoBehaviour
{
    private ExpressionController _expressionController;
    private VRMBlendShapeProxy _proxy;
    private SceneLighting _sceneLighting;
    private VRMIdleAnimator _idleAnimator;
    private Coroutine _idleCoroutine;
    
    // Mood states
    public enum Mood
    {
        Neutral,
        Happy,
        Excited,
        Thoughtful,
        Concerned,
        Sleepy,
        Angry,
        Confused,
        Sad
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
        { "cerita", Mood.Thoughtful },
        { "marah", Mood.Angry },
        { "kesal", Mood.Angry },
        { "ganggu", Mood.Angry },
        { "bingung", Mood.Confused },
        { "rahasia", Mood.Confused },
        { "menyedihkan", Mood.Sad },
        { "sedih", Mood.Sad },
        { "kecewa", Mood.Sad },
        { "gembira", Mood.Happy },
        { "senang", Mood.Happy },
        { "lucu", Mood.Happy }
    };
    
    public void Initialize(ExpressionController expr)
    {
        _expressionController = expr;
        _proxy = GetComponent<VRMBlendShapeProxy>();
        _sceneLighting = FindAnyObjectByType<SceneLighting>();
        _idleAnimator = GetComponent<VRMIdleAnimator>();
    }
    
    void Awake()
    {
        // Fallback if not initialized via VRMLoader
        if (_expressionController == null)
            _expressionController = GetComponent<ExpressionController>();
        if (_proxy == null)
            _proxy = GetComponent<VRMBlendShapeProxy>();
        if (_sceneLighting == null)
            _sceneLighting = FindAnyObjectByType<SceneLighting>();
        if (_idleAnimator == null)
            _idleAnimator = FindAnyObjectByType<VRMIdleAnimator>();
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
            case Mood.Angry:
                _expressionController.SetExpression("angry", 0.7f);
                break;
            case Mood.Confused:
                _expressionController.SetExpression("neutral", 0.5f);
                break;
            case Mood.Sad:
                _expressionController.SetExpression("sad", 0.6f);
                break;
            default:
                _expressionController.SetExpression("neutral", 0.3f);
                break;
        }
        
        // Update scene lighting based on mood
        if (_sceneLighting != null)
            _sceneLighting.SetMoodFromEmotion(mood.ToString().ToLower());

        // Sync body language with idle animator
        if (_idleAnimator != null)
            _idleAnimator.SetMood(mood.ToString().ToLower());

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
            case "happy": case "joy": case "senang": case "gembira": SetMood(Mood.Happy); break;
            case "angry": case "marah": case "kesal": SetMood(Mood.Angry); break;
            case "sad": case "sedih": case "kecewa": SetMood(Mood.Sad); break;
            case "surprised": case "fun": case "lucu": SetMood(Mood.Excited); break;
            case "confused": case "bingung": SetMood(Mood.Confused); break;
            case "neutral": case "biasa": SetMood(Mood.Neutral); break;
            default: SetMood(Mood.Neutral); break;
        }
    }
}

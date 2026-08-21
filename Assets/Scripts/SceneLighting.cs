using UnityEngine;
using System.Collections;

public class SceneLighting : MonoBehaviour
{
    [Header("Main Light")]
    public Light mainLight;
    public Light fillLight;

    [Header("Day/Night Settings")]
    public float dayIntensity = 1.2f;
    public float nightIntensity = 0.3f;
    public Color dayColor = new Color(1f, 0.95f, 0.85f);
    public Color nightColor = new Color(0.3f, 0.35f, 0.6f);

    [Header("Mood Colors")]
    public Color happyColor = new Color(1f, 0.95f, 0.7f);
    public Color sadColor = new Color(0.5f, 0.55f, 0.75f);
    public Color angryColor = new Color(1f, 0.6f, 0.5f);
    public Color surprisedColor = new Color(1f, 0.9f, 0.95f);
    public Color neutralColor = new Color(0.95f, 0.92f, 0.88f);

    private float _transitionSpeed = 2f;
    private Color _targetColor;
    private float _targetIntensity;

    private ExpressionPresets _presets;
    private ExpressionController _expressionController;
    private float _time;
    private bool _isDaytime = true;

    void Start()
    {
        _presets = FindAnyObjectByType<ExpressionPresets>();
        _expressionController = FindAnyObjectByType<ExpressionController>();

        if (mainLight == null)
            mainLight = FindAnyObjectByType<Light>();
        if (fillLight == null)
        {
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l != mainLight)
                {
                    fillLight = l;
                    break;
                }
            }
        }

        _targetColor = mainLight != null ? mainLight.color : neutralColor;
        _targetIntensity = mainLight != null ? mainLight.intensity : dayIntensity;

        // Start a slow day/night cycle
        StartCoroutine(DayNightCycle());
    }

    void Update()
    {
        _time += Time.deltaTime;

        if (mainLight != null)
        {
            mainLight.color = Color.Lerp(mainLight.color, _targetColor, Time.deltaTime * _transitionSpeed);
            mainLight.intensity = Mathf.Lerp(mainLight.intensity, _targetIntensity, Time.deltaTime * _transitionSpeed);
        }

        if (fillLight != null)
        {
            fillLight.intensity = Mathf.Lerp(fillLight.intensity, _targetIntensity * 0.4f, Time.deltaTime * _transitionSpeed);
        }

        // Check mood changes
        if (_presets != null)
        {
            ApplyMoodColor(_presets.GetCurrentMood());
        }
    }

    private void ApplyMoodColor(ExpressionPresets.Mood mood)
    {
        switch (mood)
        {
            case ExpressionPresets.Mood.Happy:
            case ExpressionPresets.Mood.Excited:
                _targetColor = _isDaytime ? Color.Lerp(dayColor, happyColor, 0.3f) : Color.Lerp(nightColor, happyColor, 0.15f);
                _targetIntensity = _isDaytime ? dayIntensity * 1.1f : nightIntensity * 1.1f;
                break;
            case ExpressionPresets.Mood.Concerned:
                _targetColor = _isDaytime ? Color.Lerp(dayColor, sadColor, 0.2f) : Color.Lerp(nightColor, sadColor, 0.15f);
                _targetIntensity = _isDaytime ? dayIntensity * 0.85f : nightIntensity * 0.9f;
                break;
            case ExpressionPresets.Mood.Thoughtful:
                _targetColor = _isDaytime ? Color.Lerp(dayColor, neutralColor, 0.1f) : nightColor;
                _targetIntensity = _isDaytime ? dayIntensity : nightIntensity;
                break;
            case ExpressionPresets.Mood.Sleepy:
                _targetColor = Color.Lerp(_targetColor, nightColor, 0.3f);
                _targetIntensity = Mathf.Lerp(_targetIntensity, nightIntensity * 0.8f, 0.3f);
                break;
            default:
                _targetColor = _isDaytime ? dayColor : nightColor;
                _targetIntensity = _isDaytime ? dayIntensity : nightIntensity;
                break;
        }
    }

    private IEnumerator DayNightCycle()
    {
        // 5-minute cycle: 2.5 min day, 2.5 min night
        while (enabled)
        {
            _isDaytime = true;
            _targetColor = dayColor;
            _targetIntensity = dayIntensity;
            yield return new WaitForSeconds(150f);

            _isDaytime = false;
            _targetColor = nightColor;
            _targetIntensity = nightIntensity;
            yield return new WaitForSeconds(150f);
        }
    }

    public void SetMoodFromEmotion(string emotion)
    {
        switch (emotion.ToLower())
        {
            case "happy": case "joy":
                _targetColor = happyColor;
                _targetIntensity = dayIntensity * 1.1f;
                break;
            case "sad":
                _targetColor = sadColor;
                _targetIntensity = dayIntensity * 0.8f;
                break;
            case "angry":
                _targetColor = angryColor;
                _targetIntensity = dayIntensity * 1.15f;
                break;
            case "surprised":
                _targetColor = surprisedColor;
                _targetIntensity = dayIntensity * 1.2f;
                break;
            default:
                _targetColor = neutralColor;
                _targetIntensity = dayIntensity;
                break;
        }
    }
}

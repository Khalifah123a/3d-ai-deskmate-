using UnityEngine;
using VRM;
using System.Collections;

public class VRMAnimationBridge : MonoBehaviour
{
    private VRMBlendShapeProxy _blendShapeProxy;
    private Coroutine _expressionCoroutine;
    private bool _hasActiveExpression;
    private BlendShapeKey _activeExpressionKey;

    public void Init(GameObject root)
    {
        _blendShapeProxy = root.GetComponent<VRMBlendShapeProxy>();
        if (_blendShapeProxy == null)
            Debug.LogWarning("[VRM] VRMBlendShapeProxy not found.");
    }

    public void SetExpression(string emotion, float duration = 4.0f, float fadeTime = 0.3f)
    {
        if (_blendShapeProxy == null || string.IsNullOrEmpty(emotion)) return;

        BlendShapeKey targetKey = GetKeyFromEmotion(emotion);

        if (_expressionCoroutine != null)
            StopCoroutine(_expressionCoroutine);

        _expressionCoroutine = StartCoroutine(TransitionToExpressionRoutine(targetKey, duration, fadeTime));
    }

    public void SetViseme(string viseme, float weight = 1f)
    {
        if (_blendShapeProxy == null) return;

        BlendShapePreset preset = GetPresetFromViseme(viseme);
        if (preset == BlendShapePreset.Unknown) return;

        var key = BlendShapeKey.CreateFromPreset(preset);
        _blendShapeProxy.ImmediatelySetValue(key, Mathf.Clamp01(weight));
    }

    public void ResetExpressions(float fadeTime = 0.3f)
    {
        if (_expressionCoroutine != null)
            StopCoroutine(_expressionCoroutine);
        _expressionCoroutine = StartCoroutine(ResetToNeutralRoutine(fadeTime));
    }

    private IEnumerator TransitionToExpressionRoutine(BlendShapeKey targetKey, float duration, float fadeTime)
    {
        if (_hasActiveExpression && !_activeExpressionKey.Equals(targetKey))
        {
            float startVal = _blendShapeProxy.GetValue(_activeExpressionKey);
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                _blendShapeProxy.ImmediatelySetValue(_activeExpressionKey, Mathf.Lerp(startVal, 0f, elapsed / fadeTime));
                yield return null;
            }
            _blendShapeProxy.ImmediatelySetValue(_activeExpressionKey, 0f);
        }

        _activeExpressionKey = targetKey;
        _hasActiveExpression = true;
        float e = 0f;
        while (e < fadeTime)
        {
            e += Time.deltaTime;
            _blendShapeProxy.ImmediatelySetValue(targetKey, Mathf.Lerp(0f, 0.75f, e / fadeTime));
            yield return null;
        }
        _blendShapeProxy.ImmediatelySetValue(targetKey, 0.75f);

        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
            yield return StartCoroutine(ResetToNeutralRoutine(fadeTime));
        }
    }

    private IEnumerator ResetToNeutralRoutine(float fadeTime)
    {
        if (_blendShapeProxy == null) yield break;

        if (_hasActiveExpression)
        {
            float startVal = _blendShapeProxy.GetValue(_activeExpressionKey);
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                _blendShapeProxy.ImmediatelySetValue(_activeExpressionKey, Mathf.Lerp(startVal, 0f, elapsed / fadeTime));
                yield return null;
            }
            _blendShapeProxy.ImmediatelySetValue(_activeExpressionKey, 0f);
            _hasActiveExpression = false;
        }
    }

    private BlendShapeKey GetKeyFromEmotion(string emotion)
    {
        switch (emotion.ToLower().Trim())
        {
            case "happy": case "joy": case "senang":
                return BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy);
            case "angry": case "marah":
                return BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry);
            case "sad": case "sorrow": case "sedih":
                return BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow);
            case "surprised": case "fun": case "kaget":
                return BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun);
            case "neutral": case "biasa":
                return BlendShapeKey.CreateFromPreset(BlendShapePreset.Neutral);
            default:
                return BlendShapeKey.CreateUnknown(emotion);
        }
    }

    private BlendShapePreset GetPresetFromViseme(string viseme)
    {
        switch (viseme.ToLower())
        {
            case "aa": case "a": return BlendShapePreset.A;
            case "ih": case "i": return BlendShapePreset.I;
            case "ou": case "u": return BlendShapePreset.U;
            case "ee": case "e": return BlendShapePreset.E;
            case "oh": case "o": return BlendShapePreset.O;
            default: return BlendShapePreset.Unknown;
        }
    }
}

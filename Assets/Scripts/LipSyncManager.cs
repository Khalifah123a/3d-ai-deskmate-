using UnityEngine;
using System.Collections;

public class LipSyncManager : MonoBehaviour
{
    private ExpressionController _expressionController;
    private VRMIdleAnimator _idleAnimator;
    private AudioSource _audioSource;
    private Coroutine _lipSyncCoroutine;

    private float[] _visemeWeights = new float[5];
    private readonly string[] _visemes = { "aa", "ih", "ou", "ee", "oh" };

    public void Init(ExpressionController expr, VRMIdleAnimator idle)
    {
        _expressionController = expr;
        _idleAnimator = idle;
    }

    void Awake()
    {
        if (_expressionController == null)
            _expressionController = GetComponent<ExpressionController>();
        if (_idleAnimator == null)
            _idleAnimator = GetComponent<VRMIdleAnimator>();
    }

    public void StartLipSync()
    {
        if (_lipSyncCoroutine != null) return;
        _lipSyncCoroutine = StartCoroutine(LipSyncUpdate());
    }

    public void StopLipSync()
    {
        if (_lipSyncCoroutine != null)
        {
            StopCoroutine(_lipSyncCoroutine);
            _lipSyncCoroutine = null;
        }

        if (_idleAnimator != null)
            _idleAnimator.SetSpeaking(false);

        ResetAllVisemes();
    }

    private IEnumerator LipSyncUpdate()
    {
        if (_idleAnimator != null)
            _idleAnimator.SetSpeaking(true);

        while (true)
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                float amplitude = GetAmplitude();
                ApplyVisemes(amplitude);
            }
            else
            {
                ResetAllVisemes();
                if (_idleAnimator != null)
                    _idleAnimator.SetSpeaking(false);
            }
            yield return null;
        }
    }

    private float GetAmplitude()
    {
        if (_audioSource == null) return 0f;

        float[] samples = new float[256];
        _audioSource.GetOutputData(samples, 0);

        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
            sum += samples[i] * samples[i];

        return Mathf.Sqrt(sum / samples.Length);
    }

    private void ApplyVisemes(float amplitude)
    {
        if (_expressionController == null) return;

        if (amplitude > 0.015f)
        {
            float targetOpen = Mathf.Clamp01(amplitude * 1.5f);
            _visemeWeights[0] = Mathf.Lerp(_visemeWeights[0], targetOpen, Time.deltaTime * 20f);
            _expressionController.SetViseme("aa", _visemeWeights[0]);

            for (int i = 1; i < 5; i++)
            {
                _visemeWeights[i] = Mathf.Lerp(_visemeWeights[i], 0f, Time.deltaTime * 15f);
                _expressionController.SetViseme(_visemes[i], _visemeWeights[i]);
            }
        }
        else
        {
            ResetAllVisemes();
        }
    }

    private void ResetAllVisemes()
    {
        if (_expressionController == null) return;
        for (int i = 0; i < 5; i++)
        {
            _visemeWeights[i] = 0f;
            _expressionController.SetViseme(_visemes[i], 0f);
        }
    }

    public void SetAudioSource(AudioSource source)
    {
        _audioSource = source;
        if (_lipSyncCoroutine == null)
            StartLipSync();
    }

    public void ClearAudioSource()
    {
        _audioSource = null;
        StopLipSync();
    }
}

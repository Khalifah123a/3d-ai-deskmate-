using UnityEngine;
using VRM;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Hand gesture system for VRM avatar
/// Supports: Thumbs Up, OK, Pointing, Wave, Prayer, etc.
/// </summary>
public class HandGestures : MonoBehaviour
{
    private Animator _animator;
    private Transform _leftHand;
    private Transform _rightHand;
    private Coroutine _gestureCoroutine;
    private bool _isDoingGesture;

    // Default hand rotations (neutral position)
    private Quaternion _leftHandDefault;
    private Quaternion _rightHandDefault;

    // Available gestures
    public enum GestureType
    {
        None,
        ThumbsUp,
        OK,
        Pointing,
        Wave,
        Prayer,
        RaisedHands,
        Clapping,
        Heart
    }

    // Gesture configurations
    private struct GestureConfig
    {
        public Quaternion leftHand;
        public Quaternion rightHand;
        public float duration;
    }

    private readonly Dictionary<GestureType, GestureConfig> _gestureConfigs = new Dictionary<GestureType, GestureConfig>
    {
        {
            GestureType.ThumbsUp,
            new GestureConfig
            {
                leftHand = Quaternion.Euler(0, 0, 0),
                rightHand = Quaternion.Euler(-20f, 0, -30f), // Thumbs up
                duration = 2.0f
            }
        },
        {
            GestureType.OK,
            new GestureConfig
            {
                leftHand = Quaternion.Euler(0, 0, 0),
                rightHand = Quaternion.Euler(-10f, 0, -20f), // OK sign
                duration = 1.5f
            }
        },
        {
            GestureType.Pointing,
            new GestureConfig
            {
                leftHand = Quaternion.Euler(0, 0, 0),
                rightHand = Quaternion.Euler(-30f, 0, 0), // Pointing forward
                duration = 2.0f
            }
        },
        {
            GestureType.Wave,
            new GestureConfig
            {
                leftHand = Quaternion.Euler(0, 0, 0),
                rightHand = Quaternion.Euler(-90f, 0, 0), // Waving up
                duration = 1.5f
            }
        },
        {
            GestureType.Prayer,
            new GestureConfig
            {
                leftHand = Quaternion.Euler(0, 0, 45f),
                rightHand = Quaternion.Euler(0, 0, -45f), // Praying together
                duration = 2.0f
            }
        },
        {
            GestureType.RaisedHands,
            new GestureConfig
            {
                leftHand = Quaternion.Euler(-90f, 0, -30f),
                rightHand = Quaternion.Euler(-90f, 0, 30f), // Both hands up
                duration = 2.0f
            }
        },
        {
            GestureType.Clapping,
            new GestureConfig
            {
                leftHand = Quaternion.Euler(0, 60f, 0),
                rightHand = Quaternion.Euler(0, -60f, 0), // Hands together for clap
                duration = 1.0f
            }
        },
        {
            GestureType.Heart,
            new GestureConfig
            {
                leftHand = Quaternion.Euler(-30f, 30f, -30f),
                rightHand = Quaternion.Euler(-30f, -30f, 30f), // Heart shape
                duration = 2.0f
            }
        }
    };

    public void Init()
    {
        _animator = GetComponent<Animator>();
        if (_animator != null && _animator.isHuman)
        {
            _leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
        }
        else
        {
            _leftHand = FindByString("J_Bip_L_Hand");
            _rightHand = FindByString("J_Bip_R_Hand");
        }

        if (_leftHand != null) _leftHandDefault = _leftHand.localRotation;
        if (_rightHand != null) _rightHandDefault = _rightHand.localRotation;
    }

    // Public API: trigger gesture
    public void TriggerGesture(GestureType gesture)
    {
        if (_gestureCoroutine != null)
            StopCoroutine(_gestureCoroutine);
        
        if (gesture == GestureType.None)
            return;

        _gestureCoroutine = StartCoroutine(PlayGestureRoutine(gesture));
    }

    // Trigger gesture by name (for slash commands)
    public void TriggerGestureByName(string name)
    {
        if (Enum.TryParse(name, true, out GestureType gesture))
        {
            TriggerGesture(gesture);
        }
        else
        {
            Debug.LogWarning($"[Gestures] Unknown gesture: {name}");
        }
    }

    private IEnumerator PlayGestureRoutine(GestureType gesture)
    {
        _isDoingGesture = true;
        
        if (!_gestureConfigs.TryGetValue(gesture, out var config))
        {
            _isDoingGesture = false;
            yield break;
        }

        // Animate to gesture pose
        float elapsed = 0f;
        float moveSpeed = 10f;
        
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.3f);
            
            if (_leftHand != null)
                _leftHand.localRotation = Quaternion.Slerp(_leftHand.localRotation, config.leftHand, t);
            if (_rightHand != null)
                _rightHand.localRotation = Quaternion.Slerp(_rightHand.localRotation, config.rightHand, t);
            
            yield return null;
        }

        // Hold gesture
        yield return new WaitForSeconds(config.duration);

        // Return to neutral
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.3f);
            
            if (_leftHand != null)
                _leftHand.localRotation = Quaternion.Slerp(_leftHand.localRotation, _leftHandDefault, t);
            if (_rightHand != null)
                _rightHand.localRotation = Quaternion.Slerp(_rightHand.localRotation, _rightHandDefault, t);
            
            yield return null;
        }

        _isDoingGesture = false;
        _gestureCoroutine = null;
    }

    private Transform FindByString(string name)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    public bool IsDoingGesture => _isDoingGesture;
}

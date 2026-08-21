using UnityEngine;
using VRM;
using System.Collections;

public class VRMIdleAnimator : MonoBehaviour
{
    private VRMBlendShapeProxy _proxy;
    private Animator _animator;

    private Transform _headBone;
    private Transform _chestBone;
    private Transform _spineBone;
    private Transform _leftUpperArm;
    private Transform _rightUpperArm;
    private Transform _leftLowerArm;
    private Transform _rightLowerArm;

    private Quaternion _headDefault;
    private Quaternion _chestDefault;
    private Quaternion _spineDefault;
    private Quaternion _leftArmDefault;
    private Quaternion _rightArmDefault;
    private Quaternion _leftForearmDefault;
    private Quaternion _rightForearmDefault;

    private float _time;
    private float _blinkTimer;
    private float _nextBlinkTime;
    private float _microExprTimer;
    private float _nextMicroExprTime;
    private bool _isSpeaking;
    private bool _isThinking;
    private bool _initialized;
    private LookAtMouse _lookAtMouse; // Skip head when mouse tracking

    // Nodding for thinking
    private float _nodTimer;
    private float _nextNodTime;
    private Quaternion _nodStartRot;

    // Hand gesture when speaking
    private float _gestureTimer;
    private bool _isGesturing;

    public void Init(GameObject root)
    {
        _proxy = root.GetComponent<VRMBlendShapeProxy>();
        _animator = root.GetComponent<Animator>();
        _lookAtMouse = root.GetComponent<LookAtMouse>();

        if (_animator != null && _animator.isHuman)
        {
            _headBone = _animator.GetBoneTransform(HumanBodyBones.Head);
            _chestBone = _animator.GetBoneTransform(HumanBodyBones.Chest);
            _spineBone = _animator.GetBoneTransform(HumanBodyBones.Spine);
            _leftUpperArm = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            _rightUpperArm = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _leftLowerArm = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            _rightLowerArm = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        }
        else
        {
            _headBone = FindByString(root, "J_Bip_C_Head");
            _chestBone = FindByString(root, "J_Bip_C_Chest");
            _spineBone = FindByString(root, "J_Bip_C_Spine");
            _leftUpperArm = FindByString(root, "J_Bip_L_UpperArm");
            _rightUpperArm = FindByString(root, "J_Bip_R_UpperArm");
            _leftLowerArm = FindByString(root, "J_Bip_L_LowerArm");
            _rightLowerArm = FindByString(root, "J_Bip_R_LowerArm");
        }

        ApplyRestPose();
        SaveDefaults();

        _initialized = true;
        _nextBlinkTime = Random.Range(2f, 5f);
        _nextMicroExprTime = Random.Range(3f, 8f);
        _nextNodTime = Random.Range(5f, 10f);
    }

    private void ApplyRestPose()
    {
        if (_leftUpperArm != null)
            _leftUpperArm.localRotation = Quaternion.Euler(0f, 0f, 75f);
        if (_rightUpperArm != null)
            _rightUpperArm.localRotation = Quaternion.Euler(0f, 0f, -75f);
        if (_leftLowerArm != null)
            _leftLowerArm.localRotation = Quaternion.Euler(20f, 10f, 0f);
        if (_rightLowerArm != null)
            _rightLowerArm.localRotation = Quaternion.Euler(20f, -10f, 0f);
    }

    private void SaveDefaults()
    {
        if (_headBone) _headDefault = _headBone.localRotation;
        if (_chestBone) _chestDefault = _chestBone.localRotation;
        if (_spineBone) _spineDefault = _spineBone.localRotation;
        if (_leftUpperArm) _leftArmDefault = _leftUpperArm.localRotation;
        if (_rightUpperArm) _rightArmDefault = _rightUpperArm.localRotation;
        if (_leftLowerArm) _leftForearmDefault = _leftLowerArm.localRotation;
        if (_rightLowerArm) _rightForearmDefault = _rightLowerArm.localRotation;
    }

    private Transform FindByString(GameObject root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    public void SetSpeaking(bool speaking) 
    { 
        _isSpeaking = speaking;
        _isGesturing = speaking;
        _gestureTimer = 0f;
    }

    public void SetThinking(bool thinking) 
    { 
        _isThinking = thinking;
        if (thinking)
        {
            _nodTimer = 0f;
            _nextNodTime = Random.Range(0.5f, 1.5f);
        }
    }

    void LateUpdate()
    {
        if (!_initialized) return;
        _time += Time.deltaTime;

        // Head: only move if LookAtMouse is NOT active (avoids conflict)
        if (_headBone != null && _lookAtMouse == null)
        {
            float headPitch = Mathf.Sin(_time * 0.8f) * 3f;
            float headYaw = Mathf.Cos(_time * 0.5f) * 4f;
            
            _headBone.localRotation = Quaternion.Slerp(_headBone.localRotation,
                _headDefault * Quaternion.Euler(headPitch, headYaw, 0f), Time.deltaTime * 3f);
        }

        // Nodding when thinking (works even with LookAtMouse)
        if (_headBone != null && _isThinking)
        {
            _nodTimer += Time.deltaTime;
            if (_nodTimer >= _nextNodTime)
            {
                _nodTimer = 0f;
                _nextNodTime = Random.Range(0.8f, 2f);
                _nodStartRot = _headBone.localRotation;
                StartCoroutine(NodRoutine());
            }
        }

        // Chest: breathing + weight shift
        if (_chestBone != null)
        {
            float breath = Mathf.Sin(_time * 1.6f) * 2.5f;
            float weightShift = Mathf.Sin(_time * 0.3f) * 1.5f;
            _chestBone.localRotation = Quaternion.Slerp(_chestBone.localRotation,
                _chestDefault * Quaternion.Euler(breath, weightShift, 0), Time.deltaTime * 3f);
        }

        // Spine: body sway
        if (_spineBone != null)
        {
            float swayX = Mathf.Sin(_time * 0.7f) * 2f;
            float swayZ = Mathf.Cos(_time * 0.4f) * 1.5f;
            _spineBone.localRotation = Quaternion.Slerp(_spineBone.localRotation,
                _spineDefault * Quaternion.Euler(swayX, 0, swayZ), Time.deltaTime * 3f);
        }

        // Arms: continuous sway + gestures when speaking
        float armSwayLeft = Mathf.Sin(_time * 0.8f) * 3f;
        float armSwayRight = Mathf.Sin(_time * 0.8f + Mathf.PI) * 3f;

        if (_isGesturing && _rightUpperArm != null)
        {
            _gestureTimer += Time.deltaTime;
            float gestureAmount = Mathf.Sin(_gestureTimer * 3f) * 5f;
            armSwayRight += gestureAmount;
        }

        if (_leftUpperArm != null)
            _leftUpperArm.localRotation = Quaternion.Slerp(_leftUpperArm.localRotation,
                _leftArmDefault * Quaternion.Euler(0, 0, armSwayLeft), Time.deltaTime * 3f);
        if (_rightUpperArm != null)
            _rightUpperArm.localRotation = Quaternion.Slerp(_rightUpperArm.localRotation,
                _rightArmDefault * Quaternion.Euler(0, 0, armSwayRight), Time.deltaTime * 3f);

        // Blinking
        _blinkTimer += Time.deltaTime;
        if (_blinkTimer >= _nextBlinkTime && !_isSpeaking)
        {
            _blinkTimer = 0f;
            _nextBlinkTime = Random.Range(2.5f, 5f);
            StartCoroutine(BlinkRoutine());
        }

        // Micro-expressions
        _microExprTimer += Time.deltaTime;
        if (_microExprTimer >= _nextMicroExprTime && !_isSpeaking && _proxy != null)
        {
            _microExprTimer = 0f;
            _nextMicroExprTime = Random.Range(5f, 12f);
            StartCoroutine(MicroExpressionRoutine());
        }
    }

    private IEnumerator NodRoutine()
    {
        if (_headBone == null) yield break;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float nodAngle = Mathf.Sin(t * Mathf.PI) * 10f;
            _headBone.localRotation = _nodStartRot * Quaternion.Euler(nodAngle, 0, 0);
            yield return null;
        }
        
        _headBone.localRotation = _nodStartRot;
    }

    private IEnumerator BlinkRoutine()
    {
        if (_proxy == null) yield break;
        float dur = 0.08f, e = 0f;
        var key = BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink);
        while (e < dur) { e += Time.deltaTime; _proxy.ImmediatelySetValue(key, e / dur); yield return null; }
        e = 0f;
        while (e < dur) { e += Time.deltaTime; _proxy.ImmediatelySetValue(key, 1f - e / dur); yield return null; }
        _proxy.ImmediatelySetValue(key, 0f);
    }

    private IEnumerator MicroExpressionRoutine()
    {
        if (_proxy == null) yield break;

        int exprType = Random.Range(0, 3);
        BlendShapeKey key;
        float maxWeight = 0.2f;

        switch (exprType)
        {
            case 0: key = BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy); break;
            case 1: key = BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun); break;
            default: key = BlendShapeKey.CreateFromPreset(BlendShapePreset.A); break;
        }

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float weight = Mathf.Sin(t * Mathf.PI) * maxWeight;
            _proxy.ImmediatelySetValue(key, weight);
            yield return null;
        }
        _proxy.ImmediatelySetValue(key, 0f);
    }
}

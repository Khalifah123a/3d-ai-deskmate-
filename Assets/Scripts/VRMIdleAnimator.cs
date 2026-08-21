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
    private bool _initialized;

    public void Init(GameObject root)
    {
        _proxy = root.GetComponent<VRMBlendShapeProxy>();
        _animator = root.GetComponent<Animator>();

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
    }

    private void ApplyRestPose()
    {
        // Slightly more open arms for better idle appearance
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

    public void SetSpeaking(bool speaking) { _isSpeaking = speaking; }

    void LateUpdate()
    {
        if (!_initialized) return;
        _time += Time.deltaTime;

        // Head: micro-movements (increased amplitude for better visibility)
        if (_headBone != null)
        {
            float headPitch = Mathf.Sin(_time * 0.8f) * 3f;
            float headYaw = Mathf.Cos(_time * 0.5f) * 4f;
            _headBone.localRotation = Quaternion.Slerp(_headBone.localRotation,
                _headDefault * Quaternion.Euler(headPitch, headYaw, 0f), Time.deltaTime * 3f);
        }

        // Chest: breathing + weight shift (increased amplitude)
        if (_chestBone != null)
        {
            float breath = Mathf.Sin(_time * 1.6f) * 2.5f;
            float weightShift = Mathf.Sin(_time * 0.3f) * 1.5f;
            _chestBone.localRotation = Quaternion.Slerp(_chestBone.localRotation,
                _chestDefault * Quaternion.Euler(breath, weightShift, 0), Time.deltaTime * 3f);
        }

        // Spine: body sway (increased amplitude)
        if (_spineBone != null)
        {
            float swayX = Mathf.Sin(_time * 0.7f) * 2f;
            float swayZ = Mathf.Cos(_time * 0.4f) * 1.5f;
            _spineBone.localRotation = Quaternion.Slerp(_spineBone.localRotation,
                _spineDefault * Quaternion.Euler(swayX, 0, swayZ), Time.deltaTime * 3f);
        }

        // Arms: continuous sway (increased amplitude for better visibility)
        float armSwayLeft = Mathf.Sin(_time * 0.8f) * 3f;
        float armSwayRight = Mathf.Sin(_time * 0.8f + Mathf.PI) * 3f;

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

        // Micro-expressions (subtle facial changes)
        _microExprTimer += Time.deltaTime;
        if (_microExprTimer >= _nextMicroExprTime && !_isSpeaking && _proxy != null)
        {
            _microExprTimer = 0f;
            _nextMicroExprTime = Random.Range(5f, 12f);
            StartCoroutine(MicroExpressionRoutine());
        }
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

        // Randomly choose: subtle smile, brow raise, or slight mouth movement
        int exprType = Random.Range(0, 3);
        BlendShapeKey key;
        float maxWeight = 0.2f; // Slightly increased for better visibility

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

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
    private LookAtMouse _lookAtMouse;
    private ExpressionPresets _expressionPresets; // Mood tracking

    // Head tilt for happy mood
    private float _tiltTimer;
    private float _tiltAmount;
    private bool _isTilting;

    // Bored/fidgety movements
    private float _fidgetTimer;
    private bool _isFidgeting;
    private float _fidgetPhase;
    private bool _isDoingSpecialAction;
    private Coroutine _specialActionCoroutine;

    // Nodding for thinking
    private float _nodTimer;
    private float _nextNodTime;
    private Quaternion _nodStartRot;

    // Hand gesture when speaking
    private float _gestureTimer;
    private bool _isGesturing;

    // Special mood states for body language
    private bool _isBouncing;      // Happy/excited bounce
    private bool _isSlouching;     // Sad/depressed slouch
    private bool _isCrossedArms;   // Angry/confident pose
    private bool _isHandOnChin;    // Thoughtful pose
    private bool _isHeadOnHand;    // Bored pose

    // Lower body (legs/feet)
    private Transform _leftUpperLeg;
    private Transform _rightUpperLeg;
    private Transform _leftLowerLeg;
    private Transform _rightLowerLeg;
    private Quaternion _leftLegDefault;
    private Quaternion _rightLegDefault;
    private Quaternion _leftShinDefault;
    private Quaternion _rightShinDefault;

    public void Init(GameObject root)
    {
        _proxy = root.GetComponent<VRMBlendShapeProxy>();
        _animator = root.GetComponent<Animator>();
        _lookAtMouse = root.GetComponent<LookAtMouse>();
        _expressionPresets = root.GetComponent<ExpressionPresets>();

        if (_animator != null && _animator.isHuman)
        {
            _headBone = _animator.GetBoneTransform(HumanBodyBones.Head);
            _chestBone = _animator.GetBoneTransform(HumanBodyBones.Chest);
            _spineBone = _animator.GetBoneTransform(HumanBodyBones.Spine);
            _leftUpperArm = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            _rightUpperArm = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _leftLowerArm = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            _rightLowerArm = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            _leftUpperLeg = _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            _rightUpperLeg = _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            _leftLowerLeg = _animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            _rightLowerLeg = _animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
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
            _leftUpperLeg = FindByString(root, "J_Bip_L_UpperLeg");
            _rightUpperLeg = FindByString(root, "J_Bip_R_UpperLeg");
            _leftLowerLeg = FindByString(root, "J_Bip_L_LowerLeg");
            _rightLowerLeg = FindByString(root, "J_Bip_R_LowerLeg");
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
        if (_leftUpperLeg) _leftLegDefault = _leftUpperLeg.localRotation;
        if (_rightUpperLeg) _rightLegDefault = _rightUpperLeg.localRotation;
        if (_leftLowerLeg) _leftShinDefault = _leftLowerLeg.localRotation;
        if (_rightLowerLeg) _rightShinDefault = _rightLowerLeg.localRotation;
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
            _tiltTimer = 0f;
            _isTilting = false;
            _isFidgeting = false;
            _nodTimer = 0f;
            _nextNodTime = Random.Range(0.5f, 1.5f);
        }
    }

    // Called by ExpressionPresets to sync mood
    public void SetMood(string mood)
    {
        // Reset all special states first
        _isBouncing = false;
        _isSlouching = false;
        _isCrossedArms = false;
        _isHandOnChin = false;
        _isHeadOnHand = false;

        switch (mood)
        {
            case "happy":
                _isTilting = true;
                _isBouncing = true;
                _isFidgeting = false;
                _tiltAmount = 0f;
                _tiltTimer = 0f;
                break;
            case "excited":
                _isTilting = true;
                _isBouncing = true;
                _isFidgeting = false;
                _tiltAmount = 0f;
                _tiltTimer = 0f;
                break;
            case "sad":
                _isTilting = false;
                _isFidgeting = false;
                _isSlouching = true;
                break;
            case "depressed":
            case "concerned":
                _isTilting = false;
                _isFidgeting = false;
                _isSlouching = true;
                break;
            case "angry":
                _isTilting = false;
                _isFidgeting = false;
                _isCrossedArms = true;
                break;
            case "sleepy":
            case "bored":
                _isTilting = false;
                _isFidgeting = true;
                _isHeadOnHand = true;
                _fidgetTimer = 0f;
                break;
            case "thoughtful":
                _isTilting = true;
                _isFidgeting = false;
                _isHandOnChin = true;
                _tiltAmount = 0f;
                _tiltTimer = 0f;
                break;
            case "confused":
                _isTilting = true;
                _isFidgeting = true;
                _tiltAmount = 0f;
                _tiltTimer = 0f;
                _fidgetTimer = 0f;
                break;
            default: // neutral
                _isTilting = false;
                _isFidgeting = false;
                break;
        }
    }

    void LateUpdate()
    {
        if (!_initialized) return;
        _time += Time.deltaTime;

        // Head: only move if LookAtMouse is NOT active (avoids conflict)
        if (_headBone != null && _lookAtMouse == null)
        {
            float basePitch = Mathf.Sin(_time * 0.8f) * 3f;
            float baseYaw = Mathf.Cos(_time * 0.5f) * 4f;

            // Happy: gentle head tilt
            if (_isTilting)
            {
                _tiltTimer += Time.deltaTime;
                _tiltAmount = Mathf.Sin(_tiltTimer * 1.5f) * 8f;
                _headBone.localRotation = Quaternion.Slerp(_headBone.localRotation,
                    _headDefault * Quaternion.Euler(basePitch, baseYaw, _tiltAmount), Time.deltaTime * 3f);
            }
            else
            {
                _headBone.localRotation = Quaternion.Slerp(_headBone.localRotation,
                    _headDefault * Quaternion.Euler(basePitch, baseYaw, 0f), Time.deltaTime * 3f);
            }
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

        // Spine: body sway + mood variations
        if (_spineBone != null)
        {
            float swayX = Mathf.Sin(_time * 0.7f) * 2f;
            float swayZ = Mathf.Cos(_time * 0.4f) * 1.5f;

            // Bored: slight slouch
            if (_isFidgeting)
                swayX += Mathf.Sin(_time * 0.5f) * 3f;

            // Happy: slight upright bounce
            if (_isTilting)
                swayX += Mathf.Sin(_time * 2f) * 1f;

            // Sad: deeper slouch
            if (_isSlouching)
                swayX += Mathf.Sin(_time * 0.3f) * 4f;

            _spineBone.localRotation = Quaternion.Slerp(_spineBone.localRotation,
                _spineDefault * Quaternion.Euler(swayX, 0, swayZ), Time.deltaTime * 3f);
        }

        // Arms: continuous sway + gestures when speaking
        float armSwayLeft = Mathf.Sin(_time * 0.8f) * 3f;
        float armSwayRight = Mathf.Sin(_time * 0.8f + Mathf.PI) * 3f;

        // Fidget when bored/sleepy
        if (_isFidgeting)
        {
            _fidgetTimer += Time.deltaTime;
            _fidgetPhase = Mathf.Sin(_fidgetTimer * 2f);
            armSwayLeft += _fidgetPhase * 2f;
            armSwayRight += _fidgetPhase * 1.5f;
        }

        // Bouncing arms for happy/excited
        if (_isBouncing)
        {
            float bounce = Mathf.Abs(Mathf.Sin(_time * 3f)) * 5f;
            armSwayLeft += bounce;
            armSwayRight -= bounce;
        }

        // Crossed arms for angry
        if (_isCrossedArms)
        {
            armSwayLeft += 15f; // Bring arms closer together
            armSwayRight -= 15f;
        }

        // Head on hand for bored
        if (_isHeadOnHand && _rightUpperArm != null)
        {
            armSwayRight = Mathf.Lerp(armSwayRight, -30f, Time.deltaTime * 2f);
        }

        // Hand on chin for thoughtful
        if (_isHandOnChin && _rightLowerArm != null)
        {
            armSwayRight = Mathf.Lerp(armSwayRight, -25f, Time.deltaTime * 2f);
        }

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

        // Lower body: subtle weight shift + foot tap when bored
        if (_leftUpperLeg != null && _rightUpperLeg != null)
        {
            float legShift = Mathf.Sin(_time * 0.4f) * 2f;
            if (_isFidgeting)
                legShift += Mathf.Sin(_time * 4f) * 3f; // Foot tap

            _leftUpperLeg.localRotation = Quaternion.Slerp(_leftUpperLeg.localRotation,
                _leftLegDefault * Quaternion.Euler(0, 0, legShift), Time.deltaTime * 3f);
            _rightUpperLeg.localRotation = Quaternion.Slerp(_rightUpperLeg.localRotation,
                _rightLegDefault * Quaternion.Euler(0, 0, -legShift), Time.deltaTime * 3f);
        }

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

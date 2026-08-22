using UnityEngine;
using VRM;
using System.Collections;

/// <summary>
/// Dance animation system for VRM avatar
/// Supports: Basic bounce, Happy dance, Excited dance, etc.
/// </summary>
public class DanceAnimations : MonoBehaviour
{
    private Animator _animator;
    private VRMBlendShapeProxy _blendShapeProxy;
    private Coroutine _danceCoroutine;
    private bool _isDancing;

    // Bone references
    private Transform _headBone;
    private Transform _chestBone;
    private Transform _spineBone;
    private Transform _leftUpperArm;
    private Transform _rightUpperArm;
    private Transform _leftLowerArm;
    private Transform _rightLowerArm;
    private Transform _leftUpperLeg;
    private Transform _rightUpperLeg;
    private Transform _leftLowerLeg;
    private Transform _rightLowerLeg;

    // Default rotations
    private Quaternion _headDefault;
    private Quaternion _chestDefault;
    private Quaternion _spineDefault;
    private Quaternion _leftArmDefault;
    private Quaternion _rightArmDefault;
    private Quaternion _leftLegDefault;
    private Quaternion _rightLegDefault;

    // Dance states
    private float _danceTime;
    private DanceStyle _currentStyle;

    public enum DanceStyle
    {
        Bounce,
        Happy,
        Excited,
        Groove,
        Victory
    }

    public void Init(GameObject root)
    {
        _animator = root.GetComponent<Animator>();
        _blendShapeProxy = root.GetComponent<VRMBlendShapeProxy>();

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

        SaveDefaults();
    }

    private void SaveDefaults()
    {
        if (_headBone != null) _headDefault = _headBone.localRotation;
        if (_chestBone != null) _chestDefault = _chestBone.localRotation;
        if (_spineBone != null) _spineDefault = _spineBone.localRotation;
        if (_leftUpperArm != null) _leftArmDefault = _leftUpperArm.localRotation;
        if (_rightUpperArm != null) _rightArmDefault = _rightUpperArm.localRotation;
        if (_leftUpperLeg != null) _leftLegDefault = _leftUpperLeg.localRotation;
        if (_rightUpperLeg != null) _rightLegDefault = _rightUpperLeg.localRotation;
    }

    // Public API: start dancing
    public void StartDancing(DanceStyle style = DanceStyle.Happy, float duration = 5f)
    {
        if (_danceCoroutine != null)
            StopCoroutine(_danceCoroutine);
        
        _currentStyle = style;
        _danceTime = 0f;
        _isDancing = true;
        
        _danceCoroutine = StartCoroutine(DanceRoutine(duration));
    }

    public void StopDancing()
    {
        if (_danceCoroutine != null)
            StopCoroutine(_danceCoroutine);
        _isDancing = false;
        _danceTime = 0f;
    }

    private IEnumerator DanceRoutine(float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration && _isDancing)
        {
            elapsed += Time.deltaTime;
            _danceTime = elapsed;
            
            switch (_currentStyle)
            {
                case DanceStyle.Bounce:
                    DoBounce();
                    break;
                case DanceStyle.Happy:
                    DoHappyDance();
                    break;
                case DanceStyle.Excited:
                    DoExcitedDance();
                    break;
                case DanceStyle.Groove:
                    DoGroove();
                    break;
                case DanceStyle.Victory:
                    DoVictoryDance();
                    break;
            }
            
            yield return null;
        }
        
        _isDancing = false;
        ResetToNeutral();
    }

    // Dance animations
    private void DoBounce()
    {
        float bounce = Mathf.Sin(_danceTime * 6f) * 5f;
        
        if (_headBone != null)
            _headBone.localRotation = _headDefault * Quaternion.Euler(bounce * 0.3f, 0, 0);
        if (_chestBone != null)
            _chestBone.localRotation = _chestDefault * Quaternion.Euler(bounce * 0.5f, 0, 0);
    }

    private void DoHappyDance()
    {
        float side = Mathf.Sin(_danceTime * 4f) * 10f;
        float bounce = Mathf.Abs(Mathf.Sin(_danceTime * 6f)) * 8f;
        
        if (_headBone != null)
            _headBone.localRotation = _headDefault * Quaternion.Euler(-bounce * 0.3f, 0, side * 0.5f);
        if (_chestBone != null)
            _chestBone.localRotation = _chestDefault * Quaternion.Euler(-bounce * 0.5f, 0, side * 0.3f);
        if (_spineBone != null)
            _spineBone.localRotation = _spineDefault * Quaternion.Euler(-bounce * 0.3f, 0, side * 0.2f);
        
        // Arms up and down
        if (_leftUpperArm != null)
            _leftUpperArm.localRotation = _leftArmDefault * Quaternion.Euler(0, 0, -bounce * 0.5f);
        if (_rightUpperArm != null)
            _rightUpperArm.localRotation = _rightArmDefault * Quaternion.Euler(0, 0, bounce * 0.5f);
    }

    private void DoExcitedDance()
    {
        float spin = Mathf.Sin(_danceTime * 8f) * 15f;
        float bounce = Mathf.Abs(Mathf.Sin(_danceTime * 8f)) * 12f;
        
        if (_headBone != null)
            _headBone.localRotation = _headDefault * Quaternion.Euler(-bounce * 0.5f, spin * 0.3f, spin * 0.2f);
        if (_chestBone != null)
            _chestBone.localRotation = _chestDefault * Quaternion.Euler(-bounce * 0.7f, spin * 0.5f, 0);
        if (_spineBone != null)
            _spineBone.localRotation = _spineDefault * Quaternion.Euler(-bounce * 0.4f, spin * 0.3f, 0);
        
        // Arms waving
        if (_leftUpperArm != null)
            _leftUpperArm.localRotation = _leftArmDefault * Quaternion.Euler(-bounce * 0.8f, 0, -15f);
        if (_rightUpperArm != null)
            _rightUpperArm.localRotation = _rightArmDefault * Quaternion.Euler(-bounce * 0.8f, 0, 15f);
        
        // Legs moving
        if (_leftUpperLeg != null)
            _leftUpperLeg.localRotation = _leftLegDefault * Quaternion.Euler(Mathf.Sin(_danceTime * 8f) * 10f, 0, 0);
        if (_rightUpperLeg != null)
            _rightUpperLeg.localRotation = _rightLegDefault * Quaternion.Euler(Mathf.Sin(_danceTime * 8f + Mathf.PI) * 10f, 0, 0);
    }

    private void DoGroove()
    {
        float sway = Mathf.Sin(_danceTime * 3f) * 12f;
        float bounce = Mathf.Abs(Mathf.Sin(_danceTime * 4f)) * 5f;
        
        if (_headBone != null)
            _headBone.localRotation = _headDefault * Quaternion.Euler(bounce * 0.3f, 0, sway * 0.5f);
        if (_chestBone != null)
            _chestBone.localRotation = _chestDefault * Quaternion.Euler(bounce * 0.5f, 0, sway * 0.3f);
        if (_spineBone != null)
            _spineBone.localRotation = _spineDefault * Quaternion.Euler(bounce * 0.3f, 0, sway * 0.2f);
        
        // Slow arm movement
        if (_leftUpperArm != null)
            _leftUpperArm.localRotation = _leftArmDefault * Quaternion.Euler(0, 0, Mathf.Sin(_danceTime * 2f) * 5f);
        if (_rightUpperArm != null)
            _rightUpperArm.localRotation = _rightArmDefault * Quaternion.Euler(0, 0, Mathf.Sin(_danceTime * 2f + Mathf.PI) * 5f);
    }

    private void DoVictoryDance()
    {
        float armRaise = Mathf.Abs(Mathf.Sin(_danceTime * 4f)) * 90f;
        float bounce = Mathf.Abs(Mathf.Sin(_danceTime * 6f)) * 10f;
        
        if (_headBone != null)
            _headBone.localRotation = _headDefault * Quaternion.Euler(-bounce * 0.3f, 0, 0);
        if (_chestBone != null)
            _chestBone.localRotation = _chestDefault * Quaternion.Euler(-bounce * 0.5f, 0, 0);
        
        // Arms raised in victory
        if (_leftUpperArm != null)
            _leftUpperArm.localRotation = _leftArmDefault * Quaternion.Euler(-armRaise * 0.8f, 0, -10f);
        if (_rightUpperArm != null)
            _rightUpperArm.localRotation = _rightArmDefault * Quaternion.Euler(-armRaise * 0.8f, 0, 10f);
        
        // Jump legs
        if (_leftUpperLeg != null)
            _leftUpperLeg.localRotation = _leftLegDefault * Quaternion.Euler(Mathf.Sin(_danceTime * 6f) * 15f, 0, 0);
        if (_rightUpperLeg != null)
            _rightUpperLeg.localRotation = _rightLegDefault * Quaternion.Euler(Mathf.Sin(_danceTime * 6f + Mathf.PI) * 15f, 0, 0);
    }

    private void ResetToNeutral()
    {
        if (_headBone != null)
            _headBone.localRotation = _headDefault;
        if (_chestBone != null)
            _chestBone.localRotation = _chestDefault;
        if (_spineBone != null)
            _spineBone.localRotation = _spineDefault;
        if (_leftUpperArm != null)
            _leftUpperArm.localRotation = _leftArmDefault;
        if (_rightUpperArm != null)
            _rightUpperArm.localRotation = _rightArmDefault;
        if (_leftUpperLeg != null)
            _leftUpperLeg.localRotation = _leftLegDefault;
        if (_rightUpperLeg != null)
            _rightUpperLeg.localRotation = _rightLegDefault;
    }

    public bool IsDancing => _isDancing;
}

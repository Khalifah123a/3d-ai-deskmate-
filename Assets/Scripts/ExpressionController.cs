using UnityEngine;
using System.Collections;

public class ExpressionController : MonoBehaviour
{
    public static bool expressionsEnabled = true;

    private VRMAnimationBridge _vrmBridge;

    public void InitBridge()
    {
        _vrmBridge = GetComponent<VRMAnimationBridge>();
    }

    public void SetExpression(string emotion, float weight = 0.5f)
    {
        if (!expressionsEnabled) return;
        if (_vrmBridge == null) _vrmBridge = GetComponent<VRMAnimationBridge>();
        try { _vrmBridge?.SetExpression(emotion); } catch (System.Exception e) { Debug.LogWarning("[Expr] Error: " + e.Message); }
    }

    public void SetViseme(string viseme, float weight = 1f)
    {
        if (!expressionsEnabled) return;
        if (_vrmBridge == null) _vrmBridge = GetComponent<VRMAnimationBridge>();
        try { _vrmBridge?.SetViseme(viseme, weight); } catch { }
    }

    public void ResetExpressions()
    {
        try { _vrmBridge?.ResetExpressions(); } catch { }
    }
}

using UnityEngine;
using VRM;

public class LookAtMouse : MonoBehaviour
{
    private Transform _target;
    private Camera _mainCamera;
    private VRMLookAtHead _lookAtHead;
    private bool _initialized;

    public void Initialize(VRMLookAtHead lookAt, Camera cam)
    {
        _lookAtHead = lookAt;
        _mainCamera = cam;

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_mainCamera == null)
        {
            Debug.LogWarning("[LookAtMouse] No camera found!");
            return;
        }

        // Create a target object that VRMLookAtHead will follow
        var go = new GameObject("MouseLookTarget");
        _target = go.transform;
        _target.position = _mainCamera.transform.position + _mainCamera.transform.forward * 3f;

        // Set VRMLookAtHead target to our mouse-driven object
        if (_lookAtHead != null)
            _lookAtHead.Target = _target;

        _initialized = true;
        Debug.Log("[LookAtMouse] Initialized - VRMLookAt will follow mouse cursor");
    }

    void Update()
    {
        if (!_initialized || _mainCamera == null || _target == null) return;

        // Convert mouse position to world space
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

        // Smooth follow mouse
        _target.position = Vector3.Lerp(_target.position, worldPos, Time.deltaTime * 5f);
    }

    void OnDestroy()
    {
        if (_target != null) Destroy(_target.gameObject);
    }
}

using UnityEngine;
using VRM;

public class LookAtMouse : MonoBehaviour
{
    private Transform _target;
    private Camera _mainCamera;

    void Start()
    {
        var go = new GameObject("MouseLookTarget");
        _target = go.transform;
        _target.position = transform.position + transform.forward * 2f;

        var vrmLookAt = GetComponent<VRMLookAtHead>();
        if (vrmLookAt != null)
            vrmLookAt.Target = _target;

        _mainCamera = Camera.main;
        Debug.Log("[LookAt] Camera: " + (_mainCamera != null) + " VRMLookAt: " + (vrmLookAt != null));
    }

    void Update()
    {
        if (_mainCamera == null) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

        _target.position = Vector3.Lerp(_target.position, worldPos, Time.deltaTime * 3f);
    }

    void OnDestroy()
    {
        if (_target != null) Destroy(_target.gameObject);
    }
}

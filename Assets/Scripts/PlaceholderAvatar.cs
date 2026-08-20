using UnityEngine;

public class PlaceholderAvatar : MonoBehaviour
{
    private GameObject _placeholderRoot;

    void Start()
    {
        _placeholderRoot = new GameObject("PlaceholderBody");
        _placeholderRoot.transform.SetParent(transform);
        _placeholderRoot.transform.localPosition = Vector3.zero;
        _placeholderRoot.transform.localScale = Vector3.one;
        _placeholderRoot.transform.localRotation = Quaternion.identity;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(_placeholderRoot.transform);
        body.transform.localPosition = new Vector3(0, 0.9f, 0);
        body.transform.localScale = new Vector3(0.35f, 0.85f, 0.25f);

        Renderer r = body.GetComponent<Renderer>();
        r.material.color = new Color(0.85f, 0.65f, 0.75f);

        Transform head = new GameObject("Head").transform;
        head.SetParent(_placeholderRoot.transform);
        head.localPosition = new Vector3(0, 1.95f, 0);

        GameObject headSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        headSphere.name = "HeadSphere";
        headSphere.transform.SetParent(head);
        headSphere.transform.localPosition = Vector3.zero;
        headSphere.transform.localScale = Vector3.one * 0.3f;
        headSphere.GetComponent<Renderer>().material.color = new Color(0.9f, 0.7f, 0.8f);

        GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eyeL.name = "EyeL";
        eyeL.transform.SetParent(head);
        eyeL.transform.localPosition = new Vector3(-0.08f, 0.03f, 0.13f);
        eyeL.transform.localScale = Vector3.one * 0.04f;
        eyeL.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);

        GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eyeR.name = "EyeR";
        eyeR.transform.SetParent(head);
        eyeR.transform.localPosition = new Vector3(0.08f, 0.03f, 0.13f);
        eyeR.transform.localScale = Vector3.one * 0.04f;
        eyeR.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);
    }

    public void HidePlaceholder()
    {
        if (_placeholderRoot != null)
            _placeholderRoot.SetActive(false);
    }

    public void ShowPlaceholder()
    {
        if (_placeholderRoot != null)
            _placeholderRoot.SetActive(true);
    }
}

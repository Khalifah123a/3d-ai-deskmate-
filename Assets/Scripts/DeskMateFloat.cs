using UnityEngine;

public class DeskMateFloat : MonoBehaviour
{
    public float floatAmplitude = 0.03f;
    public float floatFrequency = 1.2f;
    public float swayAmplitude = 0.015f;
    public float swayFrequency = 0.8f;

    private Vector3 _initialPosition;

    void Start()
    {
        _initialPosition = transform.localPosition;
    }

    void Update()
    {
        float newY = _initialPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        float newX = _initialPosition.x + Mathf.Cos(Time.time * swayFrequency) * swayAmplitude;

        transform.localPosition = new Vector3(newX, newY, _initialPosition.z);
    }
}

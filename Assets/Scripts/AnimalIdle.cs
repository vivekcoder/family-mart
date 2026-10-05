using UnityEngine;

public class AnimalIdle : MonoBehaviour
{
    public Transform head;
    public Transform body;
    public float amplitude = 0.05f;

    Vector3 _headStart;
    Vector3 _bodyStart;
    float _phase;

    void Start()
    {
        _phase = Random.value * 10f;
        _headStart = head.localPosition;
        _bodyStart = body.localPosition;
    }

    void Update()
    {
        _phase += Time.deltaTime;
        body.localPosition = _bodyStart + Vector3.up * (Mathf.Sin(_phase * 2f) * amplitude * 0.3f);
        head.localPosition = _headStart + Vector3.down * (Mathf.Abs(Mathf.Sin(_phase * 2.5f)) * amplitude);
    }
}
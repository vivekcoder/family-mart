using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public Transform body, armL, armR, legL, legR;

    Vector3 _lastPosition;
    float _phase;
    float _blend;

    void Start()
    {
        _lastPosition = transform.position;
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 delta = transform.position - _lastPosition;
        _lastPosition = transform.position;
        delta.y = 0f;

        float speed = delta.magnitude / dt;
        _blend = Mathf.MoveTowards(_blend, Mathf.Clamp01(speed / 2f), 6f * dt);
        _phase += dt * 11f * _blend;

        float swing = Mathf.Sin(_phase) * 35f * _blend;
        legL.localRotation = Quaternion.Euler(swing, 0f, 0f);
        legR.localRotation = Quaternion.Euler(-swing, 0f, 0f);
        armL.localRotation = Quaternion.Euler(-swing * 0.8f, 0f, 0f);
        armR.localRotation = Quaternion.Euler(swing * 0.8f, 0f, 0f);

        body.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(_phase)) * 0.05f * _blend, 0f);
    }
}
using UnityEngine;

public class DustPuff : MonoBehaviour
{
    Vector3 _velocity;
    float _life;
    float _size;
    float _age;

    public static void Burst(Vector3 center)
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8f + Random.value * 0.4f;
            Vector3 outward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            var go = GameBootstrap.MakePrimitive(PrimitiveType.Sphere, "Puff", new Color(0.97f, 0.96f, 0.92f), false);
            go.transform.position = center + outward * 0.8f + Vector3.up * 0.2f;
            go.transform.localScale = Vector3.zero;

            var puff = go.AddComponent<DustPuff>();
            puff._velocity = outward * 1.4f + Vector3.up * 1.1f;
            puff._life = 0.7f + Random.value * 0.3f;
            puff._size = 0.5f + Random.value * 0.3f;
        }
    }

    void Update()
    {
        _age += Time.deltaTime;
        float k = _age / _life;
        if (k >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += _velocity * Time.deltaTime;
        _velocity *= 1f - 2f * Time.deltaTime;
        transform.localScale = Vector3.one * (_size * Mathf.Sin(k * Mathf.PI));   // grows, then shrinks away
    }
}
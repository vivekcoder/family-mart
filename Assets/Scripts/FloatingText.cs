using UnityEngine;

public class FloatingText : MonoBehaviour
{
    TextMesh _text;
    Color _color;
    float _age;

    public static void Spawn(Vector3 position, string message, Color color)
    {
        var go = new GameObject("Floating Text");
        go.transform.position = position;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var text = go.AddComponent<TextMesh>();
        text.font = font;
        text.fontSize = 64;
        text.characterSize = 0.14f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.text = message;
        text.color = color;
        go.GetComponent<MeshRenderer>().material = font.material;

        var floating = go.AddComponent<FloatingText>();
        floating._text = text;
        floating._color = color;
    }

    void LateUpdate()
    {
        _age += Time.deltaTime;
        transform.position += Vector3.up * (1.2f * Time.deltaTime);
        if (Camera.main != null) transform.rotation = Camera.main.transform.rotation;

        // Fully visible for 0.6s, then fades out over 0.6s
        _color.a = Mathf.Clamp01(1f - (_age - 0.6f) / 0.6f);
        _text.color = _color;

        if (_age >= 1.2f) Destroy(gameObject);
    }
}
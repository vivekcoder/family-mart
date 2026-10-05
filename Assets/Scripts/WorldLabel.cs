using UnityEngine;

public class WorldLabel : MonoBehaviour
{
    public static TextMesh Create(Transform parent, Vector3 localPosition)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var text = go.AddComponent<TextMesh>();
        text.font = font;
        text.fontSize = 64;
        text.characterSize = 0.12f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.white;

        go.GetComponent<MeshRenderer>().material = font.material;
        go.AddComponent<WorldLabel>();
        return text;
    }

    void LateUpdate()
    {
        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;
    }
}
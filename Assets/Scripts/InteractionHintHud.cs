using UnityEngine;
using UnityEngine.UI;

public class InteractionHintHud : MonoBehaviour
{
    GameObject _pill;
    Text _text;
    string _shown;

    public static void Create()
    {
        var canvasGO = new GameObject("Hint Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 15;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        var hud = canvasGO.AddComponent<InteractionHintHud>();

        var pill = new GameObject("Pill", typeof(RectTransform), typeof(Image));
        pill.transform.SetParent(canvasGO.transform, false);
        var image = pill.GetComponent<Image>();
        image.sprite = Icons.RoundedRect;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.1f, 0.12f, 0.17f, 0.8f);
        image.raycastTarget = false;

        var rect = (RectTransform)pill.transform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 165f);
        rect.sizeDelta = new Vector2(440f, 100f);

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(pill.transform, false);
        var text = textGO.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 52;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;

        var textRect = (RectTransform)textGO.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        hud._pill = pill;
        hud._text = text;
        pill.SetActive(false);
    }

    void LateUpdate()
    {
        string hint = InteractionHint.Consume();
        if (hint == _shown) return;

        _shown = hint;
        if (hint == null)
        {
            _pill.SetActive(false);
        }
        else
        {
            _text.text = hint;
            _pill.SetActive(true);
        }
    }
}
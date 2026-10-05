using UnityEngine;
using UnityEngine.UI;

public class MoneyHud : MonoBehaviour
{
    Text _moneyText;
    Text _inventoryText;
    int _lastMoney = -1;
    int _lastCount = -1;
    int _lastCapacity = -1;

    public static void Create()
    {
        var canvasGO = new GameObject("HUD Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        var hud = canvasGO.AddComponent<MoneyHud>();

        hud._moneyText = MakeText("Money Text", canvasGO.transform, 72, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(50f, -60f), new Vector2(700f, 120f));

        hud._inventoryText = MakeText("Inventory Text", canvasGO.transform, 56, TextAnchor.LowerCenter,
            new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(800f, 100f));
    }

    static Text MakeText(string name, Transform parent, int fontSize, TextAnchor alignment,
        Vector2 anchor, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);

        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = alignment;
        text.color = new Color(1f, 0.9f, 0.3f);
        text.raycastTarget = false;

        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return text;
    }

    void Update()
    {
        if (Wallet.Money != _lastMoney)
        {
            _lastMoney = Wallet.Money;
            _moneyText.text = "$ " + _lastMoney;
        }

        PlayerInventory inv = PlayerInventory.Instance;
        if (inv != null && (inv.Count != _lastCount || inv.Capacity != _lastCapacity))
        {
            _lastCount = inv.Count;
            _lastCapacity = inv.Capacity;
            _inventoryText.text = "Inventory: " + _lastCount + " / " + _lastCapacity;
            _inventoryText.color = inv.IsFull ? new Color(1f, 0.55f, 0.2f) : new Color(1f, 0.9f, 0.3f);
        }
    }
}
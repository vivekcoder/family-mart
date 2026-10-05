using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour
{
    // Direction the player is pushing: (-1..1, -1..1)
    public Vector2 Value { get; private set; }

    const float Radius = 120f; // joystick travel, in canvas units

    Canvas _canvas;
    RectTransform _background;
    RectTransform _knob;
    Vector2 _origin;
    bool _held;

    public static VirtualJoystick Create()
    {
        var canvasGO = new GameObject("UI Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        var joystick = canvasGO.AddComponent<VirtualJoystick>();
        joystick._canvas = canvas;

        Sprite circle = MakeCircleSprite();
        joystick._background = MakeImage("Joystick Background", canvasGO.transform, circle,
            new Color(1f, 1f, 1f, 0.25f), Radius * 2f, Vector2.zero);
        joystick._knob = MakeImage("Joystick Knob", joystick._background, circle,
            new Color(1f, 1f, 1f, 0.6f), Radius, new Vector2(0.5f, 0.5f));
        joystick._background.gameObject.SetActive(false);

        return joystick;
    }

    void Update()
    {
        float scale = _canvas.scaleFactor;
        ReadPointer(out bool pressedThisFrame, out bool isPressed, out Vector2 position);

        // A tap on a UI button must not start the joystick
        if (pressedThisFrame && UiInput.IsOverUi(position)) pressedThisFrame = false;

        if (pressedThisFrame)
        {
            _held = true;
            _origin = position;
            _background.gameObject.SetActive(true);
            _background.anchoredPosition = _origin / scale;
            _knob.anchoredPosition = Vector2.zero;
        }

        if (_held && isPressed)
        {
            Vector2 delta = (position - _origin) / scale;
            Vector2 clamped = Vector2.ClampMagnitude(delta, Radius);
            _knob.anchoredPosition = clamped;
            Value = clamped / Radius;
        }
        else
        {
            if (_held)
            {
                _held = false;
                _background.gameObject.SetActive(false);
            }
            // Keyboard fallback (WASD / arrows) for testing on PC
            Value = ReadKeyboard();
        }
    }

    // Reads touch on a phone, or the mouse on a PC
    static void ReadPointer(out bool pressedThisFrame, out bool isPressed, out Vector2 position)
    {
        pressedThisFrame = false;
        isPressed = false;
        position = Vector2.zero;

        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            var touch = touchscreen.primaryTouch;
            if (touch.press.isPressed || touch.press.wasPressedThisFrame)
            {
                pressedThisFrame = touch.press.wasPressedThisFrame;
                isPressed = touch.press.isPressed;
                position = touch.position.ReadValue();
                return;
            }
        }

        var mouse = Mouse.current;
        if (mouse != null)
        {
            pressedThisFrame = mouse.leftButton.wasPressedThisFrame;
            isPressed = mouse.leftButton.isPressed;
            position = mouse.position.ReadValue();
        }
    }

    static Vector2 ReadKeyboard()
    {
        var kb = Keyboard.current;
        if (kb == null) return Vector2.zero;

        float x = 0f, y = 0f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
        return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
    }

    static RectTransform MakeImage(string name, Transform parent, Sprite sprite,
        Color color, float size, Vector2 anchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;

        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(size, size);
        return rt;
    }

    static Sprite MakeCircleSprite()
    {
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float c = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(c - d)));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class UiInput
{
    static readonly List<RectTransform> _blockers = new List<RectTransform>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        _blockers.Clear();
    }

    // While a UI panel is registered, presses on it won't start the joystick
    public static void Register(RectTransform rect)
    {
        if (!_blockers.Contains(rect)) _blockers.Add(rect);
    }

    public static void Unregister(RectTransform rect)
    {
        _blockers.Remove(rect);
    }

    public static bool IsOverUi(Vector2 screenPosition)
    {
        foreach (RectTransform rect in _blockers)
        {
            if (rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, null))
                return true;
        }
        return false;
    }

    // Reads the touch screen on a phone, or the mouse on a PC
    public static void ReadPointer(out bool pressedThisFrame, out bool isPressed, out Vector2 position)
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
}
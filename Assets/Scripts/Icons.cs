using UnityEngine;

// Small icons drawn in code, so no image files are needed
public static class Icons
{
    const int Size = 64;

    static Sprite _exclamation;
    static Sprite _upArrow;
    static Sprite _whiteCenter;
    static Sprite _whiteLeft;
    static Sprite _rounded;
    static readonly Sprite[] _faces = new Sprite[4];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        _exclamation = null;
        _upArrow = null;
        _whiteCenter = null;
        _whiteLeft = null;
        _rounded = null;
        for (int i = 0; i < _faces.Length; i++) _faces[i] = null;
    }

    // A white circle with a dark "!" (tinted by priority)
    public static Sprite Exclamation
    {
        get
        {
            if (_exclamation == null) _exclamation = MakeBadge(false);
            return _exclamation;
        }
    }

    // A white circle with a dark up-arrow (for "you can afford an upgrade")
    public static Sprite UpArrow
    {
        get
        {
            if (_upArrow == null) _upArrow = MakeBadge(true);
            return _upArrow;
        }
    }

    // A 1 x 1 unit white square, centered
    public static Sprite WhiteCenter
    {
        get
        {
            if (_whiteCenter == null) _whiteCenter = MakeWhite(new Vector2(0.5f, 0.5f));
            return _whiteCenter;
        }
    }

    // A 1 x 1 unit white square whose pivot is its left edge (for bars that grow to the right)
    public static Sprite WhiteLeft
    {
        get
        {
            if (_whiteLeft == null) _whiteLeft = MakeWhite(new Vector2(0f, 0.5f));
            return _whiteLeft;
        }
    }

    // A rounded rectangle for UI panels (stretches without distorting its corners)
    public static Sprite RoundedRect
    {
        get
        {
            if (_rounded == null) _rounded = MakeRounded();
            return _rounded;
        }
    }

    // 0 = happy, 1 = neutral, 2 = angry, 3 = furious
    public static Sprite Face(int level)
    {
        level = Mathf.Clamp(level, 0, 3);
        if (_faces[level] == null) _faces[level] = MakeFace(level);
        return _faces[level];
    }

    // ---- Builders ----

    static Sprite MakeBadge(bool arrow)
    {
        Color[] px = NewPixels();
        Circle(px, 32f, 32f, 30f, Color.white);

        Color ink = new Color(0.2f, 0.2f, 0.25f);
        if (arrow)
        {
            Segment(px, 32f, 16f, 32f, 44f, 7f, ink);
            Segment(px, 32f, 50f, 17f, 34f, 7f, ink);
            Segment(px, 32f, 50f, 47f, 34f, 7f, ink);
        }
        else
        {
            Segment(px, 32f, 28f, 32f, 50f, 9f, ink);
            Circle(px, 32f, 16f, 5.5f, ink);
        }
        return ToSprite(px);
    }

    static Sprite MakeFace(int level)
    {
        Color[] backgrounds =
        {
            new Color(0.45f, 0.85f, 0.45f),
            new Color(0.98f, 0.85f, 0.35f),
            new Color(1f, 0.6f, 0.25f),
            new Color(0.95f, 0.3f, 0.28f)
        };
        Color back = backgrounds[level];
        Color rim = Color.Lerp(back, Color.black, 0.35f);
        Color ink = new Color(0.2f, 0.12f, 0.12f);

        Color[] px = NewPixels();
        Circle(px, 32f, 32f, 30f, rim);
        Circle(px, 32f, 32f, 27f, back);

        // Eyes
        Circle(px, 23f, 38f, 4f, ink);
        Circle(px, 41f, 38f, 4f, ink);

        // Mouth
        switch (level)
        {
            case 0: Mouth(px, 21f, 9f, ink); break;                    // smile
            case 1: Segment(px, 22f, 24f, 42f, 24f, 4f, ink); break;   // flat
            case 2: Mouth(px, 26f, -7f, ink); break;                   // frown
            default: Mouth(px, 27f, -10f, ink); break;                 // big frown
        }

        // Angry eyebrows
        if (level >= 2)
        {
            float thickness = level == 3 ? 5f : 4f;
            Segment(px, 15f, 48f, 28f, 42f, thickness, ink);
            Segment(px, 49f, 48f, 36f, 42f, thickness, ink);
        }
        return ToSprite(px);
    }

    // A curved mouth: positive curve = smile (ends higher), negative = frown (ends lower)
    static void Mouth(Color[] px, float centerY, float curve, Color ink)
    {
        float previousX = 20f;
        float previousY = centerY + curve;
        for (int i = 1; i <= 8; i++)
        {
            float t = i / 8f;
            float x = 20f + 24f * t;
            float y = centerY + curve * (2f * t - 1f) * (2f * t - 1f);
            Segment(px, previousX, previousY, x, y, 4f, ink);
            previousX = x;
            previousY = y;
        }
    }

    static Sprite MakeWhite(Vector2 pivot)
    {
        var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        var pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 4, 4), pivot, 4f);
    }

    static Sprite MakeRounded()
    {
        const int size = 64;
        const int radius = 20;

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x, radius, size - 1 - radius);
                float cy = Mathf.Clamp(y, radius, size - 1 - radius);
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(radius - distance + 0.5f)));
            }
        }
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    // ---- Tiny drawing helpers ----

    static Color[] NewPixels()
    {
        return new Color[Size * Size];   // fully transparent
    }

    static Sprite ToSprite(Color[] px)
    {
        var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(px);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
    }

    static void Circle(Color[] px, float cx, float cy, float r, Color color)
    {
        int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - r - 1f));
        int x1 = Mathf.Min(Size - 1, Mathf.CeilToInt(cx + r + 1f));
        int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - r - 1f));
        int y1 = Mathf.Min(Size - 1, Mathf.CeilToInt(cy + r + 1f));

        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                float dx = x + 0.5f - cx;
                float dy = y + 0.5f - cy;
                float alpha = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                if (alpha > 0f) Blend(ref px[y * Size + x], color, alpha);
            }
        }
    }

    static void Segment(Color[] px, float ax, float ay, float bx, float by, float thickness, Color color)
    {
        var a = new Vector2(ax, ay);
        var b = new Vector2(bx, by);
        Vector2 ab = b - a;
        float lengthSquared = Mathf.Max(0.0001f, ab.sqrMagnitude);

        int x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ax, bx) - thickness));
        int x1 = Mathf.Min(Size - 1, Mathf.CeilToInt(Mathf.Max(ax, bx) + thickness));
        int y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ay, by) - thickness));
        int y1 = Mathf.Min(Size - 1, Mathf.CeilToInt(Mathf.Max(ay, by) + thickness));

        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSquared);
                float distance = (p - (a + ab * t)).magnitude;
                float alpha = Mathf.Clamp01(thickness * 0.5f - distance + 0.5f);
                if (alpha > 0f) Blend(ref px[y * Size + x], color, alpha);
            }
        }
    }

    static void Blend(ref Color destination, Color source, float alpha)
    {
        float outAlpha = alpha + destination.a * (1f - alpha);
        destination = new Color(
            (source.r * alpha + destination.r * destination.a * (1f - alpha)) / outAlpha,
            (source.g * alpha + destination.g * destination.a * (1f - alpha)) / outAlpha,
            (source.b * alpha + destination.b * destination.a * (1f - alpha)) / outAlpha,
            outAlpha);
    }
}
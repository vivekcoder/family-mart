using UnityEngine;

public class CharacterStyle
{
    public Color shirt;
    public Color skin;
    public Color hair;
    public Color cap;
    public Color apron = Color.white;
    public bool hasHair;
    public bool hasCap;
    public bool hasChefHat;
    public bool hasApron;
    public float size = 1f;          // overall scale
    public float torsoWidth = 1f;    // slimmer or rounder body

    static readonly Color[] Skins =
    {
        new Color(0.96f, 0.8f, 0.69f), new Color(0.87f, 0.67f, 0.5f), new Color(0.74f, 0.52f, 0.36f),
        new Color(0.55f, 0.37f, 0.25f), new Color(0.4f, 0.26f, 0.18f)
    };

    static readonly Color[] Hairs =
    {
        new Color(0.1f, 0.08f, 0.07f), new Color(0.3f, 0.18f, 0.1f), new Color(0.55f, 0.35f, 0.15f),
        new Color(0.85f, 0.7f, 0.35f), new Color(0.5f, 0.5f, 0.52f)
    };

    static readonly Color[] Shirts =
    {
        new Color(0.95f, 0.45f, 0.4f), new Color(0.95f, 0.75f, 0.3f), new Color(0.45f, 0.78f, 0.5f),
        new Color(0.4f, 0.7f, 0.9f), new Color(0.7f, 0.55f, 0.9f), new Color(0.95f, 0.6f, 0.75f),
        new Color(0.9f, 0.9f, 0.85f)
    };

    public static CharacterStyle Player()
    {
        return new CharacterStyle
        {
            shirt = new Color(0.2f, 0.5f, 0.95f),
            skin = Skins[1],
            hair = Hairs[0],
            cap = new Color(0.9f, 0.25f, 0.2f),
            hasCap = true
        };
    }

    public static CharacterStyle RandomCustomer()
    {
        float roll = Random.value;
        return new CharacterStyle
        {
            shirt = Shirts[Random.Range(0, Shirts.Length)],
            skin = Skins[Random.Range(0, Skins.Length)],
            hair = Hairs[Random.Range(0, Hairs.Length)],
            cap = Shirts[Random.Range(0, Shirts.Length)],
            hasHair = roll < 0.6f,
            hasCap = roll >= 0.6f && roll < 0.85f,
            size = Random.Range(0.92f, 1.08f),
            torsoWidth = Random.Range(0.9f, 1.2f)
        };
    }
}

public static class CharacterBuilder
{
    static readonly Color Pants = new Color(0.2f, 0.24f, 0.34f);
    static readonly Color Ink = new Color(0.12f, 0.1f, 0.12f);

    // Builds a character standing at 'parent's origin, facing +Z. Returns the shirt renderer.
    public static Renderer Build(Transform parent, CharacterStyle style)
    {
        var visual = new GameObject("Character").transform;
        visual.SetParent(parent, false);
        visual.localScale = Vector3.one * style.size;

        float width = style.torsoWidth;

        // Legs
        Transform legL = Pivot(visual, new Vector3(-0.14f, 0.5f, 0f));
        Transform legR = Pivot(visual, new Vector3(0.14f, 0.5f, 0f));
        Look.Part(legL, PrimitiveType.Cube, Pants, new Vector3(0f, -0.25f, 0f), new Vector3(0.2f, 0.5f, 0.22f));
        Look.Part(legR, PrimitiveType.Cube, Pants, new Vector3(0f, -0.25f, 0f), new Vector3(0.2f, 0.5f, 0.22f));

        // Torso and a big round head
        Transform body = Pivot(visual, Vector3.zero);
        Transform torso = Look.Part(body, PrimitiveType.Capsule, style.shirt,
            new Vector3(0f, 0.88f, 0f), new Vector3(0.56f * width, 0.36f, 0.42f * width));
        Look.Part(body, PrimitiveType.Sphere, style.skin, new Vector3(0f, 1.52f, 0f), Vector3.one * 0.56f);

        // Face: big eyes with pupils, rosy cheeks, a little smile
        foreach (float x in new[] { -0.1f, 0.1f })
        {
            Look.Part(body, PrimitiveType.Sphere, Color.white, new Vector3(x, 1.56f, 0.2f), Vector3.one * 0.14f);
            Look.Part(body, PrimitiveType.Sphere, Ink, new Vector3(x, 1.56f, 0.26f), Vector3.one * 0.075f);
            Look.Part(body, PrimitiveType.Sphere, Color.Lerp(style.skin, new Color(1f, 0.4f, 0.4f), 0.5f),
                new Vector3(x * 1.9f, 1.46f, 0.2f), Vector3.one * 0.08f);
        }
        Look.Part(body, PrimitiveType.Cube, Ink, new Vector3(0f, 1.42f, 0.27f), new Vector3(0.12f, 0.03f, 0.03f));

        // Arms
        float shoulder = 0.34f * width + 0.04f;
        Transform armL = Pivot(body, new Vector3(-shoulder, 1.1f, 0f));
        Transform armR = Pivot(body, new Vector3(shoulder, 1.1f, 0f));
        foreach (Transform arm in new[] { armL, armR })
        {
            Look.Part(arm, PrimitiveType.Capsule, style.shirt, new Vector3(0f, -0.2f, 0f), new Vector3(0.15f, 0.24f, 0.15f));
            Look.Part(arm, PrimitiveType.Sphere, style.skin, new Vector3(0f, -0.43f, 0f), Vector3.one * 0.13f);
        }

        // Clothing and headwear
        if (style.hasApron)
            Look.Part(body, PrimitiveType.Cube, style.apron, new Vector3(0f, 0.8f, 0.22f * width), new Vector3(0.5f * width, 0.46f, 0.05f));

        if (style.hasHair)
            Look.Part(body, PrimitiveType.Sphere, style.hair, new Vector3(0f, 1.66f, -0.03f), new Vector3(0.6f, 0.34f, 0.6f));

        if (style.hasCap)
        {
            Look.Part(body, PrimitiveType.Sphere, style.cap, new Vector3(0f, 1.66f, 0f), new Vector3(0.6f, 0.34f, 0.6f));
            Look.Part(body, PrimitiveType.Cube, style.cap, new Vector3(0f, 1.6f, 0.29f), new Vector3(0.34f, 0.04f, 0.26f));
        }

        if (style.hasChefHat)
        {
            Look.Part(body, PrimitiveType.Cylinder, Color.white, new Vector3(0f, 1.8f, 0f), new Vector3(0.4f, 0.12f, 0.4f));
            Look.Part(body, PrimitiveType.Sphere, Color.white, new Vector3(0f, 2f, 0f), new Vector3(0.56f, 0.38f, 0.56f));
        }

        var animator = visual.gameObject.AddComponent<CharacterAnimator>();
        animator.body = body;
        animator.armL = armL;
        animator.armR = armR;
        animator.legL = legL;
        animator.legR = legR;

        return torso.GetComponent<Renderer>();
    }

    static Transform Pivot(Transform parent, Vector3 localPosition)
    {
        var pivot = new GameObject("Pivot").transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = localPosition;
        return pivot;
    }
}
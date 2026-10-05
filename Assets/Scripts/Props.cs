using UnityEngine;

public enum CropVisual { Bush, Stalks }
public enum BuildingLook { Tank, Mill, Oven, Chicken, Cow }

public static class Props
{
    // ---- Plants ----

    public static void DecoratePlant(Transform root, CropDef crop)
    {
        Transform bush = root.Find("Bush");

        if (crop.visual == CropVisual.Stalks)
        {
            // Turn the bush into a flat mound of earth and grow golden stalks on it
            if (bush != null)
            {
                bush.localPosition = new Vector3(0f, 0.15f, 0f);
                bush.localScale = new Vector3(1.9f, 0.3f, 1.9f);
                bush.GetComponent<Renderer>().sharedMaterial = Look.Mat(new Color(0.45f, 0.3f, 0.18f));
            }

            Color stalk = new Color(0.88f, 0.72f, 0.25f);
            Color ear = new Color(0.95f, 0.82f, 0.35f);
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2f / 12f;
                float radius = 0.35f + (i % 3) * 0.2f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0.65f, Mathf.Sin(angle) * radius);
                Look.Part(root, PrimitiveType.Cylinder, stalk, position, new Vector3(0.05f, 0.5f, 0.05f));
                Look.Part(root, PrimitiveType.Sphere, ear, position + Vector3.up * 0.55f, new Vector3(0.1f, 0.2f, 0.1f));
            }
            return;
        }

        // Tomato bush: leafy clumps around the core, and a wooden stake
        Color leaf = new Color(0.18f, 0.5f, 0.2f);
        Color darkLeaf = new Color(0.12f, 0.38f, 0.16f);
        for (int i = 0; i < 5; i++)
        {
            float angle = i * Mathf.PI * 2f / 5f;
            Vector3 position = new Vector3(Mathf.Cos(angle) * 0.45f, 0.9f + (i % 2) * 0.2f, Mathf.Sin(angle) * 0.45f);
            Look.Part(root, PrimitiveType.Sphere, i % 2 == 0 ? leaf : darkLeaf, position, Vector3.one * 0.55f);
        }
        Look.Part(root, PrimitiveType.Cylinder, new Color(0.6f, 0.45f, 0.28f), new Vector3(0.55f, 0.9f, 0.1f), new Vector3(0.06f, 0.8f, 0.06f));
    }

    // ---- Checkout counter ----

    public static void DecorateCounter(Transform root)
    {
        Color top = new Color(0.86f, 0.76f, 0.6f);
        Color red = new Color(0.88f, 0.3f, 0.28f);
        Color white = new Color(0.97f, 0.95f, 0.9f);
        Color post = new Color(0.45f, 0.3f, 0.2f);

        // Countertop slab and paper bags
        Look.Part(root, PrimitiveType.Cube, top, new Vector3(0f, 1.02f, 0f), new Vector3(1.9f, 0.1f, 1.9f));
        Look.Part(root, PrimitiveType.Cube, new Color(0.9f, 0.8f, 0.6f), new Vector3(0.55f, 1.25f, -0.45f), new Vector3(0.3f, 0.35f, 0.2f));
        Look.Part(root, PrimitiveType.Cube, new Color(0.9f, 0.8f, 0.6f), new Vector3(0.2f, 1.2f, -0.55f), new Vector3(0.26f, 0.26f, 0.18f));

        // Striped awning
        Look.Part(root, PrimitiveType.Cylinder, post, new Vector3(-0.85f, 1.6f, 0.8f), new Vector3(0.08f, 0.6f, 0.08f));
        Look.Part(root, PrimitiveType.Cylinder, post, new Vector3(0.85f, 1.6f, 0.8f), new Vector3(0.08f, 0.6f, 0.08f));
        for (int i = 0; i < 4; i++)
        {
            float x = -0.7125f + i * 0.475f;
            Look.Part(root, PrimitiveType.Cube, i % 2 == 0 ? red : white, new Vector3(x, 2.22f, 0.4f), new Vector3(0.475f, 0.08f, 1.5f));
        }
    }

    // ---- Machines ----

    // ---- Animals (they are machines too: feed in, product out) ----

    public static void SizeCollider(GameObject root, BuildingLook look)
    {
        var box = root.GetComponent<BoxCollider>();
        if (box == null) return;

        if (look == BuildingLook.Chicken)
        {
            box.center = new Vector3(0f, 0.5f, 0f);
            box.size = new Vector3(2f, 1f, 1.8f);
        }
        else if (look == BuildingLook.Cow)
        {
            box.center = new Vector3(0f, 0.7f, 0f);
            box.size = new Vector3(2.8f, 1.4f, 2.2f);
        }
    }

    public static float ZoneRadius(BuildingLook look)
    {
        return look == BuildingLook.Chicken || look == BuildingLook.Cow ? 2.2f : 2.6f;
    }

    public static void BuildChicken(Transform t, BuildingDef def, out Transform fill, out Vector3 inputPos, out Vector3 outputPos)
    {
        Color straw = new Color(0.9f, 0.78f, 0.4f);
        Color wood = new Color(0.62f, 0.45f, 0.28f);
        Color coop = new Color(0.8f, 0.3f, 0.25f);
        Color orange = new Color(1f, 0.6f, 0.15f);
        Color red = new Color(0.9f, 0.15f, 0.15f);
        Color dark = new Color(0.1f, 0.1f, 0.12f);

        // Straw bed, fence, and a little coop at the back
        Look.Part(t, PrimitiveType.Cube, straw, new Vector3(0f, 0.05f, 0f), new Vector3(2f, 0.1f, 1.8f));
        Look.Part(t, PrimitiveType.Cube, wood, new Vector3(0f, 0.35f, 0.85f), new Vector3(2f, 0.5f, 0.06f));
        Look.Part(t, PrimitiveType.Cube, wood, new Vector3(-0.97f, 0.35f, 0f), new Vector3(0.06f, 0.5f, 1.7f));
        Look.Part(t, PrimitiveType.Cube, wood, new Vector3(0.97f, 0.35f, 0f), new Vector3(0.06f, 0.5f, 1.7f));
        Look.Part(t, PrimitiveType.Cube, coop, new Vector3(-0.45f, 0.55f, 0.5f), new Vector3(0.9f, 0.9f, 0.6f));
        Transform roof = Look.Part(t, PrimitiveType.Cube, wood, new Vector3(-0.45f, 1.05f, 0.5f), new Vector3(1.05f, 0.08f, 0.75f));
        roof.localRotation = Quaternion.Euler(0f, 0f, 8f);
        Look.Part(t, PrimitiveType.Cube, new Color(0.2f, 0.12f, 0.1f), new Vector3(-0.45f, 0.4f, 0.19f), new Vector3(0.3f, 0.4f, 0.04f));

        // The chicken
        Transform body = Look.Part(t, PrimitiveType.Sphere, Color.white, new Vector3(0.4f, 0.5f, 0.05f), new Vector3(0.5f, 0.42f, 0.62f));
        Transform head = Look.Part(t, PrimitiveType.Sphere, Color.white, new Vector3(0.4f, 0.85f, -0.27f), Vector3.one * 0.26f);
        Look.Part(t, PrimitiveType.Cube, orange, new Vector3(0.4f, 0.82f, -0.42f), new Vector3(0.07f, 0.06f, 0.12f));
        Look.Part(t, PrimitiveType.Cube, red, new Vector3(0.4f, 1f, -0.27f), new Vector3(0.05f, 0.12f, 0.16f));
        Look.Part(t, PrimitiveType.Sphere, dark, new Vector3(0.32f, 0.88f, -0.38f), Vector3.one * 0.05f);
        Look.Part(t, PrimitiveType.Sphere, dark, new Vector3(0.48f, 0.88f, -0.38f), Vector3.one * 0.05f);
        Look.Part(t, PrimitiveType.Cube, Color.white, new Vector3(0.4f, 0.68f, 0.38f), new Vector3(0.3f, 0.3f, 0.1f));
        Look.Part(t, PrimitiveType.Cylinder, orange, new Vector3(0.3f, 0.12f, 0.05f), new Vector3(0.05f, 0.12f, 0.05f));
        Look.Part(t, PrimitiveType.Cylinder, orange, new Vector3(0.5f, 0.12f, 0.05f), new Vector3(0.05f, 0.12f, 0.05f));

        // Feed bowl and the egg tray in front
        Look.Part(t, PrimitiveType.Cylinder, wood, new Vector3(-0.4f, 0.14f, -0.25f), new Vector3(0.4f, 0.05f, 0.4f));
        Look.Part(t, PrimitiveType.Cube, wood, new Vector3(0f, 0.45f, -1.05f), new Vector3(2f, 0.1f, 0.6f));

        var idle = t.gameObject.AddComponent<AnimalIdle>();
        idle.body = body;
        idle.head = head;
        idle.amplitude = 0.06f;

        fill = ProgressBar(t);
        inputPos = new Vector3(-0.4f, 0.3f, -0.25f);
        outputPos = new Vector3(0f, 0.5f, -1.05f);
    }

    public static void BuildCow(Transform t, BuildingDef def, out Transform fill, out Vector3 inputPos, out Vector3 outputPos)
    {
        Color straw = new Color(0.9f, 0.78f, 0.4f);
        Color wood = new Color(0.62f, 0.45f, 0.28f);
        Color white = new Color(0.96f, 0.96f, 0.94f);
        Color black = new Color(0.15f, 0.15f, 0.17f);
        Color pink = new Color(0.95f, 0.65f, 0.7f);
        Color horn = new Color(0.9f, 0.85f, 0.7f);

        // Hay bed and fence
        Look.Part(t, PrimitiveType.Cube, straw, new Vector3(0f, 0.04f, 0f), new Vector3(2.8f, 0.08f, 2.2f));
        Look.Part(t, PrimitiveType.Cube, wood, new Vector3(0f, 0.4f, 1.05f), new Vector3(2.8f, 0.55f, 0.06f));
        Look.Part(t, PrimitiveType.Cube, wood, new Vector3(-1.37f, 0.4f, 0f), new Vector3(0.06f, 0.55f, 2f));
        Look.Part(t, PrimitiveType.Cube, wood, new Vector3(1.37f, 0.4f, 0f), new Vector3(0.06f, 0.55f, 2f));

        // The cow: body with spots, head, snout, ears, horns, legs, udder and tail
        Transform body = Look.Part(t, PrimitiveType.Cube, white, new Vector3(0f, 0.95f, 0.15f), new Vector3(1f, 0.8f, 1.6f));
        Look.Part(t, PrimitiveType.Cube, black, new Vector3(0f, 1f, 0f), new Vector3(1.02f, 0.4f, 0.5f));
        Look.Part(t, PrimitiveType.Cube, black, new Vector3(0.3f, 1.34f, 0.5f), new Vector3(0.4f, 0.04f, 0.4f));
        Look.Part(t, PrimitiveType.Cube, black, new Vector3(-0.51f, 0.95f, 0.5f), new Vector3(0.04f, 0.35f, 0.35f));
        Transform head = Look.Part(t, PrimitiveType.Cube, white, new Vector3(0f, 1.15f, -0.85f), new Vector3(0.55f, 0.5f, 0.5f));
        Look.Part(t, PrimitiveType.Cube, pink, new Vector3(0f, 1.05f, -1.12f), new Vector3(0.4f, 0.25f, 0.16f));
        Look.Part(t, PrimitiveType.Sphere, black, new Vector3(-0.15f, 1.25f, -1.1f), Vector3.one * 0.06f);
        Look.Part(t, PrimitiveType.Sphere, black, new Vector3(0.15f, 1.25f, -1.1f), Vector3.one * 0.06f);
        Look.Part(t, PrimitiveType.Cube, black, new Vector3(-0.35f, 1.3f, -0.8f), new Vector3(0.2f, 0.08f, 0.14f));
        Look.Part(t, PrimitiveType.Cube, black, new Vector3(0.35f, 1.3f, -0.8f), new Vector3(0.2f, 0.08f, 0.14f));
        Look.Part(t, PrimitiveType.Cylinder, horn, new Vector3(-0.2f, 1.45f, -0.8f), new Vector3(0.07f, 0.1f, 0.07f));
        Look.Part(t, PrimitiveType.Cylinder, horn, new Vector3(0.2f, 1.45f, -0.8f), new Vector3(0.07f, 0.1f, 0.07f));
        foreach (float x in new[] { -0.33f, 0.33f })
        {
            Look.Part(t, PrimitiveType.Cylinder, white, new Vector3(x, 0.3f, 0.7f), new Vector3(0.17f, 0.3f, 0.17f));
            Look.Part(t, PrimitiveType.Cylinder, white, new Vector3(x, 0.3f, -0.4f), new Vector3(0.17f, 0.3f, 0.17f));
        }
        Look.Part(t, PrimitiveType.Sphere, pink, new Vector3(0f, 0.5f, 0.55f), new Vector3(0.3f, 0.22f, 0.3f));
        Look.Part(t, PrimitiveType.Cylinder, black, new Vector3(0f, 1f, 0.98f), new Vector3(0.05f, 0.35f, 0.05f));

        // Trough and the milk tray in front
        Look.Part(t, PrimitiveType.Cube, wood, new Vector3(0.85f, 0.25f, -0.85f), new Vector3(0.8f, 0.22f, 0.4f));
        Look.Part(t, PrimitiveType.Cube, wood, new Vector3(0f, 0.45f, -1.6f), new Vector3(2.2f, 0.1f, 0.6f));

        var idle = t.gameObject.AddComponent<AnimalIdle>();
        idle.body = body;
        idle.head = head;
        idle.amplitude = 0.03f;

        fill = ProgressBar(t);
        inputPos = new Vector3(0.85f, 0.4f, -0.85f);
        outputPos = new Vector3(0f, 0.5f, -1.6f);
    }

    // ---- Shopping cart ----

    // Builds a cart in front of a character. Returns the spot where products sit inside it.
    public static Transform BuildCart(Transform parent)
    {
        var cart = new GameObject("Cart").transform;
        cart.SetParent(parent, false);
        cart.localPosition = new Vector3(0f, 0f, 1.05f);

        Color basket = new Color(0.85f, 0.3f, 0.28f);
        Color metal = new Color(0.75f, 0.78f, 0.82f);
        Color dark = new Color(0.25f, 0.27f, 0.3f);

        Look.Part(cart, PrimitiveType.Cube, basket, new Vector3(0f, 0.32f, 0f), new Vector3(0.85f, 0.08f, 0.65f));
        Look.Part(cart, PrimitiveType.Cube, basket, new Vector3(0f, 0.5f, 0.31f), new Vector3(0.85f, 0.32f, 0.05f));
        Look.Part(cart, PrimitiveType.Cube, basket, new Vector3(0f, 0.5f, -0.31f), new Vector3(0.85f, 0.32f, 0.05f));
        Look.Part(cart, PrimitiveType.Cube, basket, new Vector3(-0.4f, 0.5f, 0f), new Vector3(0.05f, 0.32f, 0.65f));
        Look.Part(cart, PrimitiveType.Cube, basket, new Vector3(0.4f, 0.5f, 0f), new Vector3(0.05f, 0.32f, 0.65f));

        // Handle on the customer's side
        Look.Part(cart, PrimitiveType.Cube, metal, new Vector3(0f, 0.8f, -0.36f), new Vector3(0.85f, 0.05f, 0.05f));
        Look.Part(cart, PrimitiveType.Cube, metal, new Vector3(-0.4f, 0.6f, -0.36f), new Vector3(0.05f, 0.4f, 0.05f));
        Look.Part(cart, PrimitiveType.Cube, metal, new Vector3(0.4f, 0.6f, -0.36f), new Vector3(0.05f, 0.4f, 0.05f));

        // Four wheels
        foreach (float x in new[] { -0.36f, 0.36f })
        {
            foreach (float z in new[] { -0.25f, 0.25f })
            {
                Transform wheel = Look.Part(cart, PrimitiveType.Cylinder, dark, new Vector3(x, 0.14f, z), new Vector3(0.16f, 0.03f, 0.16f));
                wheel.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }

        var slots = new GameObject("Cart Slots").transform;
        slots.SetParent(cart, false);
        slots.localPosition = new Vector3(0f, 0.36f, 0f);
        return slots;
    }

    static Transform ProgressBar(Transform t)
    {
        Look.Part(t, PrimitiveType.Cube, new Color(0.22f, 0.25f, 0.3f), new Vector3(0f, 2.75f, 0f), new Vector3(1.6f, 0.14f, 0.08f));
        return Look.Part(t, PrimitiveType.Cube, new Color(0.4f, 0.9f, 0.4f), new Vector3(-0.75f, 2.75f, -0.02f), new Vector3(0.001f, 0.1f, 0.08f));
    }

    public static void BuildMill(Transform t, BuildingDef def, out Transform fill, out Vector3 inputPos, out Vector3 outputPos)
    {
        Color stone = new Color(0.62f, 0.6f, 0.58f);
        Color roof = new Color(0.55f, 0.3f, 0.2f);
        Color wood = new Color(0.78f, 0.58f, 0.36f);

        Look.Part(t, PrimitiveType.Cube, stone, new Vector3(0f, 0.45f, 0f), new Vector3(2.2f, 0.9f, 1.8f));
        Look.Part(t, PrimitiveType.Cylinder, def.color, new Vector3(0f, 1.55f, 0f), new Vector3(1.4f, 0.65f, 1.4f));
        Look.Part(t, PrimitiveType.Cylinder, roof, new Vector3(0f, 2.28f, 0f), new Vector3(1.6f, 0.08f, 1.6f));
        Look.Part(t, PrimitiveType.Sphere, roof, new Vector3(0f, 2.3f, 0f), new Vector3(0.9f, 0.5f, 0.9f));

        // Windmill blades on the front of the tower
        Transform bladeA = Look.Part(t, PrimitiveType.Cube, wood, new Vector3(0f, 1.6f, -0.78f), new Vector3(1.9f, 0.14f, 0.05f));
        bladeA.localRotation = Quaternion.Euler(0f, 0f, 25f);
        Transform bladeB = Look.Part(t, PrimitiveType.Cube, wood, new Vector3(0f, 1.6f, -0.78f), new Vector3(1.9f, 0.14f, 0.05f));
        bladeB.localRotation = Quaternion.Euler(0f, 0f, 115f);
        Look.Part(t, PrimitiveType.Sphere, roof, new Vector3(0f, 1.6f, -0.82f), Vector3.one * 0.2f);

        // Flour sacks and the output tray
        Look.Part(t, PrimitiveType.Cube, Color.white, new Vector3(1.35f, 0.2f, -0.5f), new Vector3(0.45f, 0.4f, 0.35f));
        Look.Part(t, PrimitiveType.Cube, Color.white, new Vector3(1.35f, 0.2f, 0f), new Vector3(0.45f, 0.4f, 0.35f));
        Look.Part(t, PrimitiveType.Cube, wood, new Vector3(0f, 0.5f, -1.15f), new Vector3(1.5f, 0.1f, 0.7f));

        fill = ProgressBar(t);
        inputPos = new Vector3(0f, 2.5f, 0f);
        outputPos = new Vector3(0f, 0.55f, -1.15f);
    }

    public static void BuildOven(Transform t, BuildingDef def, out Transform fill, out Vector3 inputPos, out Vector3 outputPos)
    {
        Color darkBrick = new Color(0.45f, 0.2f, 0.15f);
        Color coal = new Color(0.15f, 0.12f, 0.12f);
        Color wood = new Color(0.78f, 0.58f, 0.36f);

        Look.Part(t, PrimitiveType.Cube, def.color, new Vector3(0f, 0.5f, 0f), new Vector3(2.2f, 1f, 1.8f));
        Look.Part(t, PrimitiveType.Sphere, def.color, new Vector3(0f, 1.15f, 0f), new Vector3(1.9f, 1.3f, 1.5f));
        Look.Part(t, PrimitiveType.Cylinder, darkBrick, new Vector3(0.75f, 2f, 0.4f), new Vector3(0.35f, 0.45f, 0.35f));
        Look.Part(t, PrimitiveType.Cube, coal, new Vector3(0f, 0.62f, -0.92f), new Vector3(0.9f, 0.55f, 0.1f));
        Look.Part(t, PrimitiveType.Cube, new Color(1f, 0.55f, 0.15f), new Vector3(0f, 0.58f, -0.97f), new Vector3(0.65f, 0.35f, 0.05f));
        Look.Part(t, PrimitiveType.Cube, darkBrick, new Vector3(0f, 1.03f, -0.92f), new Vector3(1.1f, 0.1f, 0.12f));
        Look.Part(t, PrimitiveType.Cube, wood, new Vector3(0f, 0.5f, -1.15f), new Vector3(1.5f, 0.1f, 0.7f));

        fill = ProgressBar(t);
        inputPos = new Vector3(0f, 2f, 0f);
        outputPos = new Vector3(0f, 0.55f, -1.15f);
    }
}
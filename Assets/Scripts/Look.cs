using System.Collections.Generic;
using UnityEngine;

public static class Look
{
    public static readonly Color Sky = new Color(0.62f, 0.82f, 0.95f);
    public static readonly Color FarmDirt = new Color(0.56f, 0.42f, 0.28f);

    static readonly Color Grass = new Color(0.45f, 0.72f, 0.38f);
    static readonly Color PathColor = new Color(0.62f, 0.6f, 0.58f);
    static readonly Color WallColor = new Color(0.9f, 0.86f, 0.8f);
    static readonly Color FloorA = new Color(0.86f, 0.8f, 0.7f);
    static readonly Color FloorB = new Color(0.8f, 0.73f, 0.62f);

    static readonly Dictionary<Color, Material> _materials = new Dictionary<Color, Material>();
    static Material _template;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        _materials.Clear();
        _template = null;
    }

    // One shared material per color (fewer materials = faster rendering)
    public static Material Mat(Color color)
    {
        if (_materials.TryGetValue(color, out Material cached) && cached != null) return cached;

        if (_template == null)
        {
            // Borrow the project's default material so we don't depend on Shader.Find
            var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _template = temp.GetComponent<Renderer>().sharedMaterial;
            Object.DestroyImmediate(temp);
        }

        var material = new Material(_template);
        material.color = color;
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.15f);
        _materials[color] = material;
        return material;
    }

    // Adds a collider-less piece as a child. Cubes become softly rounded boxes.
    public static Transform Part(Transform parent, PrimitiveType type, Color color, Vector3 localPosition, Vector3 localScale)
    {
        // Very thin pieces stay plain so they can be resized later (progress bars)
        bool rounded = type == PrimitiveType.Cube
            && localScale.x >= 0.02f && localScale.y >= 0.02f && localScale.z >= 0.02f;
        GameObject go = rounded
            ? ProcMesh.MakeRoundedBox(localScale, color)
            : GameBootstrap.MakePrimitive(type, "Part", color, false);
        if (rounded) localScale = Vector3.one;   // the size is baked into the mesh

        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;
        return go.transform;
    }

    public static Material FloorMaterial()
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.SetPixels(new[] { FloorA, FloorB, FloorB, FloorA });
        texture.Apply();

        var material = new Material(Mat(Color.white));
        material.mainTexture = texture;
        material.mainTextureScale = new Vector2(10f, 10f);   // 20 x 20 floor = 1-unit tiles
        return material;
    }

    public static void SetupScene()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.62f, 0.64f, 0.7f);

        Light sun = Object.FindFirstObjectByType<Light>();
        if (sun == null)
        {
            sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
        }
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        sun.color = new Color(1f, 0.96f, 0.88f);
        sun.intensity = 1.1f;
        sun.shadows = LightShadows.Soft;
    }

    // A huge grass field and the walkway from the street. (Walls come from the Zones system.)
    public static void BuildSurroundings()
    {
        // Layer 2 ("Ignore Raycast") is left out of the NavMesh
        var grass = GameBootstrap.MakePrimitive(PrimitiveType.Plane, "Grass", Grass, true);
        grass.layer = 2;
        grass.transform.position = new Vector3(0f, -0.03f, 0f);
        grass.transform.localScale = new Vector3(60f, 1f, 60f);

        var path = GameBootstrap.MakePrimitive(PrimitiveType.Plane, "Entrance Path", PathColor, true);
        path.transform.position = new Vector3(0f, 0f, -12.5f);
        path.transform.localScale = new Vector3(0.4f, 1f, 0.5f);
    }

    public static GameObject Wall(Vector3 groundPosition, Vector3 size)
    {
        var wall = GameBootstrap.MakePrimitive(PrimitiveType.Cube, "Wall", WallColor, true);
        wall.transform.position = groundPosition + Vector3.up * (size.y / 2f);
        wall.transform.localScale = size;
        return wall;
    }
}
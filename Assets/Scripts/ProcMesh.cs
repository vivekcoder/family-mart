using System.Collections.Generic;
using UnityEngine;

public static class ProcMesh
{
    static readonly Dictionary<Vector3Int, Mesh> _boxes = new Dictionary<Vector3Int, Mesh>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        _boxes.Clear();
    }

    public static GameObject MakeRoundedBox(Vector3 size, Color color)
    {
        var go = new GameObject("Part");
        go.AddComponent<MeshFilter>().sharedMesh = RoundedBox(size);
        go.AddComponent<MeshRenderer>().sharedMaterial = Look.Mat(color);
        return go;
    }

    // A box with softly rounded edges. Meshes are cached per size.
    public static Mesh RoundedBox(Vector3 size)
    {
        var key = new Vector3Int(
            Mathf.RoundToInt(size.x * 100f), Mathf.RoundToInt(size.y * 100f), Mathf.RoundToInt(size.z * 100f));
        if (_boxes.TryGetValue(key, out Mesh cached) && cached != null) return cached;

        Vector3 half = size * 0.5f;
        float radius = Mathf.Min(0.07f, 0.35f * Mathf.Min(size.x, Mathf.Min(size.y, size.z)));

        float[] xs = Lines(half.x, radius);
        float[] ys = Lines(half.y, radius);
        float[] zs = Lines(half.z, radius);
        int n = xs.Length;

        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();

        for (int axis = 0; axis < 3; axis++)
        {
            for (int sign = -1; sign <= 1; sign += 2)
            {
                float[] uLines = axis == 0 ? ys : xs;   // the two directions across this face
                float[] vLines = axis == 2 ? ys : zs;
                int start = vertices.Count;

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        float u = uLines[i];
                        float v = vLines[j];

                        Vector3 point;
                        Vector3 faceNormal;
                        if (axis == 0) { point = new Vector3(sign * half.x, u, v); faceNormal = new Vector3(sign, 0f, 0f); }
                        else if (axis == 1) { point = new Vector3(u, sign * half.y, v); faceNormal = new Vector3(0f, sign, 0f); }
                        else { point = new Vector3(u, v, sign * half.z); faceNormal = new Vector3(0f, 0f, sign); }

                        // Pull the point onto a rounded surface
                        Vector3 inner = new Vector3(
                            Mathf.Clamp(point.x, -half.x + radius, half.x - radius),
                            Mathf.Clamp(point.y, -half.y + radius, half.y - radius),
                            Mathf.Clamp(point.z, -half.z + radius, half.z - radius));
                        Vector3 direction = point - inner;
                        Vector3 normal = direction.sqrMagnitude > 1e-8f ? direction.normalized : faceNormal;

                        vertices.Add(inner + normal * radius);
                        normals.Add(normal);
                    }
                }

                // Make the triangles face outward
                Vector3 a = vertices[start];
                Vector3 b = vertices[start + n];
                Vector3 c = vertices[start + n + 1];
                bool flip = Vector3.Dot(Vector3.Cross(b - a, c - a), normals[start]) < 0f;

                for (int i = 0; i < n - 1; i++)
                {
                    for (int j = 0; j < n - 1; j++)
                    {
                        int v00 = start + i * n + j;
                        int v10 = start + (i + 1) * n + j;
                        int v11 = start + (i + 1) * n + j + 1;
                        int v01 = start + i * n + j + 1;

                        if (!flip)
                        {
                            triangles.Add(v00); triangles.Add(v10); triangles.Add(v11);
                            triangles.Add(v00); triangles.Add(v11); triangles.Add(v01);
                        }
                        else
                        {
                            triangles.Add(v00); triangles.Add(v11); triangles.Add(v10);
                            triangles.Add(v00); triangles.Add(v01); triangles.Add(v11);
                        }
                    }
                }
            }
        }

        var mesh = new Mesh();
        mesh.name = "Rounded Box";
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();

        _boxes[key] = mesh;
        return mesh;
    }

    // Sample positions along one axis: the edge, partway through the bevel, and the end of the bevel (both sides)
    static float[] Lines(float half, float radius)
    {
        return new[]
        {
            -half, -half + 0.3f * radius, -half + radius,
            half - radius, half - 0.3f * radius, half
        };
    }
}
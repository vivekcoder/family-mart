using System.Collections.Generic;
using UnityEngine;

public enum ZoneFloor { Tile, Dirt }

public class ZoneDef
{
    public readonly string id;
    public readonly string title;
    public readonly Vector2Int cell;   // position on the 20 x 20 grid; the starting mart is (0, 0)
    public readonly ZoneFloor floor;

    public ZoneDef(string id, string title, Vector2Int cell, ZoneFloor floor)
    {
        this.id = id;
        this.title = title;
        this.cell = cell;
        this.floor = floor;
    }
}

// The world is a grid of 20 x 20 areas. Walls are rebuilt wherever areas meet.
public static class Zones
{
    public const float CellSize = 20f;

    static readonly HashSet<Vector2Int> _built = new HashSet<Vector2Int>();
    static readonly Dictionary<string, GameObject> _walls = new Dictionary<string, GameObject>();
    static readonly HashSet<string> _wanted = new HashSet<string>();
    static readonly List<string> _stale = new List<string>();
    static readonly Vector2Int[] Sides = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        _built.Clear();
        _walls.Clear();
    }

    // The starting mart already has its floor; this registers it and builds the first walls
    public static void Begin()
    {
        _built.Clear();
        _walls.Clear();
        _built.Add(Vector2Int.zero);
        RebuildWalls(false);
    }

    public static Vector3 CellCenter(Vector2Int cell)
    {
        return new Vector3(cell.x * CellSize, 0f, cell.y * CellSize);
    }

    public static void Build(ZoneDef zone, bool animate)
    {
        if (_built.Contains(zone.cell)) return;
        _built.Add(zone.cell);

        var floor = GameBootstrap.MakePrimitive(PrimitiveType.Plane, "Zone Floor " + zone.id, Color.white, true);
        floor.GetComponent<Renderer>().sharedMaterial =
            zone.floor == ZoneFloor.Dirt ? Look.Mat(Look.FarmDirt) : Look.FloorMaterial();
        floor.transform.position = CellCenter(zone.cell);
        floor.transform.localScale = new Vector3(2f, 1f, 2f);
        if (animate) ConstructionPop.Play(floor, true, false, false);

        RebuildWalls(animate);
    }

    // Walls go around the outside of the built area only, so connected zones are open to each other
    static void RebuildWalls(bool animate)
    {
        _wanted.Clear();
        foreach (Vector2Int cell in _built)
        {
            for (int side = 0; side < 4; side++)
                if (!_built.Contains(cell + Sides[side])) EmitWalls(cell, side, animate);
        }

        _stale.Clear();
        foreach (var pair in _walls)
            if (!_wanted.Contains(pair.Key)) _stale.Add(pair.Key);

        foreach (string key in _stale)
        {
            if (_walls[key] != null) Object.DestroyImmediate(_walls[key]);
            _walls.Remove(key);
        }
    }

    static void EmitWalls(Vector2Int cell, int side, bool animate)
    {
        Vector3 c = CellCenter(cell);
        float half = CellSize / 2f;

        switch (side)
        {
            case 0:   // north
                Emit(cell, side, 0, new Vector3(c.x, 0f, c.z + half + 0.25f), new Vector3(21f, 0.7f, 0.5f), animate);
                break;
            case 1:   // east
                Emit(cell, side, 0, new Vector3(c.x + half + 0.25f, 0f, c.z), new Vector3(0.5f, 0.7f, 21f), animate);
                break;
            case 2:   // south (the starting mart has the entrance gap)
                if (cell == Vector2Int.zero)
                {
                    Emit(cell, side, 0, new Vector3(c.x - 6.25f, 0f, c.z - half - 0.25f), new Vector3(8.5f, 0.7f, 0.5f), animate);
                    Emit(cell, side, 1, new Vector3(c.x + 6.25f, 0f, c.z - half - 0.25f), new Vector3(8.5f, 0.7f, 0.5f), animate);
                }
                else
                {
                    Emit(cell, side, 0, new Vector3(c.x, 0f, c.z - half - 0.25f), new Vector3(21f, 0.7f, 0.5f), animate);
                }
                break;
            default:  // west
                Emit(cell, side, 0, new Vector3(c.x - half - 0.25f, 0f, c.z), new Vector3(0.5f, 0.7f, 21f), animate);
                break;
        }
    }

    static void Emit(Vector2Int cell, int side, int part, Vector3 position, Vector3 size, bool animate)
    {
        string key = cell.x + "," + cell.y + ":" + side + ":" + part;
        _wanted.Add(key);

        if (_walls.TryGetValue(key, out GameObject existing) && existing != null) return;

        GameObject wall = Look.Wall(position, size);
        _walls[key] = wall;
        if (animate) ConstructionPop.Play(wall, false, false, false);
    }
}
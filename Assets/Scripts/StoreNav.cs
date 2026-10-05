using Unity.AI.Navigation;
using UnityEngine;

public static class StoreNav
{
    static NavMeshSurface _surface;

    // Call after the floor and all obstacles exist (and before anything that shouldn't block paths)
    public static void Build()
    {
        if (_surface == null)
        {
            var go = new GameObject("NavMesh Surface");
            _surface = go.AddComponent<NavMeshSurface>();
            _surface.collectObjects = CollectObjects.All;
            _surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            _surface.layerMask = ~(1 << 2);
        }

        Physics.SyncTransforms();
        _surface.BuildNavMesh();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        _surface = null;
    }
}
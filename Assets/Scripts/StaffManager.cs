using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class StaffManager
{
    public static readonly List<StaffWorker> All = new List<StaffWorker>();

    // Set by the staff upgrades
    public static float SpeedMultiplier = 1f;
    public static int CarryBonus = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        All.Clear();
        SpeedMultiplier = 1f;
        CarryBonus = 0;
    }

    public static StaffWorker Hire(StaffDef def, Vector3 position)
    {
        var root = new GameObject(def.title);
        root.transform.position = position;

        // A random person in the role's uniform color, always wearing a cap
        CharacterStyle style = CharacterStyle.RandomCustomer();
        style.shirt = def.shirt;
        style.cap = Color.Lerp(def.shirt, Color.white, 0.35f);
        style.hasCap = true;
        style.hasHair = false;
        CharacterBuilder.Build(root.transform, style);

        var carry = new GameObject("Carry Root").transform;
        carry.SetParent(root.transform, false);
        carry.localPosition = new Vector3(0f, 0.5f, 0.9f);

        var agent = root.AddComponent<NavMeshAgent>();
        agent.radius = 0.4f;
        agent.height = 1.8f;
        agent.speed = def.moveSpeed;
        agent.acceleration = 14f;
        agent.stoppingDistance = 0.15f;
        agent.avoidancePriority = Random.Range(20, 50);

        var worker = root.AddComponent<StaffWorker>();
        worker.def = def;
        worker.carryRoot = carry;

        WorldLabel.Create(root.transform, new Vector3(0f, 2.5f, 0f)).text = def.title;

        SaveManager.RequestSave();
        return worker;
    }

    public static void Capture(List<StaffSave> into)
    {
        foreach (StaffWorker worker in All)
            into.Add(new StaffSave { defId = worker.def.id, position = worker.transform.position });
    }
}
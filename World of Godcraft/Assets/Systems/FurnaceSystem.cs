using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class FurnaceSystem : IEcsRunSystem
{
    [EcsFilter(typeof(FurnaceComponent))]
    EcsFilter filterFurnace = default;
    [EcsPool]
    EcsPool<FurnaceComponent> poolFurnace = default;

    Craft craft;
    PrefabsHolder prefabs;

    

    public void Run(EcsSystems systems)
    {
        if (craft == null)
            craft = Service<Craft>.Get();

        if (prefabs == null)
            prefabs = Service<PrefabsHolder>.Get();

        CheckCraft();
        UpdateFurnace();
    }

    void UpdateFurnace()
    {
        foreach (var entity in filterFurnace)
        {
            ref var furnace = ref poolFurnace.Get(entity);

            if (furnace.firing)
            {
                furnace.firingTime += Time.deltaTime;

                Debug.Log("горит сцуко " + (furnace.combustionTime - furnace.firingTime));

                if (furnace.firingTime > furnace.combustionTime)
                {
                    Debug.Log("сгорело");
                    furnace.firing = false;
                    furnace.firingTime = 0;
                }
            }
        }
    }

    void CheckCraft()
    {
        foreach (var entity in filterFurnace)
        {
            float? fireTime = null;

            ref var furnace = ref poolFurnace.Get(entity);

            if (furnace.combustible != null)
            {
                if (craft.setsCombustible.ContainsKey(furnace.combustible.Value.blockID))
                {
                    fireTime = craft.setsCombustible[furnace.combustible.Value.blockID];
                    furnace.combustionTime = fireTime.Value;
                }
            }

            if (fireTime != null && furnace.furnaceable != null)
            {
                if (craft.setsFurnaceable.ContainsKey(furnace.furnaceable.Value.blockID))
                {
                    var result = craft.setsFurnaceable[furnace.furnaceable.Value.blockID];

                    furnace.firing = true;
                    furnace.result = result;
                }
            }
        }
    }

    
}

using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class FurnaceSystem : IEcsRunSystem
{
    [EcsWorld]
    EcsWorld ecsWorld = default;
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
                // Горение топлива
                furnace.firingTime += Time.deltaTime;

                if (furnace.firingTime > furnace.combustionTime)
                {
                    var combustible = furnace.combustible.Value;
                    combustible.count--;
                    if(combustible.count == 0)
                    {
                        Object.Destroy(combustible.view);
                        furnace.combustible = null;
                    }
                    else
                    {
                        furnace.combustible = combustible;
                    }

                    furnace.firing = false;
                    furnace.firingTime = 0;
                }
                // Обжиг предмета
                if (!furnace.burning)
                    return;

                furnace.burningTime += Time.deltaTime;

                if (furnace.burningTime > furnace.result.fireTime)
                {
                    furnace.burning = false;

                    if (furnace.craftResult == null)
                    {
                        if (furnace.result.itemID == ITEMS.INGOT_IRON)
                        {
                            var ingot = Object.Instantiate(prefabs.ingotIron);
                            ingot.layer = 5;
                            ingot.transform.localScale = Vector3.one * 0.7f;

                            InventoryItem item;
                            item.blockID = furnace.result.itemID;
                            item.view = ingot;
                            item.count = 1;
                            item.itemType = ItemType.Item;
                            furnace.craftResult = item;
                        }
                    }
                    else
                    {
                        var item = furnace.craftResult.Value;
                        item.count++;
                        furnace.craftResult = item;
                    }

                    if (furnace.furnaceable != null)
                    {
                        var furnaceable = furnace.furnaceable.Value;
                        furnaceable.count--;
                        if (furnaceable.count == 0)
                        {
                            Object.Destroy(furnaceable.view);
                            furnace.furnaceable = null;
                        }
                        else
                        {
                            furnace.furnaceable = furnaceable;
                            furnace.burning = true;
                        }
                    }

                    furnace.burningTime = 0;
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

            if (furnace.firing)
                continue;

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
                    furnace.burning = true;
                    furnace.result = result;
                }
            }
        }
    }

    
}

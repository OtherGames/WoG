using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

sealed class WriteBlockSystem : IEcsRunSystem
{
    [EcsWorld]
    readonly EcsWorld world = default;

    [EcsFilter(typeof(ChunckHitEvent))]
    readonly EcsFilter hitFilter = default;

    private List<WritableBlock> writedBlocks = new();

    bool writing;

    public void Run(EcsSystems systems)
    {
        if (!HUD.WriteMode)
        {
            if (!writing)
            {
                return;
            }
            else
            {
                writing = false;
                SaveBlocks();
                return;
            }
        }

        writing = true;

        foreach (var hitEntity in hitFilter)
        {
            ref var hitComponent = ref world.GetPool<ChunckHitEvent>().Get(hitEntity);

            if (hitComponent.blockId == 0)
            {
                var pos = hitComponent.position;
                var block = writedBlocks.Find(b => b.pos == pos);

                if (writedBlocks.Contains(block))
                    writedBlocks.Remove(block);
            }
            else
            {
                writedBlocks.Add(new() 
                { 
                    pos = new Vector3Int((int)hitComponent.position.x, (int)hitComponent.position.y, (int)hitComponent.position.z),
                    id = hitComponent.blockId 
                });
            }
            
        }
    }

    void SaveBlocks()
    {
        string name = $"{Random.Range(100, 999)}";
     
        var json = Json.Serialize(writedBlocks, true);
        File.WriteAllText($"{Application.dataPath}/Data/{"tree"}.json", json);


        writedBlocks.Clear();
    }

    
}

[System.Serializable]
public struct WritableBlock
{
    public Vector3Int pos;
    public byte id;
}

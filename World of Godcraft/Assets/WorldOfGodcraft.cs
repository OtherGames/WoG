using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedSystems;
using UnityEngine;
using Leopotam.EcsLite.Di;
using LeopotamGroup.Globals;
using Client;

sealed class WorldOfGodcraft : MonoBehaviour
{
    public Material mat;
    [SerializeField]
    private Camera main;

    public EcsWorld EcsWorld { get; set; }
    EcsSystems systems;

    void Start()
    {
        main.gameObject.SetActive(false);

        var meshGenerator = new MeshGenerator();
        meshGenerator.Init();
        Service<MeshGenerator>.Set(meshGenerator);
        Service<PrefabsHolder>.Set(GetComponent<PrefabsHolder>());
        Service<DropedBlockGenerator>.Set(new());
        Service<Craft>.Set(new());

        var world = new World();
        EcsWorld = new EcsWorld();

        Service<World>.Set(world);
        // register your shared data here, for example:
        // var shared = new Shared ();
        // systems = new EcsSystems (new EcsWorld (), shared);
        systems = new EcsSystems(EcsWorld);
        systems
            // register your systems here, for example:
            .Add(new WorldGeneratorInit())
            .Add(new WorldRaycastHitSystem())
            .Add(new NetworkChunckChangeSendSystem())
            .Add(new WriteBlockSystem())
            .Add(new PlantsGeneratorSystem())
            .Add(new GenerateMorkvaSystem())
            .Add(new DropedLifetimeSystem())
            .Add(new DropConvertBlockToItemSystem())
            .Add(new TakeDropedSystem())
            .Add(new TakeBlockSystem())
            .Add(new SatietySystem())
            .Add(new TakeItemSystem())
            .Add(new UsingFoodSystem())
            .Add(new PlacedFurnaceSystem())
            .Add(new FurnaceSystem())
            .Add(new UsedItemUpdatedSystem())
            .Add(new GunFireRateSystem())
            .Add(new GunFireSystem())
            .Add(new ProjectileSystem())

            // register additional worlds here, for example:
            // .AddWorld (new EcsWorld (), "events")
#if UNITY_EDITOR
            // add debug systems for custom worlds here, for example:
            // .Add (new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem ("events"))
            .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
            //.DelHere<ChunckHitEvent>()
            .DelHere<NetworkChunckChanged>()
            .DelHere<ChunkInited>()
            .DelHere<DropedCreated>()
            .DelHere<BlockTaked>()
            .DelHere<ItemTaked>()
            .DelHere<ItemUsed>()
            .DelHere<BlockPlaced>()
            .DelHere<UsedItemUpdated>()
            .DelHere<GunFired>()

            .Inject(world)
            .Init();
    }

    void Update()
    {
        systems?.Run();
    }

    void OnDestroy()
    {
        if (systems != null)
        {
            systems.Destroy();
            // add here cleanup for custom worlds, for example:
            // _systems.GetWorld ("events").Destroy ();
            systems.GetWorld().Destroy();
            systems = null;
        }
    }
}

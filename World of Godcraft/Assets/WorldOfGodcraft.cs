using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedSystems;
using UnityEngine;
using Leopotam.EcsLite.Di;
using LeopotamGroup.Globals;


sealed class WorldOfGodcraft : MonoBehaviour
{
    public Material mat;

    public EcsWorld EcsWorld { get; set; }
    EcsSystems systems;

    void Start()
    {
        var meshGenerator = new MeshGenerator();
        meshGenerator.Init();
        Service<MeshGenerator>.Set(meshGenerator);

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

            // .Add (new TestSystem2 ())

            // register additional worlds here, for example:
            // .AddWorld (new EcsWorld (), "events")
#if UNITY_EDITOR
            // add debug systems for custom worlds here, for example:
            // .Add (new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem ("events"))
            .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .DelHere<ChunckHitEvent>()


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

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Client
{
    sealed class NetworkChunckChangeSendSystem : IEcsRunSystem
    {
        [EcsWorld]
        readonly EcsWorld world = default;

        [EcsFilter(typeof(ChunckHitEvent), typeof(NetworkChunckChanged))]
        // field will be injected with filter (ChunckHitEvent included)
        // from default world instance.
        readonly EcsFilter hitFilter = default;

        public void Run(EcsSystems systems)
        {
            foreach (var hitEntity in hitFilter)
            {
                ref var hitComponent = ref world.GetPool<ChunckHitEvent>().Get(hitEntity);
                ref var networkComponent = ref world.GetPool<NetworkChunckChanged>().Get(hitEntity);

                networkComponent.photonView.RaiseEvent(EventCode.ChunckChange);
            }
        }
    }
}
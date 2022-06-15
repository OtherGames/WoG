using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

sealed class CheckVehicleIsGround : IEcsRunSystem
{
    [EcsFilter(typeof(Character))]
    readonly EcsFilter filterPlayers = default;
    [EcsPool]
    readonly EcsPool<Character> poolPlayers = default;

    private Rigidbody vehicleBody;
    private FirstPersonController firstPersonController;
    private CharacterController characterController;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterPlayers)
        {
            ref var character = ref poolPlayers.Get(entity);
            var player = character.view;

            if(Physics.Raycast(player.transform.position, Vector3.down, out var hit, 1.5f))
            {
                if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Vehicle"))
                {
                    if (!vehicleBody)
                        vehicleBody = hit.transform.GetComponent<Rigidbody>();

                    if (!firstPersonController)
                        firstPersonController = player.GetComponent<FirstPersonController>();

                    if (!characterController)
                        characterController = player.GetComponent<CharacterController>();


                    var move = vehicleBody.velocity;
                    move.y = 0;
                    characterController.Move(move * Time.deltaTime);
                }
            }
            else
            {
                vehicleBody = null;
                firstPersonController = null;
                characterController = null;
            }
        }
    }
}

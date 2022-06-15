using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBehaviour : MonoBehaviour
{

    PlayerCharacter player;

    private void Start()
    {
        player = GetComponent<PlayerCharacter>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (player.FPC.isSteering)
            {
                GlobalEvents.onStopSteeringVehicle?.Invoke();
                player.SetSteeringMode(false);
            }
        }
    }
}

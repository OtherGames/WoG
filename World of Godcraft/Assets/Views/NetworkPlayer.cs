using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class NetworkPlayer : MonoBehaviour
{
    PlayerCharacter player;
    PhotonView photonView;

    private void Start()
    {
        player = GetComponent<PlayerCharacter>();
        photonView = GetComponent<PhotonView>();

        player.onChunkHit += Chunck_Changed;

        photonView.RegisterMethod<int>(EventCode.ChunckChange, ChunckChanged);
    }

    private void ChunckChanged(int sdfsdf)
    {
        print(sdfsdf + " Чанк был измене");
    }

    private void Chunck_Changed(Entity entity, ChunckHitEvent data)
    {
        var world = FindObjectOfType<WorldOfGodcraft>().EcsWorld;

        var pool = world.GetPool<NetworkChunckChanged>();
        pool.Add(entity.id);
        ref var component = ref pool.Get(entity.id);
        component.photonView = photonView;
    }
}

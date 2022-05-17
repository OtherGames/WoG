using LeopotamGroup.Globals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Photon.Pun;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using Leopotam.EcsLite;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField]
    private Transform highlightPrefab;
    [SerializeField]
    private GameObject cam;
    public LayerMask lm;
    public Camera uiCamera;

    Transform highlight;

    WorldOfGodcraft godcraft;

    new PhotonView networkView;
    EcsFilter filter;
    EcsWorld ecsWorld;
    HUD hud;

    internal Action<Entity, ChunckHitEvent> onChunkHit;

    int entityBlockHit;
    bool isHit;

    // Start is called before the first frame update
    void Start()
    {
        networkView = GetComponent<PhotonView>();

        if (!networkView.IsMine)
        {
            Destroy(cam);
            
            Destroy(GetComponent<FirstPersonController>());
            Destroy(GetComponent<CharacterController>());
            Destroy(GetComponent<AudioSource>());
        }

        hud = FindObjectOfType<HUD>();

        highlight = Instantiate(highlightPrefab);
        godcraft = FindObjectOfType<WorldOfGodcraft>();
        ecsWorld = godcraft.EcsWorld;

        filter = ecsWorld.Filter<InventoryItem>().Inc<ItemQuickInventory>().End();

        int entity = ecsWorld.NewEntity();
        ecsWorld.GetPool<Character>().Add(entity);
        ref var satiety = ref ecsWorld.GetPool<SatietyComponent>().Add(entity);
        satiety.MaxValue = 100;
        satiety.Value = satiety.MaxValue;
    }

    // Update is called once per frame
    void Update()
    {
        BlockController();
        SaveController();
    }

    void BlockController()
    {
        if (hud.InventoryShowed)
            return;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 7f, lm))
        {
            if (hit.collider.gameObject.CompareTag("PICKABLE"))
            {
                HitOnPickable(hit.collider.gameObject);
                return;
            }
           
            Vector3 normalPos = hit.point - (hit.normal / 2);

            int x = Mathf.FloorToInt(normalPos.x);
            int y = Mathf.FloorToInt(normalPos.y);
            int z = Mathf.FloorToInt(normalPos.z);

            Vector3 blockPosition = new(x, y, z);

            highlight.position = blockPosition;
            
            if (Input.GetMouseButtonDown(0))
            {
                entityBlockHit = godcraft.EcsWorld.NewEntity();

                var pool = godcraft.EcsWorld.GetPool<ChunckHitEvent>();
                pool.Add(entityBlockHit);
                ref var component = ref pool.Get(entityBlockHit);
                component.collider = hit.collider;
                component.position = blockPosition;
                component.blockId = 0;

                onChunkHit?.Invoke(new Entity { id = entityBlockHit }, component);

                isHit = true;
            }

            if (Input.GetMouseButtonUp(0) && isHit)
            {
                isHit = false;

                var pool = godcraft.EcsWorld.GetPool<ChunckHitEvent>();
                var filter = godcraft.EcsWorld.Filter<ChunckHitEvent>().End();
                foreach (var entity in filter)
                {
                    if(entity == entityBlockHit)
                    {
                        pool.Del(entityBlockHit);
                    }
                }

            }

            if (Input.GetMouseButtonDown(1))
            {
                // �����-�� ����� ���������� 1 �� ��� X, �� ������ ���, �� ������ ��� ��������
                ref var chunck = ref Service<World>.Get().GetChunk(blockPosition + Vector3.right);
                var pos = chunck.renderer.transform.position;

                // �����-�� ����� ���������� 1 �� ��� X, �� ������ ���, �� ������ ��� ��������
                int xBlock = x - Mathf.FloorToInt(pos.x) + 1;
                int yBlock = y - Mathf.FloorToInt(pos.y);
                int zBlock = z - Mathf.FloorToInt(pos.z);
                byte hitBlockID = chunck.blocks[xBlock, yBlock, zBlock];

                if (hitBlockID == 100 || hitBlockID == 101 || hitBlockID == 102)
                {
                    GlobalEvents.interactBlockHited.Invoke(hitBlockID, new(x + 1, y, z));
                }
                else
                {
                    int idx = 0;
                    foreach (var entity in filter)
                    {
                        if (idx == InputHandler.Instance.quickSlotID - 1)
                        {
                            var poolItems = ecsWorld.GetPool<InventoryItem>();
                            ref var item = ref poolItems.Get(entity);

                            if (item.itemType == ItemType.Block)
                            {
                                var e = godcraft.EcsWorld.NewEntity();

                                var pool = godcraft.EcsWorld.GetPool<ChunckHitEvent>();
                                pool.Add(e);
                                ref var component = ref pool.Get(e);
                                component.collider = hit.collider;
                                component.position = blockPosition + hit.normal;
                                component.blockId = item.blockID;

                                onChunkHit?.Invoke(new Entity { id = e }, component);

                                // HOT FIX ������� � ��������� �������
                                item.count--;
                                if (item.count == 0)
                                {
                                    Destroy(item.view);
                                    ecsWorld.DelEntity(entity);
                                }

                                StartCoroutine(Delay());

                                //-----------------------------------
                            }
                            else
                            {
                                ref var used = ref ecsWorld.GetPool<ItemUsed>().Add(entity);
                                used.entity = entity;
                                used.id = item.blockID;

                                StartCoroutine(Delay());
                            }

                            IEnumerator Delay()
                            {
                                yield return null;

                                GlobalEvents.itemUsing?.Invoke(entity);
                            }

                            break;
                        }
                        idx++;
                    }


                }
            }



        }
        else
        {
            highlight.position = default;
        }
    }

    void HitOnPickable(GameObject view)
    {
        GlobalEvents.onHitPickable?.Invoke(view);

        if (Input.GetMouseButtonDown(0))
        {
            ref var item = ref ecsWorld.GetPool<ItemTaked>().Add(ecsWorld.NewEntity());
            item.view = view;
        }
    }

    void SaveController()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            var w = Service<World>.Get();

            ChuncksData chuncksData;
            chuncksData.chuncks = new List<byte[,,]>();

            StartCoroutine(Requests());

            IEnumerator Requests()
            {
                int idx = 0;
                foreach (var item in w.chuncks)
                {
                    var json = Json.Serialize(item.blocks);

                    var request = new UpdateUserDataRequest
                    {
                        //KeysToRemove = new List<string> { $"chunks-{idx}" }
                        Data = new Dictionary<string, string>
                        {
                            {$"chunks-{idx}", json }
                        }
                    };

                    PlayFabClientAPI.UpdateUserData(request, OnSuccess, OnFailure);

                    idx++;

                    yield return new WaitForSeconds(0.7f);
                }
            }

            
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            var r = new UpdateUserDataRequest { KeysToRemove = new List<string> { "chunks" } };
            PlayFabClientAPI.UpdateUserData(r, OnSuccess, OnFailure);
        }
    }

    private void OnSuccess(UpdateUserDataResult obj)
    {
        Debug.Log($"{obj.ToJson()}");
    }

    private void OnFailure(PlayFabError obj)
    {
        Debug.Log($"�����, ����� {obj.GenerateErrorReport()}");
    }

    void GetData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnFailure);
    }

    private void OnDataRecieved(GetUserDataResult obj)
    {
        //obj.Data.ContainsKey("")
        //obj.Data[""].Value
    }

    public struct ChuncksData
    {
        public List<byte[,,]> chuncks;
    }
}

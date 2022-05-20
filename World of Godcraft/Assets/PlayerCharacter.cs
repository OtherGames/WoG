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

    [Space]

    [SerializeField] Transform pistolParent;
    [SerializeField] Transform rootHolder;

    public GunView GunView { get; set; }

    Transform highlight;
    WorldOfGodcraft godcraft;
    EcsPool<GunComponent> poolGuns;
    EcsPool<GunFired> poolFired;
    new PhotonView networkView;
    EcsFilter filter;
    EcsWorld ecsWorld;
    HUD hud;

    internal Action<Entity, ChunckHitEvent> onChunkHit;
    internal Action onSlotUpdate;

    int idxQuickSlot;
    int entityBlockHit;
    int entitySelectedItem;
    bool isHit;
    byte? itemID;

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
        poolGuns = ecsWorld.GetPool<GunComponent>();
        poolFired = ecsWorld.GetPool<GunFired>();

        int entity = ecsWorld.NewEntity();
        ref var character = ref ecsWorld.GetPool<Character>().Add(entity);
        character.view = this;
        ref var satiety = ref ecsWorld.GetPool<SatietyComponent>().Add(entity);
        satiety.MaxValue = 100;
        satiety.Value = satiety.MaxValue;

        onSlotUpdate += Slot_Updated;
        itemID = null;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHolderRotation();
        CheckQuickSlots();
        BlockController();
        SaveController();
    }

    void CheckQuickSlots()
    {
        if (idxQuickSlot != InputHandler.Instance.quickSlotID - 1)
        {
            idxQuickSlot = InputHandler.Instance.quickSlotID - 1;
            onSlotUpdate?.Invoke();
        }

        int idx = 0;
        bool foundItem = false;
        byte? selectedItem = null;
        foreach (var entity in filter)
        {
            if(idx == idxQuickSlot)
            {
                selectedItem = ecsWorld.GetPool<InventoryItem>().Get(entity).blockID;
                if (itemID == null || itemID != selectedItem)
                {
                    itemID = selectedItem;
                    entitySelectedItem = entity;
                    foundItem = true;
                    onSlotUpdate?.Invoke();
                }
            }

            idx++;
        }

        if(!foundItem && itemID != selectedItem)
        {
            itemID = null;
            onSlotUpdate?.Invoke();
        }
    }

    void Slot_Updated()
    {
        print("слот обновлен " + itemID);

        ClearView();

        if(itemID != null)
        {
            ref var item = ref ecsWorld.GetPool<InventoryItem>().Get(entitySelectedItem);
            var view = Instantiate(item.view);
            view.SetActive(true);
            view.layer = 0;

            foreach (var t in view.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = 0;
            }
            view.transform.parent = pistolParent;
            view.transform.localPosition = Vector3.zero;
            view.transform.localScale = Vector3.one * 0.5f;
            view.transform.localRotation = Quaternion.identity;
            if(item.itemType == ItemType.Block)
            {
                view.transform.localScale = Vector3.one * 0.3f;
            }

            GunView = view.GetComponent<GunView>();
        }

        ref var updated = ref ecsWorld.GetPool<UsedItemUpdated>().Add(ecsWorld.NewEntity());
        updated.id = itemID;
        updated.entity = entitySelectedItem;
    }

    void ClearView()
    {
        foreach (Transform item in pistolParent)
        {
            Destroy(item.gameObject);
        }
    }

    void BlockController()
    {
        if (hud.InventoryShowed)
            return;

        if (poolGuns.Has(entitySelectedItem))
        {
            if (poolGuns.Get(entitySelectedItem).shotAvailable && Input.GetMouseButton(0))
            {
                poolFired.Add(entitySelectedItem);
            }
            return;
        }

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
                // зачем-то нужно прибавлять 1 по оси X, хз почему так, но именно так работает
                ref var chunck = ref Service<World>.Get().GetChunk(blockPosition + Vector3.right);
                var pos = chunck.renderer.transform.position;

                // зачем-то нужно прибавлять 1 по оси X, хз почему так, но именно так работает
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

                                // HOT FIX вынести в отдельную систему
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

    void UpdateHolderRotation()
    {
        rootHolder.localRotation = cam.transform.localRotation;
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
        Debug.Log($"ммммм, хуэта {obj.GenerateErrorReport()}");
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

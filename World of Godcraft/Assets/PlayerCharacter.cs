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
using System.Linq;

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

    [SerializeField]
    private Transform spherePrefab;
    [SerializeField]
    private Transform cubePrefab;
    Transform highlightSphere;
    Transform highlightCube;

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
    byte? selectedItemID;

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
        highlightSphere = Instantiate(cubePrefab);
        highlightCube = Instantiate(spherePrefab);
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
        selectedItemID = null;
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
                if (selectedItemID == null || selectedItemID != selectedItem)
                {
                    selectedItemID = selectedItem;
                    entitySelectedItem = entity;
                    foundItem = true;
                    onSlotUpdate?.Invoke();
                }
            }

            idx++;
        }

        if(!foundItem && selectedItemID != selectedItem)
        {
            selectedItemID = null;
            onSlotUpdate?.Invoke();
        }
    }

    void Slot_Updated()
    {
        print("Быстрой слот обновлен: " + selectedItemID);

        ClearView();

        if(selectedItemID != null)
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
        updated.id = selectedItemID;
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

        if (selectedItemID != null && poolGuns.Has(entitySelectedItem))
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

            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Vehicle"))
            {
                HitOnVehicle(hit);
                return;
            }

            highlightSphere.position = Vector3.zero;
            highlightCube.position = Vector3.zero;

            Vector3 normalPos = hit.point - (hit.normal / 2);

            int x = Mathf.FloorToInt(normalPos.x);
            int y = Mathf.FloorToInt(normalPos.y);
            int z = Mathf.FloorToInt(normalPos.z);

            Vector3 blockPosition = new(x, y, z);

            highlight.position = blockPosition;
            highlight.forward = Vector3.forward;
            
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

    void HitOnVehicle(RaycastHit hit)
    {
        highlight.position = Vector3.zero;

        if (highlightSphere.parent != hit.transform)
            highlightSphere.parent = hit.transform;
        if (highlightCube.parent != hit.transform)
            highlightCube.parent = hit.transform;

        highlightSphere.position = hit.point;
        highlightSphere.forward = hit.normal;
        
        highlightCube.localPosition = Vector3.zero;

        var localX = highlightSphere.localPosition.x - 0.0001f;
        var localY = highlightSphere.localPosition.y - 0.0001f;
        var localZ = highlightSphere.localPosition.z - 0.0001f;
        var localCubePos = highlightSphere.localPosition;
        var optaX = localX - Mathf.RoundToInt(localX);
        var optaY = localY - Mathf.RoundToInt(localY);
        var optaZ = localZ - Mathf.RoundToInt(localZ);
        
        var rotX = Mathf.RoundToInt(highlightSphere.localRotation.eulerAngles.x);
        var rotY = Mathf.RoundToInt(highlightSphere.localRotation.eulerAngles.y);
        var rotZ = Mathf.RoundToInt(highlightSphere.localRotation.eulerAngles.z);
        
        if (optaX > 0)
        {
            localCubePos.x = Mathf.RoundToInt(localX) + 0.5f;
        }
        else
        {
            localCubePos.x = Mathf.RoundToInt(localX) - 0.5f;
        }
        if(rotY == 270 && rotX != 270)
        {
            localCubePos.x = Mathf.RoundToInt(localX) + 0.5f;
        }

        if(optaY > 0)
        {
            localCubePos.y = Mathf.RoundToInt(localY) + 0.5f;
        }
        else
        {
            localCubePos.y = Mathf.RoundToInt(localY) - 0.5f;
        }
        if(rotX == 90)
        {
            localCubePos.y = Mathf.RoundToInt(localY) + 0.5f;
        }
        

        if (optaZ > 0)
        {
            localCubePos.z = Mathf.RoundToInt(localZ) + 0.5f;
        }
        else
        {
            localCubePos.z = Mathf.RoundToInt(localZ) - 0.5f;
        }
        if (rotY == 180 && (rotX == 0 || rotX == 360))
        {
            localCubePos.z = Mathf.RoundToInt(localZ) + 0.5f;
        }

        //var localX = highlightSphere.localPosition.x - 0.0001f;
        //var localY = highlightSphere.localPosition.y - 0.0001f;
        //var localZ = highlightSphere.localPosition.z - 0.0001f;
        //var localCubePos = highlightSphere.localPosition;
        //var optaX = localX - MathF.Truncate(localX);
        //var optaY = localY - MathF.Truncate(localY);
        //var optaZ = localZ - MathF.Truncate(localZ);
        //print(Mathf.RoundToInt(localX).ToString("F7"));
        //print((localX).ToString("F7") + " pure");
        ////print(highlightSphere.localRotation.eulerAngles);
        //localCubePos.x = Mathf.RoundToInt(localX) + 0.5f;
        //localCubePos.y = Mathf.RoundToInt(localY) + 0.5f;
        //localCubePos.z = Mathf.RoundToInt(localZ) + 0.5f;

        //if (Mathf.Approximately(highlightSphere.localRotation.eulerAngles.x, -90))
        //{
        //    localCubePos.y = Mathf.FloorToInt(localY) - 0.5f;
        //    print("dvosndvin");
        //}
        //else
        //{
        //    localCubePos.y = Mathf.FloorToInt(localY) + 0.5f;
        //}



        //if (Mathf.Abs(localX) > 0.01f)
        //{
        //    localCubePos.x = Mathf.FloorToInt(localX) + 0.5f;
        //}
        //if (Mathf.Abs(localY) > 0.01f)
        //{
        //    localCubePos.y = Mathf.FloorToInt(localY) + 0.5f;
        //}
        //if (Mathf.Abs(localZ) > 0.01f)
        //{
        //    localCubePos.z = Mathf.FloorToInt(localZ) + 0.5f;
        //}

        //if (Mathf.Abs(optaZ) > 0.01f)
        //{
        //    localCubePos.z = Mathf.FloorToInt((hit.point - pos).z) + 0.5f;
        //}

        highlightCube.localPosition = localCubePos;

        var locRotZ = Mathf.RoundToInt((float)rotZ / 90f) * 90;
        var locRot = new Vector3(rotX, rotY, locRotZ);
        highlightCube.localRotation = Quaternion.Euler(locRot);


        if (Input.GetMouseButtonDown(1))
        {
            var blockID = GetBlockIDQuickSlot();

            if (blockID == 0)
                return;

            var e = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<VehicleHitEvent>();
            ref var hitEvent = ref pool.Add(e);
            var blockPos = localCubePos - new Vector3(-0.5f, 0.5f, 0.5f);
            var connectedPos = blockPos;
            rotX = Mathf.RoundToInt((float)rotX / 90f) * 90;
            rotY = Mathf.RoundToInt((float)rotY / 90f) * 90;
            rotZ = Mathf.RoundToInt((float)rotZ / 90f) * 90;
            print($"Rotation {rotX} : {rotY} : {rotZ}");

            if (rotX == 0 && rotY == 180)
            {
                blockPos += Vector3.back;
            }
            else if(rotX == 0 && rotY == 0)
            {
                blockPos += Vector3.forward;
            }
            else if(rotX == 0 && rotY == 90)
            {
                blockPos += Vector3.right;
            }
            else if(rotX == 0 && rotY == 270)
            {
                blockPos += Vector3.left;
            }
            else if(rotX == 270 )
            {
                blockPos += Vector3.up;
            }
            else if(rotX == 90)
            {
                blockPos += Vector3.down;
            }

            hitEvent.connectedPos = new Vector3Int(Mathf.RoundToInt(connectedPos.x), Mathf.RoundToInt(connectedPos.y), Mathf.RoundToInt(connectedPos.z)); ;
            hitEvent.blockPos = new Vector3Int(Mathf.RoundToInt(blockPos.x), Mathf.RoundToInt(blockPos.y), Mathf.RoundToInt(blockPos.z));
            hitEvent.blockID = blockID;
        }
        if (Input.GetMouseButtonDown(0))
        {
            var e = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<VehicleHitEvent>();
            ref var hitEvent = ref pool.Add(e);
            var blockPos = localCubePos - new Vector3(-0.5f, 0.5f, 0.5f);
            hitEvent.blockPos = new Vector3Int(Mathf.RoundToInt(blockPos.x), Mathf.RoundToInt(blockPos.y), Mathf.RoundToInt(blockPos.z));
            hitEvent.blockID = 0;
        }

    }

    byte GetBlockIDQuickSlot()
    {
        byte result = 0;

        int idx = 0;
        foreach (var entity in filter)
        {
            if (idx == InputHandler.Instance.quickSlotID - 1)
            {
                var poolItems = ecsWorld.GetPool<InventoryItem>();
                ref var item = ref poolItems.Get(entity);

                if (item.itemType == ItemType.Block)
                {
                    // HOT FIX вынести в отдельную систему
                    result = item.blockID;

                    item.count--;
                    if (item.count == 0)
                    {
                        Destroy(item.view);
                        ecsWorld.DelEntity(entity);
                    }

                    StartCoroutine(Delay());

                    return result;
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

        return result;
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

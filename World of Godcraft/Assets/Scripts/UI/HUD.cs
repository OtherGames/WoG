using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using Leopotam.EcsLite;
using TMPro;
using LeopotamGroup.Globals;

public class HUD : MonoBehaviour
{
    [SerializeField]
    private GameObject group;
    [SerializeField] Canvas canvasInventory;
    [SerializeField]
    Inventory inventory;
    [SerializeField] Inventory workbenchSimle;
    [SerializeField] Inventory workbench;
    [SerializeField] Inventory furnace;
    [SerializeField]
    QuickInventory quickInventory;

    [SerializeField]
    private Image blockWriteMode;
    [SerializeField]
    private Color redColorMode;

    [Space]

    [SerializeField] Image indicatorEat;
    [SerializeField] TMP_Text pickableLabel;

    public static bool WriteMode { get; set; }
    public bool InventoryShowed => inventory.IsShowed || workbenchSimle.IsShowed || workbench.IsShowed || furnace.IsShowed;

    PlayerCharacter player;
    GameObject pickable;
    EcsWorld ecsWorld;
    EcsFilter players;
    EcsFilter filterPickable;
    EcsPool<PickableComponent> poolPickable;

    private IEnumerator Start()
    {
        inventory.Init();
        inventory.Hide();

        workbenchSimle.Init();
        workbenchSimle.Hide();

        workbench.Init();
        workbench.Hide();

        furnace.Init();
        furnace.Hide();

        quickInventory.Init();

        GlobalEvents.interactBlockHited.AddListener(InteractableBlock_Hit);
        GlobalEvents.onHitPickable.AddListener(Pickable_Hited);

        group.SetActive(false);

        ecsWorld = FindObjectOfType<WorldOfGodcraft>().EcsWorld;
        players = ecsWorld.Filter<Character>().Inc<SatietyComponent>().End();
        filterPickable = ecsWorld.Filter<PickableComponent>().End();
        poolPickable = ecsWorld.GetPool<PickableComponent>();

        yield return null;

        group.SetActive(true);

        yield return null;

        group.SetActive(false);

        yield return null;

        group.SetActive(true);

        // HOT FIX
        //canvasInventory.worldCamera = FindObjectOfType<PlayerCharacter>().uiCamera;
    }

    private void InteractableBlock_Hit(byte blockID, Vector3Int blockPos)
    {
        if (!player)
            player = FindObjectOfType<PlayerCharacter>();

        if (blockID == 100)
        {
            workbenchSimle.Show(blockPos);
            player.GetComponent<FirstPersonController>().MouseLook.SetCursorLock(false);
        }

        if (blockID == 101)
        {
            workbench.Show(blockPos);
            player.GetComponent<FirstPersonController>().MouseLook.SetCursorLock(false);
        }

        if (blockID == BLOCKS.FURNACE)
        {
            furnace.Show(blockPos);
            player.GetComponent<FirstPersonController>().MouseLook.SetCursorLock(false);
        }
    }


    private void Update()
    {
        TESOEBIR();

        if (Input.GetKeyDown(KeyCode.R))
        {
            WriteMode = !WriteMode;

            blockWriteMode.color = WriteMode ? redColorMode : new Color(0, 0, 0, 0);
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.I))
        {
            if (!player)
                player = FindObjectOfType<PlayerCharacter>();

            if (inventory.IsShowed)
            {
                inventory.Hide();
                player.GetComponent<FirstPersonController>().MouseLook.SetCursorLock(true);
            }
            else if(!workbenchSimle.IsShowed && !workbench.IsShowed && !furnace.IsShowed)
            {
                inventory.Show(default);
                player.GetComponent<FirstPersonController>().MouseLook.SetCursorLock(false);
            }

            if (workbenchSimle.IsShowed)
            {
                workbenchSimle.Hide();
                player.GetComponent<FirstPersonController>().MouseLook.SetCursorLock(true);
            }

            if (workbench.IsShowed)
            {
                workbench.Hide();
                player.GetComponent<FirstPersonController>().MouseLook.SetCursorLock(true);
            }

            if (furnace.IsShowed)
            {
                furnace.Hide();
                player.GetComponent<FirstPersonController>().MouseLook.SetCursorLock(true);
            }
        }

        inventory.ScreenScale = transform.lossyScale.x;
        workbenchSimle.ScreenScale = transform.lossyScale.x;
        workbench.ScreenScale = transform.lossyScale.x;
        furnace.ScreenScale = transform.lossyScale.x;

        UpdateIndicators();
        UpdatePickabaleLabel();
    }

    private void UpdateIndicators()
    {
        foreach (var entity in players)
        {
            ref var satiety = ref ecsWorld.GetPool<SatietyComponent>().Get(entity);
            indicatorEat.fillAmount = (float)satiety.Value / (float)satiety.MaxValue;
        }
    }

    private void UpdatePickabaleLabel()
    {
        if (pickable)
        {
            foreach (var entity in filterPickable)
            {
                ref var component = ref poolPickable.Get(entity);
                if (component.view == pickable)
                {
                    pickableLabel.text = component.name;

                    var pos = pickable.transform.position + (Vector3.up * 0.7f);
                    var screenpos = Camera.main.WorldToScreenPoint(pos);
                    pickableLabel.transform.position = screenpos;
                }
            }
        }
        else
        {
            pickableLabel.text = string.Empty;
        }

        pickable = null;
    }

    private void Pickable_Hited(GameObject view)
    {
        pickable = view;
    }

    private void TESOEBIR()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            byte ID = BLOCKS.FURNACE;
            var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
            var dropedBlock = new GameObject("Droped Block");
            dropedBlock.AddComponent<MeshRenderer>().material = FindObjectOfType<WorldOfGodcraft>().mat;
            dropedBlock.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMesh(ID);
            dropedBlock.AddComponent<DropedBlock>();
            dropedBlock.transform.localScale /= 3f;
            dropedBlock.layer = 5;

            var entity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<InventoryItem>();
            pool.Add(entity);
            ref var component = ref pool.Get(entity);
            component.blockID = ID;
            component.view = dropedBlock;
            component.count = 1;
            component.itemType = ItemType.Block;

            ecsWorld.GetPool<ItemQuickInventory>().Add(entity);

            var quick = FindObjectOfType<QuickInventory>();
            var cell = quick.Cells.Find(c => c.EntityItem == null);
            cell.Init(entity, ref component);
            quick.UpdateItems();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            byte ID = 101;
            var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
            var dropedBlock = new GameObject("Droped Block");
            dropedBlock.AddComponent<MeshRenderer>().material = FindObjectOfType<WorldOfGodcraft>().mat;
            dropedBlock.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMesh(ID);
            dropedBlock.AddComponent<DropedBlock>();
            dropedBlock.transform.localScale /= 3f;
            dropedBlock.layer = 5;

            var entity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<InventoryItem>();
            pool.Add(entity);
            ref var component = ref pool.Get(entity);
            component.blockID = ID;
            component.view = dropedBlock;
            component.count = 1;
            component.itemType = ItemType.Block;

            ecsWorld.GetPool<ItemQuickInventory>().Add(entity);

            var quick = FindObjectOfType<QuickInventory>();
            var cell = quick.Cells.Find(c => c.EntityItem == null);
            cell.Init(entity, ref component);
            quick.UpdateItems();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            byte ID = ITEMS.SIMPLE_PISTOL;
            var prefab = Service<PrefabsHolder>.Get().Get(ID);
            var item = Instantiate(prefab);
            item.transform.localScale /= 1.9f;
            item.layer = 5;

            var entity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<InventoryItem>();
            pool.Add(entity);
            ref var component = ref pool.Get(entity);
            component.blockID = ID;
            component.view = item;
            component.count = 1;
            component.itemType = ItemType.Item;

            ecsWorld.GetPool<ItemQuickInventory>().Add(entity);

            var quick = FindObjectOfType<QuickInventory>();
            var cell = quick.Cells.Find(c => c.EntityItem == null);
            cell.Init(entity, ref component);
            quick.UpdateItems();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            byte ID = ITEMS.BULLET;
            var prefab = Service<PrefabsHolder>.Get().Get(ID);
            var item = Instantiate(prefab);
            item.layer = 5;

            var entity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<InventoryItem>();
            pool.Add(entity);
            ref var component = ref pool.Get(entity);
            component.blockID = ID;
            component.view = item;
            component.count = 1;
            component.itemType = ItemType.Item;

            ecsWorld.GetPool<ItemQuickInventory>().Add(entity);

            var quick = FindObjectOfType<QuickInventory>();
            var cell = quick.Cells.Find(c => c.EntityItem == null);
            cell.Init(entity, ref component);
            quick.UpdateItems();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            byte ID = BLOCKS.ENGINE;
            var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
            var dropedBlock = new GameObject("Engine opta");
            dropedBlock.AddComponent<MeshRenderer>().material = FindObjectOfType<WorldOfGodcraft>().mat;
            dropedBlock.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMesh(ID);
            dropedBlock.AddComponent<DropedBlock>();
            dropedBlock.transform.localScale /= 3f;
            dropedBlock.layer = 5;

            var entity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<InventoryItem>();
            pool.Add(entity);
            ref var component = ref pool.Get(entity);
            component.blockID = ID;
            component.view = dropedBlock;
            component.count = 1;
            component.itemType = ItemType.Block;

            ecsWorld.GetPool<ItemQuickInventory>().Add(entity);

            var quick = FindObjectOfType<QuickInventory>();
            var cell = quick.Cells.Find(c => c.EntityItem == null);
            cell.Init(entity, ref component);
            quick.UpdateItems();

        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            byte ID = BLOCKS.ACTUATOR;
            var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
            var dropedBlock = new GameObject("Engine opta");
            dropedBlock.AddComponent<MeshRenderer>().material = FindObjectOfType<WorldOfGodcraft>().mat;
            dropedBlock.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMesh(ID);
            dropedBlock.AddComponent<DropedBlock>();
            dropedBlock.transform.localScale /= 3f;
            dropedBlock.layer = 5;

            var entity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<InventoryItem>();
            pool.Add(entity);
            ref var component = ref pool.Get(entity);
            component.blockID = ID;
            component.view = dropedBlock;
            component.count = 1;
            component.itemType = ItemType.Block;

            ecsWorld.GetPool<ItemQuickInventory>().Add(entity);

            var quick = FindObjectOfType<QuickInventory>();
            var cell = quick.Cells.Find(c => c.EntityItem == null);
            cell.Init(entity, ref component);
            quick.UpdateItems();

        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            foreach (var joint in FindObjectsOfType<HingeJoint>())
            {
                joint.useMotor = true;
                var motor = joint.motor;
                if (!Mathf.Approximately(joint.axis.x, 0))
                {
                    motor.targetVelocity += 100 * joint.axis.x;
                }
                if (!Mathf.Approximately(joint.axis.y, 0))
                {
                    motor.targetVelocity += 100 * joint.axis.y;
                }
                if (!Mathf.Approximately(joint.axis.z, 0))
                {
                    motor.targetVelocity += 100 * joint.axis.z;
                }
                motor.force += 100;
                joint.motor = motor;
            }
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            byte ID = BLOCKS.ACTUATOR_ROTARY;
            var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
            var dropedBlock = new GameObject("Engine opta");
            dropedBlock.AddComponent<MeshRenderer>().material = FindObjectOfType<WorldOfGodcraft>().mat;
            dropedBlock.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMesh(ID);
            dropedBlock.AddComponent<DropedBlock>();
            dropedBlock.transform.localScale /= 3f;
            dropedBlock.layer = 5;

            var entity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<InventoryItem>();
            pool.Add(entity);
            ref var component = ref pool.Get(entity);
            component.blockID = ID;
            component.view = dropedBlock;
            component.count = 1;
            component.itemType = ItemType.Block;

            ecsWorld.GetPool<ItemQuickInventory>().Add(entity);

            var quick = FindObjectOfType<QuickInventory>();
            var cell = quick.Cells.Find(c => c.EntityItem == null);
            cell.Init(entity, ref component);
            quick.UpdateItems();

        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            byte ID = 3;
            var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
            var dropedBlock = new GameObject("Engine opta");
            dropedBlock.AddComponent<MeshRenderer>().material = FindObjectOfType<WorldOfGodcraft>().mat;
            dropedBlock.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMesh(ID);
            dropedBlock.AddComponent<DropedBlock>();
            dropedBlock.transform.localScale /= 3f;
            dropedBlock.layer = 5;

            var entity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<InventoryItem>();
            pool.Add(entity);
            ref var component = ref pool.Get(entity);
            component.blockID = ID;
            component.view = dropedBlock;
            component.count = 100;
            component.itemType = ItemType.Block;

            ecsWorld.GetPool<ItemQuickInventory>().Add(entity);

            var quick = FindObjectOfType<QuickInventory>();
            var cell = quick.Cells.Find(c => c.EntityItem == null);
            cell.Init(entity, ref component);
            quick.UpdateItems();

        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            byte ID = BLOCKS.STEERING;
            var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
            var dropedBlock = new GameObject("Engine opta");
            dropedBlock.AddComponent<MeshRenderer>().material = FindObjectOfType<WorldOfGodcraft>().mat;
            dropedBlock.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMesh(ID);
            dropedBlock.AddComponent<DropedBlock>();
            dropedBlock.transform.localScale /= 3f;
            dropedBlock.layer = 5;

            var entity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<InventoryItem>();
            pool.Add(entity);
            ref var component = ref pool.Get(entity);
            component.blockID = ID;
            component.view = dropedBlock;
            component.count = 1;
            component.itemType = ItemType.Block;

            ecsWorld.GetPool<ItemQuickInventory>().Add(entity);

            var quick = FindObjectOfType<QuickInventory>();
            var cell = quick.Cells.Find(c => c.EntityItem == null);
            cell.Init(entity, ref component);
            quick.UpdateItems();

        }
    }
}

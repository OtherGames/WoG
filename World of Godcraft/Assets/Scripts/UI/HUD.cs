using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using Leopotam.EcsLite;
using TMPro;

public class HUD : MonoBehaviour
{
    [SerializeField]
    private GameObject group;
    [SerializeField] Canvas canvasInventory;
    [SerializeField]
    Inventory inventory;
    [SerializeField] Inventory workbenchSimle;
    [SerializeField] Inventory workbench;
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
    public bool InventoryShowed => inventory.IsShowed || workbenchSimle.IsShowed;

    PlayerCharacter player;
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

        quickInventory.Init();

        quickInventory.onItemClicked += Item_Clicked;
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

    private void InteractableBlock_Hit(byte blockID)
    {
        if (!player)
            player = FindObjectOfType<PlayerCharacter>();

        if (blockID == 100)
        {
            workbenchSimle.Show();
            player.GetComponent<FirstPersonController>().MouseLook.SetCursorLock(false);
        }

        if (blockID == 101)
        {
            workbench.Show();
            player.GetComponent<FirstPersonController>().MouseLook.SetCursorLock(false);
        }
    }

    private void Item_Clicked(int entity)
    {
        //inventory.ItemClicked(entity);
    }

    private void Update()
    {
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
            else if(!workbenchSimle.IsShowed && !workbench.IsShowed)
            {
                inventory.Show();
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
        }

        inventory.ScreenScale = transform.lossyScale.x;
        workbenchSimle.ScreenScale = transform.lossyScale.x;
        workbench.ScreenScale = transform.lossyScale.x;

        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        foreach (var entity in players)
        {
            ref var satiety = ref ecsWorld.GetPool<SatietyComponent>().Get(entity);
            indicatorEat.fillAmount = (float)satiety.Value / (float)satiety.MaxValue;
        }
    }

    private void Pickable_Hited(GameObject view)
    {
        foreach (var entity in filterPickable)
        {
            ref var component = ref poolPickable.Get(entity);
            if(component.view == view)
            {
                print("нашел сраную моркву");
            }
        }
    }
}

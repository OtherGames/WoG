using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using System;

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

    public static bool WriteMode { get; set; }
    public bool InventoryShowed => inventory.IsShowed || workbenchSimle.IsShowed;

    PlayerCharacter player;

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

        group.SetActive(false);

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
    }
}

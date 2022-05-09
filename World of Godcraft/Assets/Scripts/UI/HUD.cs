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
    [SerializeField]
    QuickInventory quickInventory;

    [SerializeField]
    private Image blockWriteMode;
    [SerializeField]
    private Color redColorMode;

    public static bool WriteMode { get; set; }
    public bool InventoryShowed => inventory.IsShowed;

    PlayerCharacter player;

    private IEnumerator Start()
    {
        inventory.Init();
        inventory.Hide();

        quickInventory.Init();

        quickInventory.onItemClicked += Item_Clicked;

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

    private void Item_Clicked(int entity)
    {
        inventory.ItemClicked(entity);
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
            else
            {
                inventory.Show();
                player.GetComponent<FirstPersonController>().MouseLook.SetCursorLock(false);
            }
        }

        inventory.ScreenScale = transform.lossyScale.x;

    }
}

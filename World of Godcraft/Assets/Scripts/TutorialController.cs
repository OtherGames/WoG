using System.Collections;
using System.Collections.Generic;
using LeopotamGroup.Globals;
using Leopotam.EcsLite;
using UnityEngine;
using System;

public class TutorialController : MonoBehaviour
{
    [SerializeField] TutorialPanel uiPanel;
    [SerializeField] QuestPanel uiQuest;
    [SerializeField] Inventory inventory;

    Dictionary<int, Func<bool>> quests; 
    WorldOfGodcraft godcraft;
    EcsWorld ecsWorld;
    EcsFilter filterItems;
    EcsFilter filterQuickSlots;
    EcsFilter filterCraftResult;
    
    EcsPool<InventoryItem> poolItems;

    Func<bool> currentQuest;

    private void Start()
    {
        uiPanel.Init(uiQuest);
        uiPanel.NextStep();

        godcraft = FindObjectOfType<WorldOfGodcraft>();
        ecsWorld = godcraft.EcsWorld;

        filterItems = ecsWorld.Filter<InventoryItem>().Exc<ItemCraftResultInventory>().End();
        filterCraftResult = ecsWorld.Filter<ItemCraftResultInventory>().End();
        filterQuickSlots = ecsWorld.Filter<InventoryItem>().Inc<ItemQuickInventory>().End();

        poolItems = ecsWorld.GetPool<InventoryItem>();

        GlobalEvents.onBlockPlaced.AddListener(Block_Placed);

        InitQuests();
    }

    private void Block_Placed(byte blockID, Vector3 pos)
    {
        if(blockID == BLOCKS.SIMPLE_WORKBENCH && Quest_11 == currentQuest)
        {
            currentQuest = () => true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && uiPanel.gameObject.activeSelf)
        {
            uiPanel.NextStep();

            if (quests.ContainsKey(uiPanel.IdxStep))
            {
                currentQuest = quests[uiPanel.IdxStep];
            }
            
        }

        if (Input.GetKeyDown(KeyCode.G))
        {

        }

        if (currentQuest())
        {
            uiPanel.gameObject.SetActive(true);
            uiPanel.NextStep();
            
            currentQuest = () => false;
        }
    }

    void InitQuests()
    {
        currentQuest = () => false;

        quests = new()
        {
            { 3,  Quest_03 },
            { 5,  Quest_05 },
            { 7,  Quest_07 },
            { 9,  Quest_09 },
            { 11, Quest_11 },
            { 13, Quest_13 },
            { 15, Quest_15 },
        };
    }

    bool Quest_03()
    {
        foreach (var entity in filterItems)
        {
            if(poolItems.Get(entity).blockID == BLOCKS.WOOD)
            {
                return true;
            }
        }

        return false;
    }

    bool Quest_05()
    {
        foreach (var entity in filterItems)
        {
            if (poolItems.Get(entity).blockID == BLOCKS.WOODEN_PLANK)
            {
                return true;
            }
        }

        return false;
    }

    bool Quest_07()
    {
        if (inventory.DragItem != null)
            return false;

        foreach (var entity in filterCraftResult)
        {
            if (poolItems.Get(entity).blockID == BLOCKS.SIMPLE_WORKBENCH)
            {
                return true;
            }
        }

        return false;
    }

    bool Quest_09()
    {
        foreach (var entity in filterQuickSlots)
        {
            if (poolItems.Get(entity).blockID == BLOCKS.SIMPLE_WORKBENCH)
            {
                return true;
            }
        }

        return false;
    }

    Func<bool> Quest_11 = () => false;

    bool Quest_13()
    {
        foreach (var entity in filterItems)
        {
            if (poolItems.Get(entity).blockID == ITEMS.STICK)
            {
                return true;
            }
        }

        return false;
    }

    bool Quest_15()
    {
        foreach (var entity in filterItems)
        {
            if (poolItems.Get(entity).blockID == ITEMS.AXE_WOODEN)
            {
                return true;
            }
        }

        return false;
    }
}

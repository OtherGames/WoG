using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using System;
using TMPro;

public class CellQuickInventory : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] Transform objectHolder;
    [SerializeField] public TMP_Text labelCount;

    public bool IsItem { get; set; }
    public int? EntityItem { get; set; }

    public Action<int> onItemClick;

    internal void Init(int entity, ref InventoryItem item)
    {
        EntityItem = entity;
        IsItem = true;

        item.view.transform.SetParent(objectHolder, false);
        item.view.transform.localPosition = Vector3.zero;
        item.view.transform.localRotation = Quaternion.identity;

        item.view.SetActive(true);
        item.view.layer = 5;

        labelCount.text = item.count > 1 ? $"x{item.count}" : "";
    }

    
    public void Clear()
    {
        labelCount.text = string.Empty;
        EntityItem = null;
        IsItem = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(EntityItem != null)
        {
            onItemClick?.Invoke(EntityItem.Value);
        }
    }
}

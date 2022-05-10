using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using System;
using TMPro;

public class CellCraftInventory : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Transform objectHolder;
    [SerializeField] public TMP_Text labelCount;

    public Action<CellCraftInventory> OnItemSeted;

    public int? EntityItem { get; set; }
    public bool IsPointerEntered { get; set; }

    internal void Init(ref InventoryItem item)
    {
        item.view.transform.SetParent(objectHolder, false);
        item.view.transform.localPosition = Vector3.zero;
        item.view.transform.localRotation = Quaternion.identity;

        item.view.SetActive(true);
        item.view.layer = 5;

        labelCount.text = item.count > 1 ? $"x{item.count}" : "";
    }

    public void Clear()
    {
        objectHolder.localPosition = new(0, 0, objectHolder.localPosition.z);
        labelCount.text = string.Empty;
        EntityItem = null;
    }

    public void SetItem(DragItem dragItem)
    {
        EntityItem = dragItem.entity;

        dragItem.view.transform.SetParent(objectHolder, false);
        dragItem.view.transform.localPosition = Vector3.zero;
        //dragItem.view.transform.localRotation = Quaternion.identity;

        OnItemSeted?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerEntered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerEntered = false;
    }
}

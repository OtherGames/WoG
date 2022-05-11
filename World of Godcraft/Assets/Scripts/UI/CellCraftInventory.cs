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

    public Action OnItemSeted;

    public int? EntityItem { get; set; }
    public bool IsPointerEntered { get; set; }

    internal void Init(int entity, ref InventoryItem item)
    {
        EntityItem = entity;

        item.view.transform.SetParent(objectHolder, false);
        item.view.transform.localPosition = Vector3.zero;
        item.view.transform.localRotation = Quaternion.identity;

        item.view.SetActive(true);
        item.view.layer = 5;

        labelCount.text = item.count > 1 ? $"x{item.count}" : "";
    }

    internal void UpdateItem(ref InventoryItem item)
    {
        labelCount.text = item.count > 1 ? $"x{item.count}" : "";
    }

    public void SetItem(DragItem dragItem)
    {
        EntityItem = dragItem.entity;

        dragItem.view.transform.SetParent(objectHolder, false);
        dragItem.view.transform.localPosition = Vector3.zero;

        labelCount.text = dragItem.count > 1 ? $"x{dragItem.count}" : "";

        OnItemSeted?.Invoke();
    }

    public void Clear()
    {
        objectHolder.localPosition = new(0, 0, objectHolder.localPosition.z);
        labelCount.text = string.Empty;
        EntityItem = null;
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

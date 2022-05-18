using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using System;
using TMPro;

public class CellInventory : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] Transform objectHolder;
    [SerializeField] public TMP_Text labelCount;

    //public bool IsPointerEntered { get; set; }
    public int? EntityItem { get; set; }

    public Action<CellInventory> OnItemSeted;
    public Action<int> onItemClick;

    internal virtual void Init(int entity, ref InventoryItem item)
    {
        EntityItem = entity;

        objectHolder.localPosition = new(0, 0, objectHolder.localPosition.z);

        item.view.transform.SetParent(objectHolder, false);
        item.view.transform.localPosition = Vector3.zero;
        item.view.transform.localRotation = Quaternion.Euler(item.rotation);

        item.view.SetActive(true);
        item.view.layer = 5;

        foreach (var view in item.view.GetComponentsInChildren<Transform>())
        {
            view.gameObject.layer = 5;
        }

        labelCount.text = item.count > 1 ? $"x{item.count}" : "";
    }

    public virtual void SetItem(DragItem dragItem)
    {
        EntityItem = dragItem.entity;

        dragItem.view.transform.SetParent(objectHolder, false);
        dragItem.view.transform.localPosition = Vector3.zero;
        //dragItem.view.transform.localRotation = Quaternion.identity;

        OnItemSeted?.Invoke(this);
    }

    internal void UpdateItem(ref InventoryItem item)
    {
        labelCount.text = item.count > 1 ? $"x{item.count}" : "";
        print("&&&&&&&&&&");
    }

    public void Clear()
    {
        objectHolder.localPosition = new(0, 0, objectHolder.localPosition.z);
        labelCount.text = string.Empty;
        EntityItem = null;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (EntityItem != null)
        {
            onItemClick?.Invoke(EntityItem.Value);
        }
    }

}

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

    public int? EntityItem { get; set; }

    public Action<CellInventory> OnItemSeted;
    public Action<int> onItemClick;

    internal virtual void Init(int entity, ref InventoryItem item)
    {
        EntityItem = entity;

        objectHolder.localPosition = new(0, 0, objectHolder.localPosition.z);

        item.view.transform.SetParent(objectHolder, false);
        item.view.transform.localPosition = Vector3.zero;
        item.view.transform.localRotation = Quaternion.Euler(Rotation(item.blockID));

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

        OnItemSeted?.Invoke(this);
    }

    internal void UpdateItem(ref InventoryItem item)
    {
        labelCount.text = item.count > 1 ? $"x{item.count}" : "";
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

    Vector3 Rotation(byte id)
    {
        switch (id)
        {
            case ITEMS.INGOT_IRON:
                return new(1.327f, 95.58f, -33.715f);
            case ITEMS.STICK:
                return new(-51f, 39f, 3.189f);
            case ITEMS.AXE_WOODEN:
                return new(-43f, 63.709f, 6.843f);
        }

        return Vector3.zero;
    }
}

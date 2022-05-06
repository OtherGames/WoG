using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class CellCraftInventory : MonoBehaviour
{
    [SerializeField] Transform objectHolder;
    [SerializeField] public TMP_Text labelCount;

    internal void Init(ref InventoryItem item)
    {
        item.view.transform.SetParent(objectHolder, false);
        item.view.transform.localPosition = Vector3.zero;
        item.view.transform.localRotation = Quaternion.identity;

        item.view.SetActive(true);
        item.view.layer = 5;

        labelCount.text = item.count > 1 ? $"x{item.count}" : "";
    }
}

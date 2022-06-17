using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestPanel : MonoBehaviour
{
    [SerializeField] TMP_Text labelQuest;
    [SerializeField] TMP_Text labelDetailed;

    [Space]

    [SerializeField] HorizontalLayoutGroup layoutGroup;

    public void Show(string description, string detailed)
    {
        labelQuest.text = description;
        labelDetailed.text = detailed;

        gameObject.SetActive(true);

        FixLayout();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void FixLayout()
    {
        StartCoroutine(Fix());

        IEnumerator Fix()
        {
            layoutGroup.childForceExpandWidth = false;

            yield return null;

            layoutGroup.childForceExpandWidth = true;
        }
    }
}

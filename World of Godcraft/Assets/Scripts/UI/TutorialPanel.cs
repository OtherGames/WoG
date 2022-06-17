using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class TutorialPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_Text description;
    [SerializeField] GameObject pressToContinue;

    [SerializeField] TutorialStep[] steps;

    [Space] 
    
    [SerializeField] VerticalLayoutGroup layoutGroup;

    QuestPanel questPanel;

    public int IdxStep { get; set; }

    public void Init(QuestPanel questPanel)
    {
        this.questPanel = questPanel;

        questPanel.gameObject.SetActive(false);
    }

    public void NextStep()
    {
        if (IdxStep >= steps.Length)
        {
            gameObject.SetActive(false);
            return;
        }

        var step = steps[IdxStep];

        if (step.isQuest)
        {
            questPanel.Show(step.description, step.detailed);

            gameObject.SetActive(false);
        }
        else
        {
            questPanel.Hide();

            description.text = step.description;

            FixLayout();
        }

        IdxStep++;
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

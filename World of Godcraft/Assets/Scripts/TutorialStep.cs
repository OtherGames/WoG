using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "Tutor Step", menuName = "Mine/Tutor Step")]
public class TutorialStep : ScriptableObject
{
    [TextArea] public string description;

    [TextArea] public string detailed;

    [SerializeField] public bool isQuest;
}

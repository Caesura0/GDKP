using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "dialogue", menuName = "Dialogues")]
public class Dialogue : ScriptableObject
{
    public string conversantName;
    public List<string> dialogueLines;
    //bool hasGivenItems;
    public bool hasSaidDialogue;
    public bool isRepeatableDialogue;


    private void Awake()
    {


        hasSaidDialogue = false;
    }

    public void OnDialogueEnd()
    {


    }
}

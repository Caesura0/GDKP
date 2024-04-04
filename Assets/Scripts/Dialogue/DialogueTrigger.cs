using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{

    [SerializeField] List<Dialogue> dialogueList;
    [SerializeField] string conversantName;

    //[SerializeField] bool shouldRandomize;



    List<Dialogue> validDialogueList;


    private void Start()
    {

        validDialogueList = new List<Dialogue>();
    }

    public void Interact(GameObject interactor)
    {

        foreach (Dialogue dialogue in dialogueList)
        {
            if (!dialogue.hasSaidDialogue)
            {
                //SimpleDialogue.instance.StartDialogue( conversantName,dialogue);
                return;
            }
            else if (dialogue.isRepeatableDialogue)
            {
                validDialogueList.Add(dialogue);
            }
        }


        if (validDialogueList.Count > 0)
        {
            int choices;
            choices = validDialogueList.Count - 1;
            int i = Random.Range(0, choices);
            //SimpleDialogue.instance.StartDialogue( conversantName, validDialogueList[i]);
        }
        else
        {
            //SimpleDialogue.instance.StartDialogue(conversantName);
        }


    }


}

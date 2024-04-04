using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    [SerializeField] List<Dialogue> firstDiagueList;
    //bool isFirstDialogueDone = false;
    bool isAtCharacterSelection;
    

    [SerializeField] IntroCameraController introCameraController;
    [SerializeField] float introDelayTimerAmount = 5;

    public float delayTimer;

    List<Dialogue> currentDialogueList;


    int currentDialogueIndex = 0;

    bool hasStartedDialogue = false;

    void Start()
    {
        currentDialogueList = firstDiagueList;
        delayTimer = introDelayTimerAmount;


    }

    void Update()
    {

        if(delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
        }

        else if( delayTimer <= 0f && !hasStartedDialogue)
        {
            hasStartedDialogue = true;
            StartFirstDialogue();
        }

        if (InputManager.Instance.IsMouseButtonDownThisFrame() && !SimpleDialogue.instance.InDialogue && !isAtCharacterSelection && 1 == 2)
        {

                //pass through dialogue and a complete action, so we know when this section is done
                // then we can start the next one in the index
                SimpleDialogue.instance.StartDialogue(currentDialogueList[currentDialogueIndex], StartNextDialogue);
       
        }


    }


    void StartFirstDialogue() 
    {
        SimpleDialogue.instance.StartDialogue(currentDialogueList[currentDialogueIndex], StartNextDialogue);
    }

    void StartNextDialogue()
    {

        if(currentDialogueIndex < currentDialogueList.Count-1) 
        {
            currentDialogueIndex++;
            SimpleDialogue.instance.StartDialogue(currentDialogueList[currentDialogueIndex], StartNextDialogue);
        }


        else
        { 
            isAtCharacterSelection= true;
            introCameraController.SwitchToCharacterSelect();
        }
    }


}

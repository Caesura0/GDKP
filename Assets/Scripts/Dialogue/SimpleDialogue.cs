using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;

public class SimpleDialogue : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] TextMeshProUGUI conversantName;

    [SerializeField] float textSpeed;

    public static event EventHandler onNextLine;
    public static event EventHandler onTypeLetter;

    [SerializeField] Dialogue defaultDialogue;

    Dialogue currentDialogue;
    Action OnComplete;

    int index = 1;

    public bool InDialogue { get; private set; }


    public static SimpleDialogue instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        InDialogue = false;
        gameObject.SetActive(false);
    }


    void Update()
    {


        if (currentDialogue != null && (InputManager.Instance.IsMouseButtonDownThisFrame()))
        {

            if (dialogueText.text == currentDialogue.dialogueLines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                dialogueText.text = currentDialogue.dialogueLines[index];
            }
        }

    }



    public void StartDialogue(Dialogue dialogue , Action onComplete)
    {

        gameObject.SetActive(true);
        this.OnComplete = onComplete;
        InDialogue = true;
        index = 0;
        dialogueText.text = string.Empty;
        if (dialogue != null)
        {
            currentDialogue = dialogue;
            conversantName.text = dialogue.conversantName;
        }
        else
        {
            currentDialogue = defaultDialogue;
        }

        
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        int charCount = 0;
        foreach (char c in currentDialogue.dialogueLines[index].ToCharArray())
        {
            dialogueText.text += c;
            
            charCount++;
            if (charCount > 1)
            {
                Debug.Log(charCount);
                onTypeLetter?.Invoke(this, EventArgs.Empty);
                charCount = 0;
            }
            //onTypeLetter?.Invoke(this, EventArgs.Empty);
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < currentDialogue.dialogueLines.Count - 1)
        {
            index++;
            dialogueText.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            currentDialogue.OnDialogueEnd();
            currentDialogue = null;
            InDialogue = false;
            
            gameObject.SetActive(false);
            OnComplete?.Invoke();

        }
    }

    IEnumerator Pause()
    {
        yield return new WaitForSeconds(1.5f);
    }
}

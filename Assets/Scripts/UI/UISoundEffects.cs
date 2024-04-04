using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISoundEffects : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] AudioClip pageTurn2;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


    public void PlayPageTurn2()
    {
        //audioSource.PlayOneShot(pageTurn2);
    }
}

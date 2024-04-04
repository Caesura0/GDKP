using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScriptingDefendTheTown : MonoBehaviour
{
    [SerializeField] AudioClip backgroundMusic;
    AudioSource audioSource;
    private void Start()
    {
        audioSource = FindObjectOfType<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.Play();
    }

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class AudioClipSO : ScriptableObject
{

    [Header("Sound Effects")]

    [SerializeField] public AudioClip[] BarBump;
    [SerializeField] public AudioClip[] BoxSlideClosed;
    [SerializeField] public AudioClip[] BoxSlideOpen;
    [SerializeField] public AudioClip[] Cloth;
    [SerializeField] public AudioClip[] FurnitureBump;
    [SerializeField] public AudioClip[] GlassDrop;
    [SerializeField] public AudioClip[] HatDrop;
    [SerializeField] public AudioClip[] MetalCupDrop;
    [SerializeField] public AudioClip[] PaperFlap;
    [SerializeField] public AudioClip[] Tumbleweed;
    [SerializeField] public AudioClip[] WoodItemDrop;

    [SerializeField] public AudioClip[] FootstepWood;
    [SerializeField] public AudioClip[] FootstepWoodJump;
    [SerializeField] public AudioClip[] FootstepWoodLand;
    [SerializeField] public AudioClip[] FootstepWoodScuff;

    [SerializeField] public AudioClip[] SandFootstepNew;
    [SerializeField] public AudioClip[] SandNewJump;
    [SerializeField] public AudioClip[] SandNewLand;

    [Space]
    [Space]
    [Space]
    [Space]
    [Space]

    [Header("Music")]

    [SerializeField] public AudioClip indoorMusic;
    [SerializeField] public AudioClip outdoorMusic;


    [Space]
    [Space]
    [Space]
    [Space]
    [Space]

    [Header("Weapon Sounds")]


    [SerializeField] public AudioClip rifleShoot;
    [SerializeField] public AudioClip pistolShoot;
    [SerializeField] public AudioClip shotgunShoot;
    [SerializeField] public AudioClip reload;

    [Space]
    [Space]
    [Space]


    [Header("Weapon Sounds")]
    [SerializeField] public AudioClip barrelExplosion;
    [SerializeField] public AudioClip grenadeExplosion;
}
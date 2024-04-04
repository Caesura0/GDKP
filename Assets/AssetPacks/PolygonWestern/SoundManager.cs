using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{


    private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";


    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClipSO audioClipSO;


    AudioSource audioSource;


    private float volume = 1f;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }


        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);

    }



    private void Start()
    {
        MercUI.onUnitChanged += MercUI_onUnitChanged;
        SimpleDialogue.onTypeLetter += SimpleDialogue_onTypeLetter;
        ShootAction.onAnyShoot += ShootAction_onAnyShoot;
        ShotgunAction.onAnyAttack += ShotgunAction_onAnyAttack;
        UnitActionSystem.Instance.OnReloadActionStarted += Instance_OnReloadActionStarted;
        UnitActionSystem.Instance.OnSelectedActionChange += Instance_OnSelectedActionChange;
        UnitActionSystem.Instance.OnSelectedUnitChange += Instance_OnSelectedUnitChange;
        ExplodableCrate.onAnyBarrelExplosion += ExplodableBarrel_OnAnyBarrelExplosion;
        GrenadeProjectile.OnAnyGrenadeExploded += GrenadeProjectile_OnAnyGrenadeExploded;
        audioSource = GetComponent<AudioSource>();
        
    }

    private void GrenadeProjectile_OnAnyGrenadeExploded(object sender, System.EventArgs e)
    {
        PlaySound(audioClipSO.barrelExplosion, transform.position);
    }

    private void ExplodableBarrel_OnAnyBarrelExplosion(object sender, System.EventArgs e)
    {
        PlaySound(audioClipSO.grenadeExplosion, transform.position);
    }

    private void Instance_OnSelectedUnitChange(object sender, System.EventArgs e)
    {
        PlaySound(audioClipSO.PaperFlap[1], transform.position);
    }

    private void Instance_OnSelectedActionChange(object sender, System.EventArgs e)
    {
        PlaySound(audioClipSO.PaperFlap[1], transform.position);
    }

    private void Instance_OnReloadActionStarted(object sender, System.EventArgs e)
    {
        PlaySound(audioClipSO.reload, transform.position);
    }

    private void ShotgunAction_onAnyAttack(object sender, BaseAttackAction.OnAttackEventArgs e)
    {
        PlaySound(audioClipSO.shotgunShoot, e.shootingUnit.GetWorldPosition());
    }

    private void ShootAction_onAnyShoot(object sender, BaseAttackAction.OnAttackEventArgs e)
    {
        switch (e.weaponType)
        {
            case WeaponType.Rifle:
                PlaySound(audioClipSO.rifleShoot, e.shootingUnit.GetWorldPosition());
                break;
            case WeaponType.Pistol:
                PlaySound(audioClipSO.rifleShoot, e.shootingUnit.GetWorldPosition());
                break;
        }

    }

    private void SimpleDialogue_onTypeLetter(object sender, System.EventArgs e)
    {
        PlaySound(audioClipSO.PaperFlap[0], transform.position, 8f);
    }



    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
    {
        PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }

    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        audioSource.PlayOneShot(audioClip, volumeMultiplier);
    }


    public void ChangeVolume()
    {
        volume += .1f;
        if (volume > 1f)
        {
            volume = 0f;
        }

        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return volume;
    }



    private void MercUI_onUnitChanged(object sender, System.EventArgs e)
    {
        Debug.Log("playsound onunit changed");
        PlaySound(audioClipSO.PaperFlap[1], transform.position);
    }

}
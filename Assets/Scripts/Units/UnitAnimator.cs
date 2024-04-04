using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Transform bulletProjectilePrefab;
    [SerializeField] Transform shootPoint;
    [SerializeField] Transform rifleTransform;
    [SerializeField] Transform swordTransform;

    private void Awake()
    {
        if (TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving;
            moveAction.OnStopMoving += MoveAction_OnStopMoving;
        }
        if (TryGetComponent<ShootAction>(out ShootAction shootAction))
        {
            shootAction.onShoot += ShootAction_OnShoot;
        }
        if (TryGetComponent<ShotgunAction>(out ShotgunAction shotgunAction))
        {
            shotgunAction.onShoot += ShotgunAction_onShoot; 
        }
        if (TryGetComponent<SwordAction>(out SwordAction swordAction))
        {
            swordAction.OnSwordActionStarted += ShootAction_OnSwordActionStarted;
            swordAction.OnSwordActionComplete += ShootAction_OnSwordActionComplete;
        }
        if (TryGetComponent<GrenadeAction>(out GrenadeAction grenadeAction))
        {
            grenadeAction.OnGrenadeActionStarted += GrenadeAction_OnGrenadeActionStarted;
            grenadeAction.OnGrenadeActionComplete += GrenadeAction_OnGrenadeActionComplete;
        }
        if(TryGetComponent<HealthSystem>(out HealthSystem healthSystem))
        {
            healthSystem.onDamaged += HealthSystem_onDamaged;
        }
        if (TryGetComponent<KickAction>(out KickAction KickAction))
        {
            KickAction.OnKickactionStarted += KickAction_OnKickactionStarted;
            KickAction.OnKickActionComplete += KickAction_OnKickActionComplete;
        }
    }

    private void ShotgunAction_onShoot(object sender, BaseAttackAction.OnAttackEventArgs e)
    {
        animator.SetTrigger("Shoot");
        Transform bulletProjectileTransform = Instantiate(bulletProjectilePrefab, shootPoint.position, Quaternion.identity);
        BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();
        Vector3 targetShootAtPosition = Vector3.zero;
        if (e.targetUnit != null)
        {
            targetShootAtPosition = e.targetUnit.GetWorldPosition();
        }
        if (e.destructable != null)
        {
            targetShootAtPosition = e.destructable.GetWorldPosition();
        }
        targetShootAtPosition.y = shootPoint.position.y;

        bulletProjectile.Setup(targetShootAtPosition);
    }

    private void KickAction_OnKickActionComplete(object sender, EventArgs e)
    {
        RemoveEquipment();
    }

    private void KickAction_OnKickactionStarted(object sender, EventArgs e)
    {
        EquipRifle();
        animator.SetTrigger("Kick");
    }

    private void HealthSystem_onDamaged(object sender, EventArgs e)
    {
        animator.SetTrigger("TakeHit");
    }

    private void GrenadeAction_OnGrenadeActionStarted(object sender, EventArgs e)
    {
        
        RemoveEquipment();
        //Debug.Log("GrenadeThrow");
        animator.SetTrigger("GrenadeThrow");
    }

    private void GrenadeAction_OnGrenadeActionComplete(object sender, EventArgs e)
    {
        EquipRifle();
    }

    private void ShootAction_OnSwordActionComplete(object sender, EventArgs e)
    {
        EquipRifle();
    }

    private void ShootAction_OnSwordActionStarted(object sender, EventArgs e)
    {
        EquipSword();
        animator.SetTrigger("SwordSlash");
    }

    void MoveAction_OnStartMoving(object sender, EventArgs e)
    {
        animator.SetBool("IsWalking", true);
    }

    void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        animator.SetBool("IsWalking", false);
    }

    void ShootAction_OnShoot(object sender, ShootAction.OnAttackEventArgs e)
    {
        animator.SetTrigger("Shoot");
        Transform bulletProjectileTransform = Instantiate(bulletProjectilePrefab, shootPoint.position, Quaternion.identity);
        BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();

        Vector3 targetUnitShootAtPosition = e.targetUnit.GetWorldPosition();
        targetUnitShootAtPosition.y = shootPoint.position.y;

        bulletProjectile.Setup(targetUnitShootAtPosition);
    }


    void EquipSword()
    {
        swordTransform.gameObject.SetActive(true);
        rifleTransform.gameObject.SetActive(false);

    }

    void EquipRifle()
    {
        //Debug.Log("equip rifle");
        swordTransform.gameObject.SetActive(false);
        rifleTransform.gameObject.SetActive(true);
    }

    void RemoveEquipment()
    {
        //Debug.Log("equip grenade");
        rifleTransform.gameObject.SetActive(false);
        swordTransform.gameObject.SetActive(false);
    }
}

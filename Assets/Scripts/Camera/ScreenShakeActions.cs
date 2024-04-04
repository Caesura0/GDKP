using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeActions : MonoBehaviour
{
    private void Start()
    {
        ShootAction.onAnyShoot += ShootAction_OnAnyShoot;
        GrenadeProjectile.OnAnyGrenadeExploded += GrenadeProjectile_OnAnyGrenadeExploded;
        SwordAction.OnAnySwordHit += SwordAction_OnAnySwordHit;
        KickAction.OnAnyKickHit += KickAction_OnAnyKickHit;
        ShootAction.onAnyShoot += ShootAction_onAnyShoot;
    }

    private void ShootAction_onAnyShoot(object sender, ShootAction.OnAttackEventArgs e)
    {
        ScreenShake.Instance.Shake(3f);
    }

    private void KickAction_OnAnyKickHit(object sender, EventArgs e)
    {
        ScreenShake.Instance.Shake(4f);
    }

    private void SwordAction_OnAnySwordHit(object sender, EventArgs e)
    {
        ScreenShake.Instance.Shake(2f);
    }

    private void GrenadeProjectile_OnAnyGrenadeExploded(object sender, EventArgs e)
    {
        ScreenShake.Instance.Shake(5f);
    }

    private void ShootAction_OnAnyShoot(object sender, ShootAction.OnAttackEventArgs e)
    {
        ScreenShake.Instance.Shake();
    }
}

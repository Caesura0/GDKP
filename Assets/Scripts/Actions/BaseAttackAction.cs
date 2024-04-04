using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAttackAction : BaseAction
{


    public class OnAttackEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
        public WeaponType weaponType;
        public IDestructable destructable;

    }


    protected enum State
    {
        Aiming,
        Shooting,
        Cooloff,
        KickBeforeHit,
        SwingingSwordBeforeHit,
        SwingingSwordAfterHit,
        Reloading,
        
    }

    [SerializeField] protected int baseAiKillWeight = 10;
    [SerializeField] protected LayerMask obstaclesLayerMask;
    [SerializeField] protected int maxAttackDistance = 7;
    [SerializeField] protected int minShootDistance = 1;
    [SerializeField] protected int damage = 40;
    [Range(0, 1.5f)]
    [SerializeField] protected  float weaponAccuracy = 9;
    [Range(0, .8f)]
    [SerializeField] protected  float distancePenaltyWeight = 9;

    [SerializeField] protected float coolOffStateTime = 0.5f;
    [SerializeField] protected float shootingStateTime = 0.1f;
    [SerializeField] protected float aimingStateTime = 1.2f;
    [SerializeField] protected float reloadingStateTime = 1f;
    [SerializeField] protected int maxAmmo = 5;
    [SerializeField] protected WeaponType weaponType; 


    protected State state;

    protected float stateTimer;
    protected Unit targetUnit;
    protected bool canShootBullet;

    protected Unit prevTargetUnit;
    protected int currentAmmo;

    protected abstract void NextState();

    //public abstract IEnumerator OpenAttackConfirmDialogue(GridPosition unitPosition);

    private void Start()
    {
        currentAmmo = maxAmmo;
    }



    public abstract void TakeReloadAction(Action onActionComplete);

    //public abstract List<GridPosition> KillShotAvailableList(GridPosition gridPosition, out Dictionary<GridPosition, int> killShotGridPositionDictionary);

    public abstract List<GridPosition> GetValidActionGridPositionList(GridPosition gridPosition);


    /// <summary>
    /// Just checks if you will kill any enemy at this grid position
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <returns></returns>
    //public abstract bool KillShotAvailbleAtGridPosition(GridPosition gridPosition);


    public virtual bool KillShotAvailbleAtGridPosition(GridPosition gridPosition)
    {

        List<GridPosition> availbleGridPositionActionList = GetValidActionGridPositionList(gridPosition);
        foreach (GridPosition actionPosition in availbleGridPositionActionList)
        {
            Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(actionPosition);
            int killDamageCalculation = (int)targetUnit.GetCurrentHealth() - damage;
            if (killDamageCalculation >= 0)
            {
                return true;
            }
        }
        return false;
    }


    public abstract int GetTargetCountAtPosition(GridPosition gridPosition);

    public Unit GetTargetUnit()
    {
        return targetUnit;
    }


    public float GetAccuracy()
    {
        return weaponAccuracy;
    }

    public float GetDistancePenaltyWeight()
    {
        return distancePenaltyWeight;
    }
    public int GetMaxAttackDistance()
    {
        return maxAttackDistance;
    }
    public int GetMinAttackDistance()
    {
        return minShootDistance;
    }
    public int GetWeaponDamage()
    {
        return damage;
    }

    public int GetMaxAmmo()
    {
        return maxAmmo;
    }
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }


    

}
public enum WeaponType
{
    Shotgun,
    Rifle,
    Pistol,
    Kick,
    Melee
}
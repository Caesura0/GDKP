using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeProjectile : MonoBehaviour
{

    public static event EventHandler OnAnyGrenadeExploded;

    [SerializeField] Transform grenadeExplosionVFXPrefab;
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] AnimationCurve arcYAnimationCurve;
    [SerializeField] float damageRadius = 4f;
    Vector3 targetPosition;
    Vector3 positionXZ;
    [SerializeField] int grenadeDamage = 30;
    Action OnGrenameBehaviorComplete;
    float totalDistance;

    private void Update()
    {
        Vector3 moveDir = (targetPosition - positionXZ).normalized;

        float moveSpeed = 10f;

        positionXZ += moveDir * moveSpeed * Time.deltaTime;

        float distance = Vector3.Distance(positionXZ, targetPosition);

        //we need this to be invereted, we need it to get closer to 1, not closer to zero as we move
        float distanceNormalized = 1 - distance / totalDistance;

        float maxHeight = totalDistance / 4f;

        float positionY = arcYAnimationCurve.Evaluate(distanceNormalized) * maxHeight;

        transform.position = new Vector3(positionXZ.x, positionY, positionXZ.z);

        float reachedTargetDistance = .2f;

        if (Vector3.Distance(transform.position, targetPosition) < reachedTargetDistance)
        {
            transform.position = targetPosition;
            Collider[] colliderArray = Physics.OverlapSphere(targetPosition, damageRadius);

            foreach(Collider collider in colliderArray)
            {
                if(collider.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    targetUnit.Damage(grenadeDamage);
                }
                if (collider.TryGetComponent<IDestructable>(out IDestructable crate))
                {
                    crate.Damage();
                }
            }
            OnAnyGrenadeExploded?.Invoke(this, EventArgs.Empty);
            trailRenderer.transform.parent = null;
            Instantiate(grenadeExplosionVFXPrefab, targetPosition + Vector3.up * 1.7f, Quaternion.identity);
            Destroy(gameObject);
            OnGrenameBehaviorComplete();
        }
    }


    public void Setup(GridPosition targetGridPosition, Action OnGrenameBehaviorComplete, float damageRadius)
    {
        targetPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);
        this.OnGrenameBehaviorComplete = OnGrenameBehaviorComplete;
        this.damageRadius = damageRadius;
        positionXZ = transform.position;
        positionXZ.y = 0;
        totalDistance = Vector3.Distance(positionXZ, targetPosition);
    }


    public float GetGrenadeRadius()
    {
        return damageRadius;
    }


    public float GetGrenadeDamage()
    {
        return grenadeDamage;
    }

}

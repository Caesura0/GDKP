using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExplodableCrate : MonoBehaviour, IDestructable
{
    [SerializeField] Transform crateParts;
    [SerializeField] int damage = 20;
    [SerializeField] float damageRadius = 20;
    [SerializeField] ParticleSystem explosion;
    [SerializeField] int knockbackMultiplier = 3;

    GridPosition gridPosition;

    public static EventHandler onAnyBarrelExplosion;

    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.SetIDestructableAtGridPosition(gridPosition, this);
        PathFinding.Instance.SetIsWalkableGridPosition(gridPosition, false);
    }

    public void Damage()
    {
        if(crateParts != null)
        {
            Transform crateDestroyedTransform = Instantiate(crateParts, transform.position, Quaternion.identity);
            ApplyExplosionToChildren(crateDestroyedTransform, 150f, transform.position, 10f);
        }

        onAnyBarrelExplosion?.Invoke(this,EventArgs.Empty);
        if(explosion != null)
        {
            explosion.Play();
        }
        Destroy(gameObject);
        ApplyDamageToArea();
    }

    void ApplyExplosionToChildren(Transform root, float explosionForce, Vector3 explosionPosition, float explosionRange)
    {
        foreach (Transform child in root)
        {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody))
            {
                childRigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
            }
            ApplyExplosionToChildren(child, explosionForce, explosionPosition, explosionRange);
        }
    }

    void ApplyDamageToArea()
    {

        Collider[] colliderArray = Physics.OverlapSphere(transform.position, damageRadius);

        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent<Unit>(out Unit targetUnit))
            {
                Vector3 aimDirection = (targetUnit.GetWorldPosition() - transform.position).normalized;

                //this needs to be grabbed from the grid/pathfinding scripts
                int gridMultiplier = 2;
                Vector3 rawKnockbackLocation = targetUnit.GetWorldPosition() + aimDirection * knockbackMultiplier * gridMultiplier;
                GridPosition knockbackGridPosition = LevelGrid.Instance.GetGridPosition(rawKnockbackLocation);
                targetUnit.Damage(damage);
                targetUnit.TriggerKnockback(LevelGrid.Instance.GetWorldPosition(knockbackGridPosition), aimDirection, knockbackGridPosition, targetUnit.GetGridPosition());
            }
            if (collider.TryGetComponent<IDestructable>(out IDestructable crate) && collider.transform != transform)
            {
                crate.Damage();
            }
        }
    }

    public Vector3 GetWorldPosition()
    {
        return LevelGrid.Instance.GetWorldPosition(gridPosition);
    }
}

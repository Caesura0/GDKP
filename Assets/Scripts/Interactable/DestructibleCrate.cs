using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleCrate : MonoBehaviour, IDestructable
{
    [SerializeField] Transform crateParts;

    GridPosition gridPosition;
    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.SetIDestructableAtGridPosition(gridPosition, this);
        PathFinding.Instance.SetIsWalkableGridPosition(gridPosition, false);
    }

    public void Damage()
    {
        Transform crateDestroyedTransform = Instantiate(crateParts, transform.position, Quaternion.identity);
        ApplyExplosionToChildren(crateDestroyedTransform, 150f, transform.position, 10f);
        Destroy(gameObject);
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



    public Vector3 GetWorldPosition()
    {
        return LevelGrid.Instance.GetWorldPosition(gridPosition);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{

    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] Transform bulletHitVFX;
    [SerializeField] float moveSpeed = 200f;

    private Vector3 targetPosition;
    public void Setup(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }
    private void Update()
    {

        var toTarget = targetPosition - transform.position;
        var dist = toTarget.magnitude;

        if (dist > 0)
        {
            var move = toTarget.normalized * (moveSpeed * Time.deltaTime);
            if (move.magnitude > dist)
            {
                move = toTarget;
            }
            transform.position += move;
        }
        else
        {
            transform.position = targetPosition;
            trailRenderer.transform.parent = null;
            Destroy(gameObject);
            Instantiate(bulletHitVFX, targetPosition, Quaternion.identity);
        }
    }
}

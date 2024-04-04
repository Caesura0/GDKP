using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] int health = 100;
    [SerializeField] int healthMax = 100;

    public event EventHandler onDie;
    public event EventHandler onDamaged;

    private void Awake()
    {
        health = healthMax;
    }


    public void Damage(int damageAmount)
    {
        health = Mathf.Clamp(health - damageAmount, 0, healthMax);
        onDamaged?.Invoke(this, EventArgs.Empty);
        if(health == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        onDie?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float)health / healthMax;
    }

    public float GetCurrentHealth()
    {
        return health;
    }


    //not currently used
    public float AttackScore()
    {
        //maybe use this to return health
        float unitPerHealthPoint = 100 / healthMax;  //the higher the health, the lower this score will be
        return (healthMax - health) * unitPerHealthPoint + unitPerHealthPoint;
    }
}

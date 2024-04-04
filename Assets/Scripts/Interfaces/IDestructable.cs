using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDestructable 
{
    public void Damage();

    public Vector3 GetWorldPosition();
}

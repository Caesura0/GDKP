using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ScreenShake : MonoBehaviour
{

    public static ScreenShake Instance { private set; get; }

    CinemachineImpulseSource cinemachineImpulse;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There's more then one ScreenShake" + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cinemachineImpulse = GetComponent<CinemachineImpulseSource>();
    }


    private void Update()
    {
        
    }

    public void Shake(float intensity = 1f)
    {
        cinemachineImpulse.GenerateImpulse(intensity);
    }
}

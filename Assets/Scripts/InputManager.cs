#define USE_NEW_INPUT_SYSTEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { private set; get; }

    PlayerInputActions playerInputActions;



    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There's more then one InputManager" + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }


    public Vector2 GetMouseScreenPosition()
    {
#if USE_NEW_INPUT_SYSTEM
        return Mouse.current.position.ReadValue();
#else
        return Input.mousePosition;
#endif

    }

    public bool IsMouseButtonDownThisFrame()
    {
#if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.Click.WasPressedThisFrame();
#else
        return Input.GetMouseButtonDown(0);
#endif

    }

    public float GetCameraRotateAmount()
    {
#if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.CameraRotation.ReadValue<float>();
#else
        float rotateAmount = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            rotateAmount = 1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotateAmount = -1f;
        }

        return rotateAmount;
#endif

    }



    public bool GetCameraMoveHeldDown()
    {
        return playerInputActions.Player.CameraMovement.IsPressed();
    }


    public bool GetCameraMovePressedThisFrame()
    {

        return playerInputActions.Player.CameraMovement.WasPerformedThisFrame() ;
    }


    public Vector2 GetCameraMoveVector()
    {
#if USE_NEW_INPUT_SYSTEM

        return playerInputActions.Player.CameraMovement.ReadValue<Vector2>();

        
#else
        Vector2 inputMoveDir = new Vector2(0,0);
        if (Input.GetKey(KeyCode.W))
        {
            inputMoveDir.y = 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputMoveDir.y = -1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputMoveDir.x = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputMoveDir.x = 1f;
        }
        return inputMoveDir;
#endif


    }


    public float GetCameraZoomAmount()
    {
#if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.CameraZoom.ReadValue<float>();
#else
        float zoomAmount = 0f;
        if (Input.mouseScrollDelta.y > 0)
        {
            zoomAmount = -1f;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            zoomAmount = +1f;
        }
        return zoomAmount;
#endif

    }

    public bool GetNextUnitButtonDownThisFrame()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.SwitchUnit.WasPressedThisFrame();

#else
        return Input.GetKeyDown(KeyCode.Tab);
#endif
    }

    public bool GetActionInput(out int keyIndex)
    {
        if (playerInputActions.Player.ActionSelection.WasPressedThisFrame())
        {
            keyIndex = (int)playerInputActions.Player.ActionSelection.ReadValue<float>();
            return true;
        }
        else
        {
            keyIndex = -1;
            return false;
        }
    }



}

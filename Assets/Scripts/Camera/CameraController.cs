
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    const float MIN_FOLLOW_Y_OFFSET = 2f;
    const float MAX_FOLLOW_Y_OFFSET = 12f;



    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float rotationSpeed = 100f;
    [SerializeField] CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] float updateInterval = 0.2f; 
    [SerializeField] float holdTimeUpdate = .5f; 

    float timeSinceLastUpdate = 0f;
    float timeSinceHeldDown = 0f;
    CinemachineTransposer cinemachineTransposer;
    Vector3 targetFollowOffset;


    Unit enemyUnitFollow;
    bool isFirstUpdate;
    bool isMoving; 


    private void Start()
    {
        cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        targetFollowOffset = cinemachineTransposer.m_FollowOffset;
        UnitActionSystem.Instance.OnSelectedUnitChange += UnitActionSystem_OnSelectedUnitChange;
        EnemyAI.Instance.OnActiveEnemyUnitChanged += EnemyAI_OnActiveEnemyUnitChanged;
        
    }



    private void EnemyAI_OnActiveEnemyUnitChanged(Unit unit)
    {
        enemyUnitFollow = unit;
    }


    private void UnitActionSystem_OnSelectedUnitChange(object sender, EventArgs e)
    {
        //this was needed because i added logic to 'TAB' through all available units
        MoveCameraToViewSelectedUnit();

    }

    private void MoveCameraToViewSelectedUnit()
    {
        Vector3 positionToCheck = UnitActionSystem.Instance.GetSelectedUnit().transform.position;
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(positionToCheck);

        if (viewportPosition.x >= 0 && viewportPosition.x <= 1 && viewportPosition.y >= 0 && viewportPosition.y <= 1)
        {
            //Unit is already on screen, no need for an abrasive switch
            return;
        }
        else
        {
            //Unit is not on screen, move to position
            transform.position = positionToCheck;
        }
    }

    void Update()
    {
        if (!isFirstUpdate)
        {
            isFirstUpdate = true;
            transform.position = UnitActionSystem.Instance.GetSelectedUnit().transform.position;
        }



        if(enemyUnitFollow != null)
        {
            transform.position = enemyUnitFollow.transform.position;
        }

        //HandleMovement();
        HandleGridMovement();

        HandleRotation();

        HandleZoom();
    }

    void HandleMovement()
    {


        Vector2 inputMoveDir = InputManager.Instance.GetCameraMoveVector();
        Vector3 moveVector = transform.forward * inputMoveDir.y + transform.right * inputMoveDir.x;

        transform.position += moveSpeed * Time.deltaTime * moveVector;
    }


    void HandleGridMovement()
    {
        GridPosition cameraPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        GridPosition destinationLocation;
        destinationLocation = cameraPosition;
        Vector2 inputMoveDir = InputManager.Instance.GetCameraMoveVector();


        if (InputManager.Instance.GetCameraMovePressedThisFrame())
        {
            destinationLocation = new GridPosition(cameraPosition.x + (int)inputMoveDir.x, cameraPosition.z + (int)inputMoveDir.y);
        }


        if (InputManager.Instance.GetCameraMoveHeldDown())
        {

            // Update the timer
            timeSinceLastUpdate += Time.deltaTime;
            timeSinceHeldDown += Time.deltaTime;

            if(timeSinceHeldDown > holdTimeUpdate)
            {

                // Check if it's time to update the grid position
                if (timeSinceLastUpdate >= updateInterval)
                {
                    destinationLocation = new GridPosition(cameraPosition.x + (int)inputMoveDir.x, cameraPosition.z + (int)inputMoveDir.y);
                    // Reset the timer
                    timeSinceLastUpdate = 0f;
                    if (LevelGrid.Instance.IsValidGridPosition(destinationLocation))
                    {
                        transform.position = LevelGrid.Instance.GetWorldPosition(destinationLocation);
                    }
                    return;
                }
                
            }
            if (LevelGrid.Instance.IsValidGridPosition(destinationLocation))
            {
                transform.position = LevelGrid.Instance.GetWorldPosition(destinationLocation);
            }
            return;

        }



        // Reset the timer
        timeSinceLastUpdate = 0f;
        timeSinceHeldDown = 0f;



    }


    void HandleRotation()
    {
        Vector3 rotationVector = new Vector3(0, 0, 0);
        rotationVector.y = InputManager.Instance.GetCameraRotateAmount();
        transform.eulerAngles += rotationSpeed * Time.deltaTime * rotationVector;
    }

    void HandleZoom()
    {
        float zoomIncreaseAmount = 1f;
        targetFollowOffset.y += InputManager.Instance.GetCameraZoomAmount() * zoomIncreaseAmount;
        targetFollowOffset.y = Mathf.Clamp(targetFollowOffset.y, MIN_FOLLOW_Y_OFFSET, MAX_FOLLOW_Y_OFFSET);
        float zoomSpeed = 5f;
        cinemachineTransposer.m_FollowOffset = Vector3.Lerp(cinemachineTransposer.m_FollowOffset, targetFollowOffset, Time.deltaTime * zoomSpeed);
    }
}

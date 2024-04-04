using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] GameObject actionCameraGameObject;
    CinemachineVirtualCamera actionVirtualCamera;

    //bool isFirstCameraChange = false;
    private void Start()
    {
        
        BaseAction.OnAnyActionStart += BaseAction_OnAnyActionStart;
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;
        actionVirtualCamera = actionCameraGameObject.GetComponent<CinemachineVirtualCamera>();
        HideActionCamera();
    }

    void ShowActionCamera()
    {
        actionCameraGameObject.SetActive(true);
        actionVirtualCamera.m_Follow = null;
        actionVirtualCamera.m_LookAt = null;

    }

    void HideActionCamera()
    {
        actionCameraGameObject.SetActive(false);
    }

    void BaseAction_OnAnyActionStart(object sender, EventArgs e)
    {
        switch (sender)
        {
            case ShootAction shootAction:
                {

                    Unit shooterUnit = shootAction.GetUnit();
                    Unit targetUnit = shootAction.GetTargetUnit();
                    //gets shoulder height on the character
                    Vector3 cameraCharacterHeight = Vector3.up * 1.7f; //if i had different height charactors i'd need to serialize this on the unit and pass it through


                    Vector3 shootDir = (targetUnit.GetWorldPosition() - shooterUnit.GetWorldPosition()).normalized;

                    float shoulderOffsetAmount = .5f;
                    Vector3 shoulderOffset = Quaternion.Euler(0, 90, 0) * shootDir * shoulderOffsetAmount;

                    Vector3 actionCameraPosition = shooterUnit.GetWorldPosition() + cameraCharacterHeight + shoulderOffset + (shootDir * -1);


                    actionCameraGameObject.transform.position = actionCameraPosition;
                    actionCameraGameObject.transform.LookAt(targetUnit.GetWorldPosition() + cameraCharacterHeight);

                    ShowActionCamera();
                    break;
                }
            case ShotgunAction shootAction:
                {

                    Unit shooterUnit = shootAction.GetUnit();
                    Unit targetUnit = shootAction.GetTargetUnit();
                    if (targetUnit != null)
                    {
                        //gets shoulder height on the character
                        Vector3 cameraCharacterHeight = Vector3.up * 1.7f; //if i had different height charactors i'd need to serialize this on the unit and pass it through


                        Vector3 shootDir = (targetUnit.GetWorldPosition() - shooterUnit.GetWorldPosition()).normalized;

                        float shoulderOffsetAmount = .5f;
                        Vector3 shoulderOffset = Quaternion.Euler(0, 90, 0) * shootDir * shoulderOffsetAmount;

                        Vector3 actionCameraPosition = shooterUnit.GetWorldPosition() + cameraCharacterHeight + shoulderOffset + (shootDir * -1);


                        actionCameraGameObject.transform.position = actionCameraPosition;
                        actionCameraGameObject.transform.LookAt(targetUnit.GetWorldPosition() + cameraCharacterHeight);

                        ShowActionCamera();
                    }

                    break;
                }


            case SwordAction shootAction:
                {

                    Unit shooterUnit = shootAction.GetUnit();
                    Unit targetUnit = shootAction.GetTargetUnit();
                    //gets shoulder height on the character
                    Vector3 cameraCharacterHeight = Vector3.up * 1.7f; //if i had different height charactors i'd need to serialize this on the unit and pass it through


                    Vector3 shootDir = (targetUnit.GetWorldPosition() - shooterUnit.GetWorldPosition()).normalized;

                    float shoulderOffsetAmount = .5f;
                    Vector3 shoulderOffset = Quaternion.Euler(0, 90, 0) * shootDir * shoulderOffsetAmount;

                    Vector3 actionCameraPosition = shooterUnit.GetWorldPosition() + cameraCharacterHeight + shoulderOffset + (shootDir * -1);


                    actionCameraGameObject.transform.position = actionCameraPosition;
                    actionCameraGameObject.transform.LookAt(targetUnit.GetWorldPosition() + cameraCharacterHeight);

                    ShowActionCamera();
                    break;
                }

            case KickAction shootAction:
                {

                    Unit shooterUnit = shootAction.GetUnit();
                    Unit targetUnit = shootAction.GetTargetUnit();
                    //gets shoulder height on the character
                    Vector3 cameraCharacterHeight = Vector3.up * 1.7f; //if i had different height charactors i'd need to serialize this on the unit and pass it through


                    Vector3 shootDir = (targetUnit.GetWorldPosition() - shooterUnit.GetWorldPosition()).normalized;

                    float shoulderOffsetAmount = .5f;
                    Vector3 shoulderOffset = Quaternion.Euler(0, 90, 0) * shootDir * shoulderOffsetAmount;

                    Vector3 actionCameraPosition = shooterUnit.GetWorldPosition() + cameraCharacterHeight + shoulderOffset + (shootDir * -1);


                    actionCameraGameObject.transform.position = actionCameraPosition;
                    actionCameraGameObject.transform.LookAt(targetUnit.GetWorldPosition() + cameraCharacterHeight);

                    ShowActionCamera();
                    break;
                }
        }
    }


    void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        switch (sender)
        {
            case ShootAction shootAction:
                {
                    HideActionCamera();
                    break;
                }
            case BaseAttackAction shootAction:
                {
                    HideActionCamera();
                    break;
                }
        }
    }
}

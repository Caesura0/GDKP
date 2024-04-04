using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class IntroCameraController : MonoBehaviour
{
    //[SerializeField] Transform cameraPosition;
    [SerializeField] List<MercUnit> unitList;
    [SerializeField] CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] SimpleDialogue simpleDialogue;
    [SerializeField] MercUI mercUI;
    //[SerializeField] float cameraDistance = 2f;
    //[SerializeField] float shoulderOffsetAmount = .5f;
    //[SerializeField] float heightOffsetAmount = .5f;
    [SerializeField] float smoothTime = 0.5f;
    //[SerializeField] float smoothTimeRotation = 0.25f;

    int unitIndex = 0;
    Vector3 actionCameraPosition;
    Transform currentMerc;

    bool isAtCharacterSelect = false;
    bool moveToNextScene = false;
    List<MercUnit> mercUnitList;

    float nextSceneTimer;

    const float nextSceneSeconds = 2.5f;

    private Vector3 cameraVelocity = Vector3.zero;
    private Vector3 lookAtVelocity = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        mercUnitList = new List<MercUnit>(); 
        mercUI.OnUnitHired += MercUI_OnUnitHired;
    }

    private void MercUI_OnUnitHired(object sender, MercUnit e)
    {
        Debug.Log(mercUnitList.Count);
        mercUnitList.Add(e);
        if(mercUnitList.Count > 3) 
        {
            mercUI.gameObject.SetActive(false);
            nextSceneTimer = nextSceneSeconds;
            moveToNextScene= true;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if(moveToNextScene && nextSceneTimer > 0f)
        {
            nextSceneTimer -= Time.deltaTime;
        }
        if(moveToNextScene && nextSceneTimer <= 0f)
        {
            Loader.Load(Loader.Scene.AssetTestLevel);
        }

        if (InputManager.Instance.GetActionInput(out int keyIndex) && !isAtCharacterSelect)
        {
            SwitchToCharacterSelect();
        }


        if (isAtCharacterSelect)
        {
            if (InputManager.Instance.GetNextUnitButtonDownThisFrame())
            {
                FirstPersonLookAt();
                //OldCameraControl();
            }

            if (currentMerc != null)
            {
                cinemachineVirtualCamera.transform.position = Vector3.SmoothDamp(cinemachineVirtualCamera.transform.position, actionCameraPosition, ref cameraVelocity, smoothTime);
                cinemachineVirtualCamera.m_LookAt = currentMerc;
                //cinemachineVirtualCamera.transform.LookAt(currentMerc.position);
            }
        }


    }




    public void SwitchToCharacterSelect()
    {
        isAtCharacterSelect = true;
        currentMerc = unitList[unitIndex].GetCameraLookAtTarget();
        mercUI.SetNextMerc(unitList[unitIndex].mercName, unitList[unitIndex].price, unitList[unitIndex].Abilities, unitList[unitIndex]);
        actionCameraPosition = unitList[unitIndex].GetCameraPositionMark().position;//+ cameraCharacterHeight + shoulderOffset + (shootDir)
    }




    //private void OldCameraControl()
    //{
    //    Transform target = unitList[unitIndex].transform;
    //    cinemachineVirtualCamera.m_LookAt = target;
    //    cinemachineVirtualCamera.m_Follow = target;



    //    Vector3 cameraPosition = target.position - target.forward * cameraDistance;
    //    cinemachineVirtualCamera.transform.position = cameraPosition;
    //    // Calculate the desired rotation for the camera to face the character's face
    //    Quaternion targetRotation = Quaternion.LookRotation(target.position - cinemachineVirtualCamera.transform.position, Vector3.up);


    //    // Smoothly rotate the camera towards the desired rotation
    //    cinemachineVirtualCamera.transform.rotation = Quaternion.RotateTowards(cinemachineVirtualCamera.transform.rotation, targetRotation, 2f * Time.deltaTime);

    //    FindNextMerc();
    //}





    void FirstPersonLookAt()
    {
        FindNextMerc();
        currentMerc = unitList[unitIndex].GetCameraLookAtTarget();
        mercUI.SetNextMerc(unitList[unitIndex].mercName, unitList[unitIndex].price, unitList[unitIndex].Abilities, unitList[unitIndex]);
        actionCameraPosition = unitList[unitIndex].GetCameraPositionMark().position;//+ cameraCharacterHeight + shoulderOffset + (shootDir)


    }

    void FindNextMerc()
    {

        if (unitIndex + 1 < unitList.Count)
        {
            unitIndex++;
        }
        else
        {
            unitIndex = 0;
        }
    }
}





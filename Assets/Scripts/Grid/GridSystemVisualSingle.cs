using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class GridSystemVisualSingle : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshRenderer top;
    [SerializeField] private MeshRenderer bottom;
    [SerializeField] private MeshRenderer right;
    [SerializeField] private MeshRenderer left;



    public void Show(Material material)
    {
        meshRenderer.enabled = true;
        meshRenderer.material = material;
        Vector3 nextPos = transform.position;

        nextPos.y = LevelGrid.Instance.GetLevelGridTransform().y + 1.8f;
        RaycastHit hit;
        Physics.Raycast(nextPos, Vector3.down, out hit, LevelGrid.Instance.GetWalkableLayers());
        Vector3 newTargetPosition = transform.position;
        newTargetPosition.y = hit.point.y + .1f;
        
        transform.position = newTargetPosition;
    }

    //public void Hide()
    //{
    //    meshRenderer.enabled = false;
    //}


    public void ShowBoarder(


            //bool isTarget,
            //Material targetMaterial,
            //Color targetColor,
            Material boundsMaterial,
            //Color boundsColor,
            bool showTop = false,
            bool showRight = false,
            bool showBottom = false,
            bool showLeft = false
        )
    {
        //if (isTarget)
        //{
        //    if (targetMaterial)
        //    {
        //        meshRenderer.material = targetMaterial;
        //    }

        //    meshRenderer.material.SetColor("_BaseColor", targetColor);

        //}


        top.gameObject.SetActive(showTop);
        right.gameObject.SetActive(showRight);
        bottom.gameObject.SetActive(showBottom);
        left.gameObject.SetActive(showLeft);

        top.material = boundsMaterial;
        right.material = boundsMaterial;
        bottom.material = boundsMaterial;
        left.material = boundsMaterial;
    }

    public void Hide()
    {
        meshRenderer.enabled = false;
        /*targetParticleSystem.Stop();
        targetParticleSystem
            .GetComponent<ParticleSystemRenderer>()
            .renderMode = ParticleSystemRenderMode.None;*/
        top.gameObject.SetActive(false);
        right.gameObject.SetActive(false);
        bottom.gameObject.SetActive(false);
        left.gameObject.SetActive(false);
    }



}

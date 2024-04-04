using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static GridSystemVisual;
using static LevelGrid;

public class GridSystemVisual : MonoBehaviour
{
    public static GridSystemVisual Instance { private set; get; }

    [SerializeField] Transform gridSystemVisualHolder;
    LineRenderer pathLineRenderer;
    [SerializeField] LineRenderer pathLineRendererPrefab;

    [Serializable]
    public struct GridVisualTypeMaterial
    {
        public GridVisualType gridVisualType;
        public Material gridMaterial;
        public Material boarderMaterial;

    }

    public enum GridVisualType
    {
        White,
        Blue,
        Red,
        Yellow,
        RedSoft,
        Green,
        Purple
    }

 
    [SerializeField] Transform gridSystemVisualSinglePrefab;
    Transform grenadeRangeSphere;
    [SerializeField] Transform grenadeRangeSpherePrefab;
    [SerializeField] List<GridVisualTypeMaterial> gridVisualTypeMaterialList;


    private GridSystemVisualSingle[,] gridSystemVisualSingleArray;

    private void Start()
    {

        if (Instance != null)
        {
            Debug.Log("There's more then one GridSustemVisual " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        grenadeRangeSphere = GameObject.Instantiate(grenadeRangeSpherePrefab);
        pathLineRenderer = GameObject.Instantiate(pathLineRendererPrefab);

        gridSystemVisualSingleArray = new GridSystemVisualSingle[LevelGrid.Instance.GetWidth(), LevelGrid.Instance.GetHeight()];

        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Transform gridSystemVisualSingleTransform =  
                    Instantiate(gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);
                gridSystemVisualSingleArray[x, z] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();
            }
        }

        UnitActionSystem.Instance.OnSelectedActionChange += UnitActionSystem_OnSelectedActionChanged;
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPosition;

        UpdateGridVisual();
    }

    private void Update()
    {
        
        if(UnitActionSystem.Instance.GetSelectedAction() == null) { return; }
        if (UnitActionSystem.Instance.GetSelectedAction().GetValidActionGridPositionList().Contains(
            LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition())
                )
            )
        {
            if (UnitActionSystem.Instance.GetSelectedAction() is GrenadeAction)
            {
                grenadeRangeSphere.gameObject.SetActive(true);
                MoveGrenadeVisual();
                UpdateGridVisual();
            }
            if (UnitActionSystem.Instance.GetSelectedAction() is OverwatchAction)
            {
                UpdateGridVisual();
            }
            else
            {
                grenadeRangeSphere.gameObject.SetActive(false);
            }
            if(UnitActionSystem.Instance.GetSelectedAction() is MoveAction)
            {
                DrawPathFinding();
            }
            else
            {
                pathLineRenderer.enabled = false;
            }
        }
        else
        {
            grenadeRangeSphere.gameObject.SetActive(false);
            pathLineRenderer.positionCount = 0;
        }

    }

    private void DrawPathFinding()
    {
        pathLineRenderer.enabled = true;
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        GridPosition mousePosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());
        List<GridPosition> calculatedPath = PathFinding.Instance.FindPath(selectedUnit.GetGridPosition(), mousePosition, out int pathLength);


        if (calculatedPath.Count() <= 2) { pathLineRenderer.positionCount = 0; return; }

        pathLineRenderer.positionCount = calculatedPath.Count;

        pathLineRenderer
                            .SetPositions(calculatedPath
                                .Select(p =>
                                {
                                    var v = LevelGrid.Instance.GetWorldPosition(p);
                                    v.y += 0.1f;
                                    return v;
                                })
                                .ToArray());
    }

    public void HideAllGridPositions()
    {
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                gridSystemVisualSingleArray[x, z].Hide();
            }
        }
    }


    //need to add a way to check against min range, worried i will need to run the same check again for min.
    void ShowGridPositionRange(GridPosition gridPosition, int maxRange,  GridVisualType gridVisualType, int minRange = 0)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -maxRange; x <= maxRange ; x++)
        {
            for (int z = -maxRange; z <= maxRange; z++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, z);
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) { continue; }



                //This will remove the corner pieces
                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > maxRange) { continue; }
                if(testDistance < minRange) { continue; }

                gridPositionList.Add(testGridPosition);
            }
        }
        ShowGridPositionList(gridPositionList, gridVisualType);

    }


    void ShowGridPositionRangeSquare(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, z);
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) { continue; }
                gridPositionList.Add(testGridPosition);
            }
        }
        ShowGridPositionList(gridPositionList, gridVisualType);
    }


    public void ShowGridPositionList(List<GridPosition> gridPositionList, GridVisualType gridVisualType)
    {
        
        foreach(GridPosition gridPosition in gridPositionList)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    GridPosition testGridPosition = gridPosition + new GridPosition(x, z);
                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) { continue; }
                    if (gridPositionList.Contains(testGridPosition)) { continue; }

                }
            }
            ShowGridVisualBoardRange(gridPositionList, GetGridBoarderVisualTypeMaterial(gridVisualType));
            gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].Show(GetGridVisualTypeMaterial(gridVisualType));
        }
    }


    void UpdateGridVisual()
    {

        HideAllGridPositions();
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
        GridVisualType gridVisualType;
        var mousePos = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());

        if (selectedAction == null) { return; }
        switch (selectedAction)
        {
            default:
            case MoveAction move:
                gridVisualType = GridVisualType.White;
                ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
                //Debug.Log(selectedAction + "after get valid");
                break;
            case PassAction pass:
                gridVisualType = GridVisualType.Blue;
                ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
                break;
            case ShootAction shoot :
                gridVisualType = GridVisualType.Red;
                ShowGridPositionRange(selectedUnit.GetGridPosition(), shoot.GetMaxAttackDistance(), GridVisualType.RedSoft, shoot.GetMinAttackDistance());
                ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
                break;
            case OverwatchAction shoot:
                gridVisualType = GridVisualType.Yellow;
                ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
                ShowGridPositionRange(mousePos, (int)shoot.GetCoverAreaSize(), GridVisualType.Red);
                break;
            case ShotgunAction shotgun:
                gridVisualType = GridVisualType.Red;
                ShowGridPositionRange(selectedUnit.GetGridPosition(), shotgun.GetMaxAttackDistance(), GridVisualType.RedSoft);
                ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
                break;
            case GrenadeAction grenade:
                gridVisualType = GridVisualType.Purple;
                ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
                ShowGridPositionRange(mousePos, (int)grenade.GetDamageRadius() / 2, GridVisualType.Red);
                break;
            case SwordAction sword:
                gridVisualType = GridVisualType.Red;
                ShowGridPositionRange(selectedUnit.GetGridPosition(), sword.GetMaxSwordDistance(), GridVisualType.RedSoft);
                ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
                break;
            case KickAction kick:
                gridVisualType = GridVisualType.Red;
                ShowGridPositionRange(selectedUnit.GetGridPosition(), kick.GetMaxKickDistance(), GridVisualType.RedSoft);
                ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
                break;
            case InteractAction interact:
                gridVisualType = GridVisualType.Blue;
                ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
                break;
            case HealAction heal:
                gridVisualType = GridVisualType.Green;
                ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
                break;

        }
        
    }

    void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    void LevelGrid_OnAnyUnitMovedGridPosition(object sender, OnAnyUnitMovedGridPositionEventArgs e)
    {

        UpdateGridVisual();
    }

    Material GetGridVisualTypeMaterial(GridVisualType gridVisualType)
    {
        foreach(GridVisualTypeMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList)
        {
            if(gridVisualTypeMaterial.gridVisualType == gridVisualType)
            {
                return gridVisualTypeMaterial.gridMaterial;
            }
        }
        Debug.LogError("Could not find GridVisualTypeMaterial for GridVisualType " + gridVisualType);
        return null;
    }

    Material GetGridBoarderVisualTypeMaterial(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList)
        {
            if (gridVisualTypeMaterial.gridVisualType == gridVisualType)
            {
                return gridVisualTypeMaterial.boarderMaterial;
            }
        }
        Debug.LogError("Could not find GridVisualTypeMaterial for GridVisualType " + gridVisualType);
        return null;
    }


    void MoveGrenadeVisual()
    {
        var mouseGridPos = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());
        grenadeRangeSphere.position = LevelGrid.Instance.GetWorldPosition(mouseGridPos);
    }

    void ShowGridVisualBoardRange(List<GridPosition> gridPositionList, Material boundsMaterial)
    {
        foreach (var gridPosition in gridPositionList)
        {

            bool showTop =
                !(gridPositionList.Contains(gridPosition + new GridPosition(0, 1)));
            bool showRight =
                !(gridPositionList.Contains(gridPosition + new GridPosition(1, 0)));
            bool showBottom =
                !(gridPositionList.Contains(gridPosition + new GridPosition(0, -1)));
            bool showLeft =
                !(gridPositionList.Contains(gridPosition + new GridPosition(-1, 0)));

            //bool isTarget =
            //gridPositionArray[gridPosition.x + 1, gridPosition.z + 1] ==
            //GridPositionContent.Target;

            gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].ShowBoarder( boundsMaterial, showTop, showRight, showBottom, showLeft) ;

        }
    }

}

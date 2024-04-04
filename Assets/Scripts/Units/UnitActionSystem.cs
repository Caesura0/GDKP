using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitActionSystem : MonoBehaviour
{

    public static UnitActionSystem Instance { private set; get; }
    

    [SerializeField] Unit selectedUnit;
    [SerializeField] GameObject mouseIndicatorPrefab;
    //[SerializeField] float mouseIndicatorHangTime = .2f;
    [SerializeField] LayerMask unitLayerMask;
    [SerializeField] ActionUIHolder actionUI;

    public event EventHandler OnSelectedUnitChange;
    public event EventHandler OnSelectedActionChange;
    public event EventHandler<bool> OnBusyChange;
    public event EventHandler OnActionStarted;
    public event EventHandler OnReloadActionStarted;

    bool attackConfirmed;
    bool attackCancelled;
    bool reload;
    

    BaseAction selectedAction;
    bool isBusy;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.Log("There's more then one UnitActionSystem" + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    private void Start()
    {
        
        //SetSelectedUnit(selectedUnit);
        TurnSystem.OnAnyTurnChanged += TurnSystem_OnTurnChanged;
    }


    private void Update()
    {
        
        if (isBusy) { return; }
        if(!TurnSystem.Instance.IsPlayerTurn()) { return; }
        if(selectedUnit == null)
        {
            //this caused an error when all units died
            //TODO: Add game over logic?
            SetSelectedUnit(UnitManager.Instance.GetFriendlyUnitsWithAPList()[0]);
        }
        HandleSwitchToNextAvailableUnit();
        TrySelectNewActionHotkey();
        if (EventSystem.current.IsPointerOverGameObject()) { return; }




        if (TryGetClickedUnit( out Unit clickedUnit))
        {
            if (TryHandlePlayerUnitClicked(clickedUnit)) { return; }
            if (TryHandleTargetUnitClicked(clickedUnit))
            {
                HandleSelectedAction(clickedUnit.GetGridPosition());
                return;
            }
        }
        HandleSelectedAction();

    }

    void HandleSelectedAction()
    {
        if (InputManager.Instance.IsMouseButtonDownThisFrame())
        {
            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());
            //Debug.Log(mouseGridPosition + " mousegridPosition");
            //Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(mouseGridPosition);
            //GridPosition unitGridPosition = targetUnit.GetGridPosition();
            //Debug.Log(selectedAction is ShootAction);
            HandleSelectedAction(mouseGridPosition);
        }
    }

    //this should be moved to attack action, maybe its own script i think
    IEnumerator OpenAttackConfirmDialogue(GridPosition unitGridPosition)
    {

        var attackAction = (BaseAttackAction)selectedAction;

        //Unit enemyUnit = LevelGrid.Instance.GetUnitAtGridPosition(unitGridPosition);



        float hitChance = selectedUnit.CalculateAccuracy(unitGridPosition, attackAction);
       // Debug.Log(hitChance + " HitChance");


        actionUI.ShowActionPanel();
        actionUI.SetAccuracyText("Hit = " + hitChance);
        actionUI.SetAttackText("Damage = " + attackAction.GetWeaponDamage());
        actionUI.SetAmmoText(attackAction.GetCurrentAmmo() + " / " + attackAction.GetMaxAmmo());
        actionUI.confirmButton.onClick.AddListener(SetConfirmedFlag);
        actionUI.cancelButton.onClick.AddListener(SetCancelFlag);
        actionUI.reloadButton.onClick.AddListener(SetReloadFlag);
        //wait for player input to approve or cancel attack
        //Debug.Log("waiting for input");
        //this needs to wait for 'Attack' or 'Cancel' 

        while(!attackCancelled && !attackConfirmed && !reload)
        {
            yield return null;
        }


        if (attackCancelled)
        {
            attackCancelled = false;
            //Debug.Log("Attack Cancelled");
            actionUI.HideActionPanel();
        }
        else if (attackConfirmed)
        {
            attackConfirmed = false;
            actionUI.HideActionPanel();
            StartSelectedAction(unitGridPosition);
        }
        else if (reload)
        {
            reload = false;
            StartSelectedReloadAction();
        }


    }


    private void StartSelectedReloadAction()
    {
        if (selectedUnit.TrySpendActionPointsToTakeAction(selectedAction) && selectedAction is BaseAttackAction)
        {
            var attackAction = (BaseAttackAction)selectedAction;
            SetBusy();
            attackAction.TakeReloadAction(ClearBusy);
            OnReloadActionStarted?.Invoke(this, EventArgs.Empty);
        }

    }


    private void StartSelectedAction(GridPosition unitGridPosition)
    {
        if (selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
        {
            SetBusy();
            selectedAction.TakeAction(unitGridPosition, ClearBusy);
            OnActionStarted?.Invoke(this, EventArgs.Empty);
        }

    }

    void SetConfirmedFlag()
    {
        attackConfirmed = true;
        attackCancelled = false;
        reload = false;
    }

    void SetCancelFlag()
    {
        attackConfirmed = false;
        attackCancelled = true;
        reload = false;
    }

    void SetReloadFlag()
    {
        attackConfirmed = false;
        attackCancelled = false;
        reload = true;
    }


    void HandleSelectedAction(GridPosition unitGridPosition)
    {
        if (InputManager.Instance.IsMouseButtonDownThisFrame())
        {
            if (selectedAction != null && selectedAction.IsValidActionGridPosition(unitGridPosition) )
            {
                if (selectedAction is BaseAttackAction)
                {
                    StartCoroutine(OpenAttackConfirmDialogue(unitGridPosition));
                    return;
                }
                if (selectedAction.IsValidActionThisTurn())
                {
                    StartSelectedAction(unitGridPosition);
                }
            }
            else
            {
                //Debug.Log("Invalid Action");
            }
        }
    }



   void HandleSwitchToNextAvailableUnit()
    {
        if (InputManager.Instance.GetNextUnitButtonDownThisFrame())
        {
            TrySwitchToNextAvailableUnit();
        }

    }

   void TrySelectNewActionHotkey()
    {
        int actionIndex;
        if(InputManager.Instance.GetActionInput(out actionIndex))
        {
            var selectedUnitAvailableActions = selectedUnit.GetBaseActionArray();
            if(selectedUnitAvailableActions[actionIndex] != null)
            {
                SetSelectedAction(selectedUnitAvailableActions[actionIndex]);
            }
        }
    }


    private void TrySwitchToNextAvailableUnit()
    {
        List<Unit> unitsWithAP = UnitManager.Instance.GetFriendlyUnitsWithAPList();
        int unitIndex = 100;
        if (selectedUnit != null)
        {
            unitIndex = unitsWithAP.IndexOf(selectedUnit);
        }
        if (unitIndex + 1 < unitsWithAP.Count && selectedUnit != null)
        {
            SetSelectedUnit(unitsWithAP[unitIndex + 1]);
        }
        else
        {

            SetSelectedUnit(unitsWithAP[0]);
        }
        SetCancelFlag();
    }

    bool TryGetClickedUnit(out Unit clickedUnit)
    {
        clickedUnit = null;
        if (InputManager.Instance.IsMouseButtonDownThisFrame())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
            {
                if (raycastHit.transform.TryGetComponent<Unit>(out clickedUnit))
                {
                    return clickedUnit.GetActionPoints() > 0;
                }
            }
        }
        return false;
    }

    bool TryHandlePlayerUnitClicked(Unit unit)
    {
        if(unit == selectedUnit /*and not self casting something*/) { Debug.Log("selected unit"); return false; }
        if (unit.IsEnemy()){ Debug.Log("enemy unit"); return false;}

        //Debug.Log(unit.gameObject.name);
        SetSelectedUnit(unit);
        return true;
    }

    bool TryHandleTargetUnitClicked(Unit unit)
    {
        if (!unit.IsEnemy()/*or spell is self cast*/) { return false; }
        return true;
    }

    void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        OnSelectedUnitChange?.Invoke(this, EventArgs.Empty); 
        SetSelectedAction(unit.GetAction<MoveAction>());
        //TODO: this would be where i start to say"no selected action of the bat"
    }

    public void SetSelectedAction(BaseAction baseAction)
    {
        selectedAction = baseAction;
        OnSelectedActionChange?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    public BaseAction GetSelectedAction()
    {
        return selectedAction;
    }

    private void SetBusy()
    {
        isBusy = true;
        OnBusyChange?.Invoke(this, isBusy);
    }

    private void TryClearUnitWithNoActionPoints()
    {
        if(selectedUnit.GetActionPoints() <= 0)
        {
            List<Unit> unitsWithAP = UnitManager.Instance.GetFriendlyUnitsWithAPList();
            int unitIndex = unitsWithAP.IndexOf(selectedUnit);
            if(unitsWithAP.Count == 0)
            {
                TurnSystem.Instance.NextTurn();
                return;
            }

            if (unitIndex + 1 < unitsWithAP.Count)
            {
                SetSelectedUnit(unitsWithAP[unitIndex + 1]);
            }
            else
            {

                SetSelectedUnit(unitsWithAP[0]);
            }
        }
    }

    private void ClearBusy()
    {
        isBusy = false ;
        OnBusyChange?.Invoke(this, isBusy);
        TryClearUnitWithNoActionPoints();
    }


    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        if (selectedUnit != null)
        {
            OnSelectedUnitChange?.Invoke(this, EventArgs.Empty);
            return;
        }

        List<Unit> friendlyUnitList = UnitManager.Instance.GetFriendlyUnitList();
        if (friendlyUnitList.Count == 0)
        {
            
            Debug.Log("Your team is not responding! This can't be good.");
        }
        else  
        {
            SetSelectedUnit(friendlyUnitList[0]);
        }
    }


    //IEnumerator MouseInputIndicator(Vector3 mousePosition)
    //{
    //    mouseIndicatorPrefab.SetActive(false);
    //    mouseIndicatorPrefab.transform.position = mousePosition;
    //    mouseIndicatorPrefab.SetActive(true);
    //    yield return new WaitForSeconds(mouseIndicatorHangTime);
    //    mouseIndicatorPrefab.SetActive(false);
    //}

}



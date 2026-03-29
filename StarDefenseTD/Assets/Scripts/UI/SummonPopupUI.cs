using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonPopupUI : MonoBehaviour
{
    private enum PopupMode
    {
        None = 0,
        Summon = 1,
        Repair = 2,
        TowerActions = 3,
    }

    private enum PopupAction
    {
        None = 0,
        Summon = 1,
        Upgrade = 2,
        Repair = 3,
        Change = 4,
    }

    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private Button summonButton;
    [SerializeField] private Button changeButton;
    [SerializeField] private TowerSpawner towerSpawner;
    [SerializeField] private PlayerGold playerGold;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Vector3 popupWorldOffset = new Vector3(0.0f, 0.6f, 0.0f);
    [SerializeField] private TMP_Text actionLabelText;
    [SerializeField] private TMP_Text goldCostText;
    [SerializeField] private TMP_Text changeGoldCostText;
    [SerializeField] private GameObject goldPanel;
    [SerializeField] private int changeGold = 75;

    private Transform selectedTile;
    private TowerWeapon selectedTower;
    private Tile selectedRepairTile;
    private PopupMode popupMode;
    private PopupAction primaryAction;
    private PopupAction secondaryAction;

    public Transform SelectedTile => selectedTile;
    public bool IsVisible => popupRoot != null && popupRoot.gameObject.activeSelf;

    private void Awake()
    {
        if (popupRoot == null)
        {
            popupRoot = GetComponent<RectTransform>();
        }

        ValidateInspectorReferences();
        ResolveTowerSpawner();
        ResolvePlayerGold();

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        ApplyStaticCostTexts();

        if (summonButton != null)
        {
            summonButton.onClick.AddListener(OnClickPrimaryAction);
        }

        if (changeButton != null)
        {
            changeButton.onClick.AddListener(OnClickChangeAction);
        }

        Hide();
    }

    private void OnValidate()
    {
        ApplyStaticCostTexts();
    }

    public bool CanShowOn(Transform tileTransform)
    {
        ResolveTowerSpawner();
        return towerSpawner != null && towerSpawner.CanSpawnTower(tileTransform);
    }

    public bool CanShowTowerActionsOn(TowerWeapon towerWeapon)
    {
        ResolveTowerSpawner();
        ResolvePlayerGold();
        return towerSpawner != null && towerWeapon != null && (towerSpawner.CanUpgradeTower(towerWeapon) || CanChangeTowerWithCost(towerWeapon));
    }

    public bool CanShowRepairOn(Tile tile)
    {
        ResolveTowerSpawner();
        return towerSpawner != null && towerSpawner.CanShowRepairTile(tile);
    }

    public void ShowSummon(Transform tileTransform)
    {
        if (popupRoot == null || tileTransform == null)
        {
            return;
        }

        ResolveTowerSpawner();

        popupMode = PopupMode.Summon;
        primaryAction = PopupAction.Summon;
        secondaryAction = PopupAction.None;
        selectedTile = tileTransform;
        selectedTower = null;
        selectedRepairTile = null;
        SetActionLabel("소환");
        SetGoldPanelVisible(true);
        SetPrimaryButtonVisible(true);
        SetChangeButtonVisible(false);
        UpdateGoldCostText();
        UpdateButtonInteractable();
        popupRoot.gameObject.SetActive(true);
        UpdatePopupPosition();
    }

    public void ShowRepair(Tile tile)
    {
        if (popupRoot == null || tile == null)
        {
            return;
        }

        ResolveTowerSpawner();

        popupMode = PopupMode.Repair;
        primaryAction = PopupAction.Repair;
        secondaryAction = PopupAction.None;
        selectedRepairTile = tile;
        selectedTile = tile.transform;
        selectedTower = null;
        SetActionLabel("수리");
        SetGoldPanelVisible(true);
        SetPrimaryButtonVisible(true);
        SetChangeButtonVisible(false);
        UpdateGoldCostText();
        UpdateButtonInteractable();
        popupRoot.gameObject.SetActive(true);
        UpdatePopupPosition();
    }

    public void ShowTowerActions(TowerWeapon towerWeapon)
    {
        if (popupRoot == null || towerWeapon == null)
        {
            return;
        }

        ResolveTowerSpawner();
        ResolvePlayerGold();
        if (towerSpawner == null)
        {
            return;
        }

        bool canUpgrade = towerSpawner.CanUpgradeTower(towerWeapon);
        bool canChange = CanChangeTowerWithCost(towerWeapon);
        if (canUpgrade == false && canChange == false)
        {
            Hide();
            return;
        }

        popupMode = PopupMode.TowerActions;
        selectedTower = towerWeapon;
        selectedTile = null;
        selectedRepairTile = null;
        SetGoldPanelVisible(false);

        primaryAction = canUpgrade ? PopupAction.Upgrade : PopupAction.None;
        secondaryAction = canChange ? PopupAction.Change : PopupAction.None;

        SetPrimaryButtonVisible(canUpgrade);
        SetChangeButtonVisible(canChange);

        if (canUpgrade)
        {
            SetActionLabel("승급");
        }

        UpdateButtonInteractable();
        popupRoot.gameObject.SetActive(true);
        UpdatePopupPosition();
    }

    public void Hide()
    {
        popupMode = PopupMode.None;
        primaryAction = PopupAction.None;
        secondaryAction = PopupAction.None;
        selectedTile = null;
        selectedTower = null;
        selectedRepairTile = null;

        SetPrimaryButtonVisible(true);
        SetChangeButtonVisible(false);

        if (popupRoot != null)
        {
            popupRoot.gameObject.SetActive(false);
        }
    }

    public void OnClickPrimaryAction()
    {
        if (ExecuteAction(primaryAction))
        {
            Hide();
        }
    }

    public void OnClickChangeAction()
    {
        if (ExecuteAction(PopupAction.Change))
        {
            Hide();
        }
    }

    private void ResolveTowerSpawner()
    {
        if (IsSceneInstance(towerSpawner))
        {
            return;
        }

        towerSpawner = FindFirstObjectByType<TowerSpawner>(FindObjectsInactive.Include);
    }

    private void ResolvePlayerGold()
    {
        if (playerGold != null)
        {
            return;
        }

        playerGold = FindFirstObjectByType<PlayerGold>(FindObjectsInactive.Include);
    }

    private bool IsSceneInstance(TowerSpawner spawner)
    {
        return spawner != null && spawner.gameObject.scene.IsValid() && spawner.gameObject.scene.isLoaded;
    }

    private void ValidateInspectorReferences()
    {
        if (summonButton == null)
        {
            Debug.LogWarning("SummonPopupUI: Summon Button is not assigned.", this);
        }

        if (changeButton == null)
        {
            Debug.LogWarning("SummonPopupUI: Change Button is not assigned.", this);
        }

        if (actionLabelText == null)
        {
            Debug.LogWarning("SummonPopupUI: Action Label Text is not assigned.", this);
        }

        if (goldPanel == null)
        {
            Debug.LogWarning("SummonPopupUI: Gold Panel is not assigned.", this);
        }

        if (goldCostText == null)
        {
            Debug.LogWarning("SummonPopupUI: Gold Cost Text is not assigned.", this);
        }

        if (changeGoldCostText == null)
        {
            Debug.LogWarning("SummonPopupUI: Change Gold Cost Text is not assigned.", this);
        }
    }

    private void ApplyStaticCostTexts()
    {
        if (changeGoldCostText != null)
        {
            changeGoldCostText.text = changeGold.ToString();
        }
    }

    private void LateUpdate()
    {
        if (IsVisible == false)
        {
            return;
        }

        if (popupMode == PopupMode.Summon && selectedTile == null)
        {
            Hide();
            return;
        }

        if (popupMode == PopupMode.Repair && (selectedRepairTile == null || selectedRepairTile.RequiresRepair == false))
        {
            Hide();
            return;
        }

        if (popupMode == PopupMode.TowerActions)
        {
            if (selectedTower == null)
            {
                Hide();
                return;
            }

            if (towerSpawner == null || (towerSpawner.CanUpgradeTower(selectedTower) == false && CanChangeTowerWithCost(selectedTower) == false))
            {
                Hide();
                return;
            }
        }

        UpdateGoldCostText();
        UpdateButtonInteractable();
        UpdatePopupPosition();
    }

    private void UpdatePopupPosition()
    {
        if (popupRoot == null)
        {
            return;
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        Vector3 worldPosition = Vector3.zero;
        if (popupMode == PopupMode.Summon && selectedTile != null)
        {
            worldPosition = selectedTile.position + popupWorldOffset;
        }
        else if (popupMode == PopupMode.Repair && selectedRepairTile != null)
        {
            worldPosition = selectedRepairTile.transform.position + popupWorldOffset;
        }
        else if (popupMode == PopupMode.TowerActions && selectedTower != null)
        {
            worldPosition = selectedTower.transform.position + popupWorldOffset;
        }
        else
        {
            return;
        }

        Vector3 screenPosition = worldCamera != null ? worldCamera.WorldToScreenPoint(worldPosition) : worldPosition;
        popupRoot.position = screenPosition;
    }

    private bool ExecuteAction(PopupAction action)
    {
        ResolveTowerSpawner();
        ResolvePlayerGold();

        if (towerSpawner == null)
        {
            return false;
        }

        switch (action)
        {
            case PopupAction.Summon:
                return selectedTile != null && towerSpawner.SpawnTower(selectedTile);
            case PopupAction.Upgrade:
                return selectedTower != null && towerSpawner.TryUpgradeTower(selectedTower);
            case PopupAction.Repair:
                return selectedRepairTile != null && towerSpawner.TryRepairTile(selectedRepairTile);
            case PopupAction.Change:
                if (selectedTower == null || CanChangeTowerWithCost(selectedTower) == false)
                {
                    return false;
                }

                if (towerSpawner.TryChangeTower(selectedTower) == false)
                {
                    return false;
                }

                playerGold.CurrnetGold -= changeGold;
                return true;
            default:
                return false;
        }
    }

    private bool CanChangeTowerWithCost(TowerWeapon towerWeapon)
    {
        if (towerSpawner == null || towerWeapon == null || playerGold == null)
        {
            return false;
        }

        if (changeGold > playerGold.CurrnetGold)
        {
            return false;
        }

        return towerSpawner.CanChangeTower(towerWeapon);
    }

    private void SetActionLabel(string label)
    {
        if (actionLabelText != null)
        {
            actionLabelText.text = label;
        }
    }

    private void SetGoldPanelVisible(bool isVisible)
    {
        if (goldPanel != null)
        {
            goldPanel.SetActive(isVisible);
        }
    }

    private void SetPrimaryButtonVisible(bool isVisible)
    {
        if (summonButton != null)
        {
            summonButton.gameObject.SetActive(isVisible);
        }
    }

    private void SetChangeButtonVisible(bool isVisible)
    {
        if (changeButton != null)
        {
            changeButton.gameObject.SetActive(isVisible);
        }
    }

    private void UpdateGoldCostText()
    {
        if (towerSpawner == null || goldCostText == null)
        {
            return;
        }

        if (popupMode == PopupMode.Repair && selectedRepairTile != null)
        {
            goldCostText.text = selectedRepairTile.RepairGold.ToString();
            return;
        }

        if (popupMode == PopupMode.Summon)
        {
            goldCostText.text = towerSpawner.TowerBuildGold.ToString();
        }
    }

    private void UpdateButtonInteractable()
    {
        if (summonButton != null)
        {
            summonButton.interactable = CanExecuteAction(primaryAction);
        }

        if (changeButton != null)
        {
            changeButton.interactable = CanExecuteAction(secondaryAction);
        }
    }

    private bool CanExecuteAction(PopupAction action)
    {
        if (towerSpawner == null)
        {
            return false;
        }

        switch (action)
        {
            case PopupAction.Summon:
                return selectedTile != null && towerSpawner.CanSpawnTower(selectedTile);
            case PopupAction.Upgrade:
                return selectedTower != null && towerSpawner.CanUpgradeTower(selectedTower);
            case PopupAction.Repair:
                return selectedRepairTile != null && towerSpawner.CanRepairTile(selectedRepairTile);
            case PopupAction.Change:
                return selectedTower != null && CanChangeTowerWithCost(selectedTower);
            default:
                return false;
        }
    }

    private void OnDestroy()
    {
        if (summonButton != null)
        {
            summonButton.onClick.RemoveListener(OnClickPrimaryAction);
        }

        if (changeButton != null)
        {
            changeButton.onClick.RemoveListener(OnClickChangeAction);
        }
    }
}

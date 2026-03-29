using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonPopupUI : MonoBehaviour
{
    private enum PopupMode
    {
        None = 0,
        Summon = 1,
        Upgrade = 2,
    }

    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private Button summonButton;
    [SerializeField] private TowerSpawner towerSpawner;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Vector3 popupWorldOffset = new Vector3(0.0f, 0.6f, 0.0f);
    [SerializeField] private TMP_Text actionLabelText;
    [SerializeField] private TMP_Text goldCostText;
    [SerializeField] private GameObject goldPanel;

    private Transform selectedTile;
    private TowerWeapon selectedTower;
    private PopupMode popupMode;

    public Transform SelectedTile => selectedTile;
    public bool IsVisible => popupRoot != null && popupRoot.gameObject.activeSelf;

    private void Awake()
    {
        if (popupRoot == null)
        {
            popupRoot = GetComponent<RectTransform>();
        }

        if (summonButton == null)
        {
            summonButton = GetComponentInChildren<Button>(true);
        }

        if (actionLabelText == null)
        {
            Transform actionLabelTransform = transform.Find("SummonText");
            if (actionLabelTransform != null)
            {
                actionLabelText = actionLabelTransform.GetComponent<TMP_Text>();
            }
        }

        if (goldPanel == null)
        {
            Transform goldPanelTransform = transform.Find("GoldPanel");
            if (goldPanelTransform != null)
            {
                goldPanel = goldPanelTransform.gameObject;
            }
        }

        if (goldCostText == null && goldPanel != null)
        {
            goldCostText = goldPanel.GetComponentInChildren<TMP_Text>(true);
        }

        ResolveTowerSpawner();

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (summonButton != null)
        {
            summonButton.onClick.AddListener(OnClickAction);
        }

        Hide();
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

        if (popupMode == PopupMode.Upgrade && selectedTower == null)
        {
            Hide();
            return;
        }

        UpdatePopupPosition();
    }

    public bool CanShowOn(Transform tileTransform)
    {
        ResolveTowerSpawner();
        return towerSpawner != null && towerSpawner.CanSpawnTower(tileTransform);
    }

    public bool CanShowUpgradeOn(TowerWeapon towerWeapon)
    {
        ResolveTowerSpawner();
        return towerSpawner != null && towerSpawner.CanUpgradeTower(towerWeapon);
    }

    public void ShowSummon(Transform tileTransform)
    {
        if (popupRoot == null || tileTransform == null)
        {
            return;
        }

        ResolveTowerSpawner();

        popupMode = PopupMode.Summon;
        selectedTile = tileTransform;
        selectedTower = null;
        SetActionLabel("소환");
        SetGoldPanelVisible(true);
        UpdateGoldCostText();
        popupRoot.gameObject.SetActive(true);
        UpdatePopupPosition();
    }

    public void ShowUpgrade(TowerWeapon towerWeapon)
    {
        if (popupRoot == null || towerWeapon == null)
        {
            return;
        }

        ResolveTowerSpawner();

        popupMode = PopupMode.Upgrade;
        selectedTower = towerWeapon;
        selectedTile = null;
        SetActionLabel("승급");
        SetGoldPanelVisible(false);
        popupRoot.gameObject.SetActive(true);
        UpdatePopupPosition();
    }

    public void Hide()
    {
        popupMode = PopupMode.None;
        selectedTile = null;
        selectedTower = null;

        if (popupRoot != null)
        {
            popupRoot.gameObject.SetActive(false);
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

    private bool IsSceneInstance(TowerSpawner spawner)
    {
        return spawner != null && spawner.gameObject.scene.IsValid() && spawner.gameObject.scene.isLoaded;
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
        else if (popupMode == PopupMode.Upgrade && selectedTower != null)
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

    private void OnClickAction()
    {
        ResolveTowerSpawner();
        if (towerSpawner == null)
        {
            return;
        }

        bool isSucceeded = false;
        if (popupMode == PopupMode.Summon && selectedTile != null)
        {
            isSucceeded = towerSpawner.SpawnTower(selectedTile);
        }
        else if (popupMode == PopupMode.Upgrade && selectedTower != null)
        {
            isSucceeded = towerSpawner.TryUpgradeTower(selectedTower);
        }

        if (isSucceeded)
        {
            Hide();
        }
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

    private void UpdateGoldCostText()
    {
        if (goldCostText != null && towerSpawner != null)
        {
            goldCostText.text = towerSpawner.TowerBuildGold.ToString();
        }
    }

    private void OnDestroy()
    {
        if (summonButton != null)
        {
            summonButton.onClick.RemoveListener(OnClickAction);
        }
    }
}
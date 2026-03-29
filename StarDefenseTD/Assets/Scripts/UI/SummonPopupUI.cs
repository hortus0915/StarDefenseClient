using UnityEngine;
using UnityEngine.UI;

public class SummonPopupUI : MonoBehaviour
{
    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private Button summonButton;
    [SerializeField] private TowerSpawner towerSpawner;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Vector3 popupWorldOffset = new Vector3(0.0f, 0.6f, 0.0f);

    private Transform selectedTile;

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

        ResolveTowerSpawner();

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (summonButton != null)
        {
            summonButton.onClick.AddListener(OnClickSummon);
        }

        Hide();
    }

    private void LateUpdate()
    {
        if (IsVisible == false || selectedTile == null)
        {
            return;
        }

        UpdatePopupPosition();
    }

    public bool CanShowOn(Transform tileTransform)
    {
        ResolveTowerSpawner();
        return towerSpawner != null && towerSpawner.CanSpawnTower(tileTransform);
    }

    public void Show(Transform tileTransform)
    {
        if (popupRoot == null || tileTransform == null)
        {
            return;
        }

        ResolveTowerSpawner();

        selectedTile = tileTransform;
        popupRoot.gameObject.SetActive(true);
        UpdatePopupPosition();
    }

    public void Hide()
    {
        selectedTile = null;

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
        if (selectedTile == null || popupRoot == null)
        {
            return;
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        Vector3 worldPosition = selectedTile.position + popupWorldOffset;
        Vector3 screenPosition = worldCamera != null ? worldCamera.WorldToScreenPoint(worldPosition) : worldPosition;
        popupRoot.position = screenPosition;
    }

    private void OnClickSummon()
    {
        if (selectedTile == null || towerSpawner == null)
        {
            return;
        }

        bool isSpawned = towerSpawner.SpawnTower(selectedTile);
        if (isSpawned)
        {
            Hide();
        }
    }

    private void OnDestroy()
    {
        if (summonButton != null)
        {
            summonButton.onClick.RemoveListener(OnClickSummon);
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectDetector : MonoBehaviour
{
    [SerializeField] private SummonPopupUI summonPopupUI;

    private Camera mainCamera;
    private Ray ray;

    private void Awake()
    {
        mainCamera = Camera.main;
        ResolveSummonPopupUI();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) == false)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return;
        }

        ResolveSummonPopupUI();
        ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        TowerWeapon clickedTower = null;
        Transform clickedTile = null;

        for (int i = 0; i < hits.Length; i++)
        {
            TowerWeapon towerWeapon = hits[i].transform.GetComponentInParent<TowerWeapon>();
            if (towerWeapon != null)
            {
                clickedTower = towerWeapon;
                break;
            }

            if (clickedTile == null && hits[i].transform.CompareTag("Tile"))
            {
                clickedTile = hits[i].transform;
            }
        }

        if (clickedTower != null)
        {
            if (summonPopupUI != null && summonPopupUI.CanShowTowerActionsOn(clickedTower))
            {
                summonPopupUI.ShowTowerActions(clickedTower);
            }
            else
            {
                HideSummonPopup();
            }

            return;
        }

        if (clickedTile == null)
        {
            HideSummonPopup();
            return;
        }

        Tile tile = clickedTile.GetComponent<Tile>();
        if (tile == null)
        {
            HideSummonPopup();
            return;
        }

        if (tile.RequiresRepair)
        {
            if (summonPopupUI != null && summonPopupUI.CanShowRepairOn(tile))
            {
                summonPopupUI.ShowRepair(tile);
            }
            else
            {
                HideSummonPopup();
            }

            return;
        }

        if (tile.IsBuuldTower)
        {
            HideSummonPopup();
            return;
        }

        if (summonPopupUI == null || summonPopupUI.CanShowOn(clickedTile) == false)
        {
            HideSummonPopup();
            return;
        }

        summonPopupUI.ShowSummon(clickedTile);
    }

    private void ResolveSummonPopupUI()
    {
        if (IsSceneInstance(summonPopupUI))
        {
            return;
        }

        summonPopupUI = FindFirstObjectByType<SummonPopupUI>(FindObjectsInactive.Include);
    }

    private bool IsSceneInstance(SummonPopupUI popupUI)
    {
        return popupUI != null && popupUI.gameObject.scene.IsValid() && popupUI.gameObject.scene.isLoaded;
    }

    private void HideSummonPopup()
    {
        if (summonPopupUI != null)
        {
            summonPopupUI.Hide();
        }
    }
}

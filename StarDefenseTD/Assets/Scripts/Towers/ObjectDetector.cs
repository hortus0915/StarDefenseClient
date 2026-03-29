using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectDetector : MonoBehaviour
{
    [SerializeField] private SummonPopupUI summonPopupUI;

    private Camera mainCamera;
    private Ray ray;
    private RaycastHit hit;

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

        if (Physics.Raycast(ray, out hit) == false)
        {
            HideSummonPopup();
            return;
        }

        if (hit.transform.CompareTag("Tile") == false)
        {
            HideSummonPopup();
            return;
        }

        Tile tile = hit.transform.GetComponent<Tile>();
        if (tile == null || tile.IsBuuldTower)
        {
            HideSummonPopup();
            return;
        }

        if (summonPopupUI == null || summonPopupUI.CanShowOn(hit.transform) == false)
        {
            HideSummonPopup();
            return;
        }

        summonPopupUI.Show(hit.transform);
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

using UnityEngine;
using UnityEngine.UI;

public class PlayerHPViewer : MonoBehaviour
{
    [SerializeField] private PlayerHP playerHP;
    [SerializeField] private Slider hpSlider;

    private void Awake()
    {
        ResolveReferences();
        InitializeSlider();
        UpdateSlider();
    }

    private void OnEnable()
    {
        ResolveReferences();
        InitializeSlider();
        UpdateSlider();
    }

    private void Update()
    {
        UpdateSlider();
    }

    private void OnValidate()
    {
        ResolveReferences();
        InitializeSlider();
        UpdateSlider();
    }

    private void ResolveReferences()
    {
        if (playerHP == null)
        {
            playerHP = FindFirstObjectByType<PlayerHP>(FindObjectsInactive.Include);
        }

        if (hpSlider == null)
        {
            hpSlider = GetComponentInChildren<Slider>();
        }
    }

    private void InitializeSlider()
    {
        if (hpSlider == null)
        {
            return;
        }

        hpSlider.minValue = 0.0f;
        hpSlider.maxValue = 1.0f;
        hpSlider.wholeNumbers = false;
        hpSlider.interactable = false;
    }

    private void UpdateSlider()
    {
        if (playerHP == null || hpSlider == null)
        {
            return;
        }

        float hpRatio = playerHP.MaxHP > 0.0f ? playerHP.CurrentHP / playerHP.MaxHP : 0.0f;
        hpSlider.value = Mathf.Clamp01(hpRatio);
    }
}

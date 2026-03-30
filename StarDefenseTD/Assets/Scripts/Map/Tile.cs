using UnityEngine;
using UnityEngine.Serialization;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool requiresRepair;
    [FormerlySerializedAs("repairGold")]
    [SerializeField] private int repairMineral = 20;
    [SerializeField] private SpriteRenderer tileRenderer;
    [SerializeField] private Sprite defaultTileSprite;
    [SerializeField] private Sprite lockedTileSprite;

    public bool IsBuuldTower { get; set; }
    public TowerWeapon CurrentTower { get; private set; }
    public bool RequiresRepair => requiresRepair;
    public int RepairMineral => repairMineral;
    public bool CanBuildTower => requiresRepair == false && IsBuuldTower == false;

    private void Awake()
    {
        if (tileRenderer == null)
        {
            tileRenderer = GetComponent<SpriteRenderer>();
        }

        IsBuuldTower = CurrentTower != null;
        ApplyTileSprite();
    }

    public void SetTower(TowerWeapon tower)
    {
        CurrentTower = tower;
        IsBuuldTower = tower != null;
    }

    public void ClearTower(TowerWeapon tower)
    {
        if (CurrentTower != tower)
        {
            return;
        }

        CurrentTower = null;
        IsBuuldTower = false;
    }

    public bool CanRepair()
    {
        return requiresRepair;
    }

    public bool TryRepair()
    {
        if (requiresRepair == false)
        {
            return false;
        }

        requiresRepair = false;
        ApplyTileSprite();
        return true;
    }

    private void OnValidate()
    {
        if (tileRenderer == null)
        {
            tileRenderer = GetComponent<SpriteRenderer>();
        }

        ApplyTileSprite();
    }

    private void ApplyTileSprite()
    {
        if (tileRenderer == null)
        {
            return;
        }

        if (requiresRepair)
        {
            if (lockedTileSprite != null)
            {
                tileRenderer.sprite = lockedTileSprite;
            }

            return;
        }

        if (defaultTileSprite != null)
        {
            tileRenderer.sprite = defaultTileSprite;
        }
    }
}

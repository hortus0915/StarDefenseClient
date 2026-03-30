using UnityEngine;

public class TowerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private int normalSpawnChance = 70;
    [SerializeField] private int rareSpawnChance = 20;
    [SerializeField] private int epicSpawnChance = 10;
    [SerializeField] private TowerData[] normalTowerDatas;
    [SerializeField] private TowerData[] rareTowerDatas;
    [SerializeField] private TowerData[] epicTowerDatas;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private int towerBuildGold = 20;
    [SerializeField] private PlayerGold playerGold;
    [SerializeField] private PlayerMineral playerMineral;
    [SerializeField] private float towerSpawnYOffset = 0.1f;
    [SerializeField] private Vector3 towerColliderCenter = new Vector3(0.0f, 0.0f, 0.0f);
    [SerializeField] private Vector3 towerColliderSize = new Vector3(1.2f, 1.2f, 0.2f);

    public int TowerBuildGold => towerBuildGold;

    private void Start()
    {
        RefreshUpgradeableTowers();
    }

    public bool CanSpawnTower(Transform tileTransform)
    {
        if (tileTransform == null || towerPrefab == null)
        {
            return false;
        }

        Tile tile = tileTransform.GetComponent<Tile>();
        if (tile == null || tile.CanBuildTower == false)
        {
            return false;
        }

        return true;
    }

    public bool CanShowRepairTile(Tile tile)
    {
        return tile != null && tile.CanRepair();
    }

    public bool CanRepairTile(Tile tile)
    {
        return tile != null && tile.CanRepair();
    }

    public bool TryRepairTile(Tile tile)
    {
        if (CanRepairTile(tile) == false || playerMineral == null)
        {
            return false;
        }

        if (tile.RepairMineral > playerMineral.CurrentMineral)
        {
            return false;
        }

        playerMineral.CurrentMineral -= tile.RepairMineral;
        return tile.TryRepair();
    }

    public bool CanUpgradeTower(TowerWeapon selectedTower)
    {
        if (selectedTower == null)
        {
            return false;
        }

        if (GetNextTowerGrade(selectedTower.TowerGrade).HasValue == false)
        {
            return false;
        }

        return FindUpgradePartner(selectedTower) != null;
    }

    public bool CanChangeTower(TowerWeapon selectedTower)
    {
        if (selectedTower == null)
        {
            return false;
        }

        return GetRandomTowerData(selectedTower.TowerGrade) != null;
    }

    public bool SpawnTower(Transform tileTransform)
    {
        if (CanSpawnTower(tileTransform) == false || playerGold == null)
        {
            return false;
        }

        if (towerBuildGold > playerGold.CurrnetGold)
        {
            return false;
        }

        Tile tile = tileTransform.GetComponent<Tile>();
        tile.IsBuuldTower = true;
        playerGold.CurrnetGold -= towerBuildGold;

        Vector3 spawnPosition = tileTransform.position + Vector3.up * towerSpawnYOffset;
        GameObject clone = Instantiate(towerPrefab, spawnPosition, Quaternion.identity);
        EnsureTowerCollider(clone);

        TowerWeapon towerWeapon = clone.GetComponent<TowerWeapon>();
        if (towerWeapon != null)
        {
            TowerData selectedTowerData = ResolveSpawnTowerData();
            if (selectedTowerData != null)
            {
                towerWeapon.SetTowerData(selectedTowerData);
            }

            towerWeapon.AssignTile(tile);
            towerWeapon.Setup(enemySpawner);
        }

        RefreshUpgradeableTowers();
        return true;
    }

    public bool TryUpgradeTower(TowerWeapon selectedTower)
    {
        if (CanUpgradeTower(selectedTower) == false)
        {
            return false;
        }

        TowerGrade? nextGrade = GetNextTowerGrade(selectedTower.TowerGrade);
        if (nextGrade.HasValue == false)
        {
            return false;
        }

        TowerWeapon partnerTower = FindUpgradePartner(selectedTower);
        if (partnerTower == null)
        {
            return false;
        }

        TowerData upgradedTowerData = GetRandomTowerData(nextGrade.Value);
        if (upgradedTowerData == null)
        {
            return false;
        }

        selectedTower.SetTowerData(upgradedTowerData);
        partnerTower.RemoveFromBoard();

        RefreshUpgradeableTowers();
        return true;
    }

    public bool TryChangeTower(TowerWeapon selectedTower)
    {
        if (CanChangeTower(selectedTower) == false)
        {
            return false;
        }

        TowerData changedTowerData = GetRandomTowerData(selectedTower.TowerGrade);
        if (changedTowerData == null)
        {
            return false;
        }

        selectedTower.SetTowerData(changedTowerData);
        RefreshUpgradeableTowers();
        return true;
    }

    public void RefreshUpgradeableTowers()
    {
        var towers = TowerWeapon.ActiveTowers;
        for (int i = 0; i < towers.Count; i++)
        {
            TowerWeapon tower = towers[i];
            if (tower == null)
            {
                continue;
            }

            tower.SetUpgradeAvailable(CanUpgradeTower(tower));
        }
    }

    private TowerData ResolveSpawnTowerData()
    {
        int totalChance = Mathf.Max(0, normalSpawnChance) + Mathf.Max(0, rareSpawnChance) + Mathf.Max(0, epicSpawnChance);
        if (totalChance <= 0)
        {
            return GetFallbackTowerData();
        }

        int randomValue = Random.Range(0, totalChance);
        if (randomValue < normalSpawnChance)
        {
            TowerData normalTowerData = GetRandomTowerData(normalTowerDatas);
            if (normalTowerData != null)
            {
                return normalTowerData;
            }
        }
        else if (randomValue < normalSpawnChance + rareSpawnChance)
        {
            TowerData rareTowerData = GetRandomTowerData(rareTowerDatas);
            if (rareTowerData != null)
            {
                return rareTowerData;
            }
        }
        else
        {
            TowerData epicTowerData = GetRandomTowerData(epicTowerDatas);
            if (epicTowerData != null)
            {
                return epicTowerData;
            }
        }

        return GetFallbackTowerData();
    }

    private TowerWeapon FindUpgradePartner(TowerWeapon selectedTower)
    {
        var towers = TowerWeapon.ActiveTowers;
        for (int i = 0; i < towers.Count; i++)
        {
            TowerWeapon tower = towers[i];
            if (tower == null || tower == selectedTower)
            {
                continue;
            }

            if (tower.TowerType != selectedTower.TowerType)
            {
                continue;
            }

            if (tower.TowerGrade != selectedTower.TowerGrade)
            {
                continue;
            }

            return tower;
        }

        return null;
    }

    private TowerGrade? GetNextTowerGrade(TowerGrade currentGrade)
    {
        if (currentGrade == TowerGrade.Normal)
        {
            return TowerGrade.Rare;
        }

        if (currentGrade == TowerGrade.Rare)
        {
            return TowerGrade.Epic;
        }

        return null;
    }

    private TowerData GetRandomTowerData(TowerGrade towerGrade)
    {
        switch (towerGrade)
        {
            case TowerGrade.Normal:
                return GetRandomTowerData(normalTowerDatas);
            case TowerGrade.Rare:
                return GetRandomTowerData(rareTowerDatas);
            case TowerGrade.Epic:
                return GetRandomTowerData(epicTowerDatas);
            default:
                return null;
        }
    }

    private TowerData GetRandomTowerData(TowerData[] towerDatas)
    {
        if (towerDatas == null || towerDatas.Length == 0)
        {
            return null;
        }

        int startIndex = Random.Range(0, towerDatas.Length);
        for (int i = 0; i < towerDatas.Length; i++)
        {
            int index = (startIndex + i) % towerDatas.Length;
            if (towerDatas[index] != null)
            {
                return towerDatas[index];
            }
        }

        return null;
    }

    private TowerData GetRandomTowerData(TowerData[] towerDatas, TowerType excludedTowerType)
    {
        if (towerDatas == null || towerDatas.Length == 0)
        {
            return null;
        }

        int validCount = 0;
        for (int i = 0; i < towerDatas.Length; i++)
        {
            if (towerDatas[i] != null && towerDatas[i].TowerType != excludedTowerType)
            {
                validCount++;
            }
        }

        if (validCount == 0)
        {
            return null;
        }

        int pickIndex = Random.Range(0, validCount);
        for (int i = 0; i < towerDatas.Length; i++)
        {
            if (towerDatas[i] == null || towerDatas[i].TowerType == excludedTowerType)
            {
                continue;
            }

            if (pickIndex == 0)
            {
                return towerDatas[i];
            }

            pickIndex--;
        }

        return null;
    }

    private TowerData GetFallbackTowerData()
    {
        TowerData towerData = GetRandomTowerData(normalTowerDatas);
        if (towerData != null)
        {
            return towerData;
        }

        towerData = GetRandomTowerData(rareTowerDatas);
        if (towerData != null)
        {
            return towerData;
        }

        return GetRandomTowerData(epicTowerDatas);
    }

    private void EnsureTowerCollider(GameObject towerObject)
    {
        if (towerObject == null)
        {
            return;
        }

        BoxCollider boxCollider = towerObject.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = towerObject.AddComponent<BoxCollider>();
        }

        boxCollider.center = towerColliderCenter;
        boxCollider.size = towerColliderSize;
        boxCollider.isTrigger = false;
    }
}

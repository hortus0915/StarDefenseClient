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
    [SerializeField] private float towerSpawnYOffset = 0.1f;

    public bool CanSpawnTower(Transform tileTransform)
    {
        if (tileTransform == null || towerPrefab == null || playerGold == null)
        {
            return false;
        }

        if (towerBuildGold > playerGold.CurrnetGold)
        {
            return false;
        }

        Tile tile = tileTransform.GetComponent<Tile>();
        if (tile == null || tile.IsBuuldTower)
        {
            return false;
        }

        return true;
    }

    public bool SpawnTower(Transform tileTransform)
    {
        if (CanSpawnTower(tileTransform) == false)
        {
            return false;
        }

        Tile tile = tileTransform.GetComponent<Tile>();
        tile.IsBuuldTower = true;
        playerGold.CurrnetGold -= towerBuildGold;

        Vector3 spawnPosition = tileTransform.position + Vector3.up * towerSpawnYOffset;
        GameObject clone = Instantiate(towerPrefab, spawnPosition, Quaternion.identity);
        TowerWeapon towerWeapon = clone.GetComponent<TowerWeapon>();
        if (towerWeapon != null)
        {
            TowerData selectedTowerData = ResolveSpawnTowerData();
            if (selectedTowerData != null)
            {
                towerWeapon.SetTowerData(selectedTowerData);
            }

            towerWeapon.Setup(enemySpawner);
        }

        return true;
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
}
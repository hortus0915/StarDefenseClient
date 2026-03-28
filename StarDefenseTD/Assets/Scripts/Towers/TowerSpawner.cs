using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSpawner : MonoBehaviour
{
    [SerializeField]private GameObject towerPrefab;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField]private int towerBuildGold = 20;
    [SerializeField] private PlayerGold playerGold;

    public void SpawnTower(Transform tileTransform)
    {
        if(towerBuildGold > playerGold.CurrnetGold)
        {
        return;    
        }

        Tile  tile = tileTransform.GetComponent<Tile>();

        if(tile.IsBuuldTower)
        {
            return;
        }

        tile.IsBuuldTower = true;
        playerGold.CurrnetGold -= towerBuildGold;
        
        GameObject clone = Instantiate(towerPrefab, tileTransform.position, Quaternion.identity);
        TowerWeapon towerWeapon = clone.GetComponent<TowerWeapon>();
        towerWeapon.Setup(enemySpawner);
    }
}

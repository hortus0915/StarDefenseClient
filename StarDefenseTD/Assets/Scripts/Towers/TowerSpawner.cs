using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSpawner : MonoBehaviour
{
    [SerializeField]private GameObject towerPrefab;
    [SerializeField] private EnemySpawner enemySpawner;

    public void SpawnTower(Transform tileTransform)
    {
        Tile  tile = tileTransform.GetComponent<Tile>();

        if(tile.IsBuuldTower)
        {
            return;
        }

        tile.IsBuuldTower = true;
        
        GameObject clone = Instantiate(towerPrefab, tileTransform.position, Quaternion.identity);
        TowerWeapon towerWeapon = clone.GetComponent<TowerWeapon>();
        towerWeapon.Setup(enemySpawner);
    }
}

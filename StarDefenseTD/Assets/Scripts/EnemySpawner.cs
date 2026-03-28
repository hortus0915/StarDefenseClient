using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField]private Transform[] wayPoints;
    [SerializeField] private float SpawnTime =0.5f;

    private void Awake()
    {
        StartCoroutine("SpawnEnemy");
    }

    private IEnumerator SpawnEnemy()
    {
        while(true)
        {
            GameObject clone = Instantiate(enemyPrefab);
            Enemy enemy = clone.GetComponent<Enemy>();
            
            enemy.Setup(wayPoints);

            yield return new WaitForSeconds(SpawnTime);
        }
    }

}

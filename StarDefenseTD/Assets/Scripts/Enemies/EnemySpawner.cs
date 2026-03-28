using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    //[SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject enemyHPSliderPrefab;
    [SerializeField] private Transform canvasTransform;
    [SerializeField]private Transform[] wayPoints;
    [SerializeField] private PlayerHP playerHP;
    //[SerializeField] private float SpawnTime =0.5f;
    [SerializeField] private PlayerGold playerGold;

    private List<Enemy> enemyList;
    private int activeSpawnRoutineCount;

    public List<Enemy> EnemyList => enemyList;
    public bool IsSpawning => activeSpawnRoutineCount > 0;

    private void Awake()
    {
        enemyList = new List<Enemy>();
    }

    public void StartWave(Wave wave)
    {
        StartCoroutine(SpawnEnemy(wave));
    }

    public void StopAllSpawn()
    {
        StopAllCoroutines();
        activeSpawnRoutineCount = 0;
    }

    private IEnumerator SpawnEnemy(Wave wave)
    {
        activeSpawnRoutineCount++;

        int spawnEnemyCount = 0;

        while (spawnEnemyCount < wave.maxEnemyCount)
        {
            int enemyIndex = UnityEngine.Random.Range(0, wave.enemyPrefabs.Length);
            GameObject clone = Instantiate(wave.enemyPrefabs[enemyIndex]);
            Enemy enemy = clone.GetComponent<Enemy>();

            enemy.Setup(this, wayPoints);
            enemyList.Add(enemy);

            SpawnEnemyHPSlider(clone);
            spawnEnemyCount++;

            yield return new WaitForSeconds(wave.spawnTime);
        }

        activeSpawnRoutineCount--;
    }

    private void SpawnEnemyHPSlider(GameObject clone)
    {
        GameObject sliderClone = Instantiate(enemyHPSliderPrefab);
        sliderClone.transform.SetParent(canvasTransform);
        sliderClone.transform.localScale = Vector3.one;

        sliderClone.GetComponent<SliderPositionAutoSetter>().Setup(clone.transform);
        sliderClone.GetComponent<EnemyHPViewer>().Setup(clone.GetComponent<EnemyHP>());
    }

    public void DestroyEnemy(EnemyDestroyType destroyType, Enemy enemy, int gold)
    {
        if (destroyType == EnemyDestroyType.Arrive)
        {
            playerHP.TakeDamage(1);
        }
        else if (destroyType == EnemyDestroyType.Kill)
        {
            playerGold.CurrnetGold += gold;
        }

        if (enemyList.Contains(enemy))
        {
            enemyList.Remove(enemy);
            Destroy(enemy.gameObject);
        }
    }
}

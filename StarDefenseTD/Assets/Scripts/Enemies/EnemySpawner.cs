using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyHPSliderPrefab;
    [SerializeField] private Transform enemyHPSliderPoolRoot;
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private PlayerHP playerHP;
    [SerializeField] private PlayerGold playerGold;

    private List<Enemy> enemyList;
    private Dictionary<Enemy, GameObject> enemySliderMap;
    private int activeSpawnRoutineCount;

    public List<Enemy> EnemyList => enemyList;
    public bool IsSpawning => activeSpawnRoutineCount > 0;

    private void Awake()
    {
        enemyList = new List<Enemy>();
        enemySliderMap = new Dictionary<Enemy, GameObject>();
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
        Vector3 spawnPosition = wayPoints != null && wayPoints.Length > 0 ? wayPoints[0].position : transform.position;

        while (spawnEnemyCount < wave.maxEnemyCount)
        {
            int enemyIndex = UnityEngine.Random.Range(0, wave.enemyPrefabs.Length);
            GameObject clone = ObjectPoolManager.Instance.GetObject(wave.enemyPrefabs[enemyIndex], spawnPosition, Quaternion.identity);
            Enemy enemy = clone != null ? clone.GetComponent<Enemy>() : null;

            if (enemy != null)
            {
                enemy.ApplyWaveStats(wave.enemyGold, wave.enemyAttackDamage);
                enemy.Setup(this, wayPoints);
                enemyList.Add(enemy);
                SpawnEnemyHPSlider(enemy);
            }

            spawnEnemyCount++;
            yield return new WaitForSeconds(wave.spawnTime);
        }

        activeSpawnRoutineCount--;
    }

    private void SpawnEnemyHPSlider(Enemy enemy)
    {
        if (enemy == null || enemyHPSliderPrefab == null || enemyHPSliderPoolRoot == null)
        {
            return;
        }

        GameObject sliderClone = ObjectPoolManager.Instance.GetObject(enemyHPSliderPrefab, Vector3.zero, Quaternion.identity, enemyHPSliderPoolRoot);
        sliderClone.transform.localScale = Vector3.one;

        SliderPositionAutoSetter sliderPositionAutoSetter = sliderClone.GetComponent<SliderPositionAutoSetter>();
        if (sliderPositionAutoSetter != null)
        {
            sliderPositionAutoSetter.Setup(enemy.transform);
        }

        EnemyHPViewer enemyHPViewer = sliderClone.GetComponent<EnemyHPViewer>();
        if (enemyHPViewer != null)
        {
            enemyHPViewer.Setup(enemy.GetComponent<EnemyHP>());
        }

        enemySliderMap[enemy] = sliderClone;
    }

    public void DestroyEnemy(EnemyDestroyType destroyType, Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (destroyType == EnemyDestroyType.Arrive)
        {
            if (playerHP != null)
            {
                playerHP.TakeDamage(enemy.AttackDamage);
            }
        }
        else if (destroyType == EnemyDestroyType.Kill)
        {
            if (playerGold != null)
            {
                playerGold.CurrnetGold += enemy.GoldReward;
            }
        }

        enemyList.Remove(enemy);

        if (enemySliderMap.TryGetValue(enemy, out GameObject sliderObject))
        {
            enemySliderMap.Remove(enemy);
            if (sliderObject != null)
            {
                ObjectPoolManager.Instance.ReturnObject(sliderObject);
            }
        }

        ObjectPoolManager.Instance.ReturnObject(enemy.gameObject);
    }
}

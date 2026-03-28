using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject enemyHPSliderPrefab;
    [SerializeField] private Transform canvasTransform;
    [SerializeField]private Transform[] wayPoints;
    [SerializeField] private PlayerHP playerHP;
    [SerializeField] private float SpawnTime =0.5f;
    [SerializeField] private PlayerGold playerGold;

    private List<Enemy> enemyList;
    public List<Enemy> EnemyList => enemyList;

    private void Awake()
    {
        enemyList = new List<Enemy>();
        StartCoroutine("SpawnEnemy");
    }

    private IEnumerator SpawnEnemy()
    {
        while(true)
        {
            GameObject clone = Instantiate(enemyPrefab);
            Enemy enemy = clone.GetComponent<Enemy>();
            
            enemy.Setup(this,wayPoints);
            enemyList.Add(enemy);
            SpawnEnemyHPSlider(clone);

            yield return new WaitForSeconds(SpawnTime);
        }
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
        if(destroyType == EnemyDestroyType.Arrive)
        {
            playerHP.TakeDamage(1);
        }
        else if (destroyType == EnemyDestroyType.Kill)
        {
            playerGold.CurrnetGold += gold;
        }
        
        if(enemyList.Contains(enemy))
        {
            enemyList.Remove(enemy);
            Destroy(enemy.gameObject);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyDestroyType { Kill = 0, Arrive }
public class Enemy : MonoBehaviour
{
    private int wayPointCount;
    private Transform[] wayPoints;
    private int currentIndex = 0;
    private Movement2D movement2D;
    private EnemySpawner enemySpawner;

    [SerializeField] private int gold = 10;
    [SerializeField] private int attackDamage = 1;

    public int GoldReward => gold;
    public int AttackDamage => attackDamage;

    public void Setup(EnemySpawner enemySpawner, Transform[] wayPoints)
    {
        this.enemySpawner = enemySpawner;
        movement2D = GetComponent<Movement2D>();

        wayPointCount = wayPoints.Length;
        this.wayPoints = new Transform[wayPointCount];
        this.wayPoints = wayPoints;

        transform.position = wayPoints[currentIndex].position;

        StartCoroutine("OnMove");
    }

    public void ApplyWaveStats(int goldReward, int waveAttackDamage)
    {
        gold = Mathf.Max(0, goldReward);
        attackDamage = Mathf.Max(0, waveAttackDamage);
    }

    private IEnumerator OnMove()
    {
        NextMoveTo();

        while (true)
        {
            transform.Rotate(Vector3.forward * 10);

            if (Vector3.Distance(transform.position, wayPoints[currentIndex].position) < 0.02f * movement2D.MoveSpeed)
            {
                NextMoveTo();
            }

            yield return null;
        }
    }

    private void NextMoveTo()
    {
        if (currentIndex < wayPointCount - 1)
        {
            transform.position = wayPoints[currentIndex].position;

            currentIndex++;
            Vector3 direction = (wayPoints[currentIndex].position - transform.position).normalized;
            movement2D.MoveTo(direction);
        }
        else
        {
            OnDie(EnemyDestroyType.Arrive);
        }
    }

    public void OnDie(EnemyDestroyType destroyType)
    {
        enemySpawner.DestroyEnemy(destroyType, this);
    }
}

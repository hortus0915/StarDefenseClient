using System.Collections;
using UnityEngine;

public enum EnemyDestroyType { Kill = 0, Arrive }

public class Enemy : MonoBehaviour
{
    private int wayPointCount;
    private Transform[] wayPoints;
    private int currentIndex;
    private Movement2D movement2D;
    private EnemySpawner enemySpawner;

    [SerializeField] private int gold = 10;
    [SerializeField] private int attackDamage = 1;

    public int GoldReward => gold;
    public int AttackDamage => attackDamage;

    private void Awake()
    {
        movement2D = GetComponent<Movement2D>();
    }

    public void Setup(EnemySpawner enemySpawner, Transform[] wayPoints)
    {
        this.enemySpawner = enemySpawner;

        if (movement2D == null)
        {
            movement2D = GetComponent<Movement2D>();
        }

        this.wayPoints = wayPoints;
        wayPointCount = wayPoints != null ? wayPoints.Length : 0;
        currentIndex = 0;
        transform.rotation = Quaternion.identity;

        if (wayPointCount == 0)
        {
            return;
        }

        transform.position = wayPoints[currentIndex].position;
        movement2D.MoveTo(Vector3.zero);

        StopAllCoroutines();
        StartCoroutine(OnMove());
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
            transform.Rotate(Vector3.forward * 10.0f);

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
        if (enemySpawner == null)
        {
            return;
        }

        enemySpawner.DestroyEnemy(destroyType, this);
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (movement2D != null)
        {
            movement2D.MoveTo(Vector3.zero);
        }
    }
}

using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3.0f;

    private Movement2D movement2D;
    private Transform target;
    private int damage;
    private Vector3 moveDirection;
    private bool followTarget;

    private void Awake()
    {
        movement2D = GetComponent<Movement2D>();
    }

    public void Setup(Transform target, int damage)
    {
        Setup(target, damage, Vector3.zero, true);
    }

    public void Setup(Transform target, int damage, Vector3 direction, bool followTarget)
    {
        if (movement2D == null)
        {
            movement2D = GetComponent<Movement2D>();
        }

        this.target = target;
        this.damage = damage;
        this.followTarget = followTarget;
        moveDirection = direction.normalized;

        if (moveDirection == Vector3.zero && target != null)
        {
            moveDirection = (target.position - transform.position).normalized;
        }

        StopAllCoroutines();
        StartCoroutine(ReturnAfterLifeTime());
    }

    private void Update()
    {
        if (followTarget)
        {
            if (target == null)
            {
                ReturnToPool();
                return;
            }

            moveDirection = (target.position - transform.position).normalized;
        }

        if (moveDirection != Vector3.zero && movement2D != null)
        {
            movement2D.MoveTo(moveDirection);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") == false)
        {
            return;
        }

        if (followTarget && target != null && collision.transform != target)
        {
            return;
        }

        EnemyHP enemyHP = collision.GetComponent<EnemyHP>();
        if (enemyHP == null)
        {
            return;
        }

        enemyHP.TakeDamage(damage);
        ReturnToPool();
    }

    private IEnumerator ReturnAfterLifeTime()
    {
        yield return new WaitForSeconds(lifeTime);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (gameObject.activeInHierarchy == false)
        {
            return;
        }

        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        target = null;
        damage = 0;
        followTarget = false;
        moveDirection = Vector3.zero;

        if (movement2D != null)
        {
            movement2D.MoveTo(Vector3.zero);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Movement2D movement2D;
    private Transform target;
    private int damage;
    private Vector3 moveDirection;
    private bool followTarget;

    public void Setup(Transform target, int damage)
    {
        Setup(target, damage, Vector3.zero, true);
    }

    public void Setup(Transform target, int damage, Vector3 direction, bool followTarget)
    {
        movement2D = GetComponent<Movement2D>();
        this.target = target;
        this.damage = damage;
        this.followTarget = followTarget;
        moveDirection = direction.normalized;

        if (moveDirection == Vector3.zero && target != null)
        {
            moveDirection = (target.position - transform.position).normalized;
        }
    }

    private void Update()
    {
        if (followTarget)
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            moveDirection = (target.position - transform.position).normalized;
        }

        if (moveDirection != Vector3.zero)
        {
            movement2D.MoveTo(moveDirection);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;
        if (followTarget && target != null && collision.transform != target) return;

        EnemyHP enemyHP = collision.GetComponent<EnemyHP>();
        if (enemyHP == null) return;

        enemyHP.TakeDamage(damage);
        Destroy(gameObject);
    }
}

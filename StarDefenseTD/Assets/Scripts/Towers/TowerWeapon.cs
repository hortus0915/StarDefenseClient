using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponState {SearchTarget =0, AttackToTarget}
public class TowerWeapon : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float attackRate = 0.5f;
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private int attackDamage = 1;

    private WeaponState weaponState = WeaponState.SearchTarget;
    private Transform attackTarget = null;
    private EnemySpawner enemySpawner;

    public void Setup(EnemySpawner enemySpawner)
    {
        this.enemySpawner = enemySpawner;
        ChangeState(WeaponState.SearchTarget);
    }

    public void ChangeState(WeaponState newState)
    {
        StopCoroutine(weaponState.ToString());
        weaponState = newState;
        StartCoroutine(weaponState.ToString());
    }

    private void Update()
    {
        if(attackTarget != null)
        {
            RoteteToTarget();
        }
    }

    private void RoteteToTarget()
    {
        float dx = attackTarget.position.x - transform.position.x;
        float dy = attackTarget.position.y - transform.position.y;

        float degree = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, degree);
    }

    private IEnumerator SearchTarget()
    {
        while(true)
        {
           float closeDistSqr = Mathf.Infinity;

           for(int i = 0 ;  i< enemySpawner.EnemyList.Count; i++)
            {
                float distance = Vector3.Distance(enemySpawner.EnemyList[i].transform.position, transform.position);

               if(distance <= attackRange && distance < closeDistSqr)
                {
                    closeDistSqr = distance;
                    attackTarget = enemySpawner.EnemyList[i].transform;
                }
          
            }

            if(attackTarget != null)
            {
                ChangeState(WeaponState.AttackToTarget);
            }

            yield return null;
        }
    }

    private IEnumerator AttackToTarget()
    {
        while(true)
        {
           if(attackTarget == null)
            {
                ChangeState(WeaponState.SearchTarget);
                break;
            }

            float distnace = Vector3.Distance(attackTarget.position,transform.position);
            if(distnace> attackRange)
            {
                attackTarget = null;
                ChangeState(WeaponState.SearchTarget);
                break;
            }

            yield return new WaitForSeconds(attackRate);

            SpawnProjectile();
        }
    }

    private void SpawnProjectile()
    {
        GameObject clone =  Instantiate(projectilePrefab, spawnPoint.position, transform.rotation);
        clone.GetComponent<Projectile>().Setup(attackTarget,attackDamage);
    }
}

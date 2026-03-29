using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponState { SearchTarget = 0, AttackToTarget }
public class TowerWeapon : MonoBehaviour
{
    private static readonly float[] DefaultShotAngles = new float[] { 0.0f };
    private static readonly List<TowerWeapon> activeTowers = new List<TowerWeapon>();

    [Header("Tower Data")]
    [SerializeField] private TowerData towerData;
    [SerializeField] private Transform headTransform;
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private SpriteRenderer upgradeIndicatorRenderer;

    [Header("Fallback")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float attackRate = 0.5f;
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private TowerAttackMode attackMode = TowerAttackMode.Projectile;
    [SerializeField] private int maxTargetCount = 1;
    [SerializeField] private float[] shotAngles = new float[] { 0.0f };
    [SerializeField] private float projectileLaneOffset = 0.2f;

    private WeaponState weaponState = WeaponState.SearchTarget;
    private Transform attackTarget = null;
    private EnemySpawner enemySpawner;
    private Tile placedTile;

    public static IReadOnlyList<TowerWeapon> ActiveTowers => activeTowers;
    public TowerData TowerData => towerData;
    public TowerType TowerType => towerData != null ? towerData.TowerType : TowerType.White;
    public TowerGrade TowerGrade => towerData != null ? towerData.TowerGrade : TowerGrade.Normal;
    public Tile PlacedTile => placedTile;

    private GameObject CurrentProjectilePrefab => towerData != null && towerData.ProjectilePrefab != null ? towerData.ProjectilePrefab : projectilePrefab;
    private float CurrentAttackRate => towerData != null ? towerData.AttackRate : attackRate;
    private float CurrentAttackRange => towerData != null ? towerData.AttackRange : attackRange;
    private int CurrentAttackDamage => towerData != null ? towerData.AttackDamage : attackDamage;
    private TowerAttackMode CurrentAttackMode => towerData != null ? towerData.AttackMode : attackMode;
    private int CurrentMaxTargetCount => towerData != null ? towerData.MaxTargetCount : maxTargetCount;
    private float[] CurrentShotAngles
    {
        get
        {
            if (towerData != null && towerData.ShotAngles != null && towerData.ShotAngles.Length > 0)
            {
                return towerData.ShotAngles;
            }

            if (shotAngles != null && shotAngles.Length > 0)
            {
                return shotAngles;
            }

            return DefaultShotAngles;
        }
    }

    private void Awake()
    {
        if (headRenderer == null && headTransform != null)
        {
            headRenderer = headTransform.GetComponent<SpriteRenderer>();
        }

        if (upgradeIndicatorRenderer != null)
        {
            upgradeIndicatorRenderer.gameObject.SetActive(false);
        }

        ApplyTowerData();
    }

    private void OnEnable()
    {
        if (activeTowers.Contains(this) == false)
        {
            activeTowers.Add(this);
        }
    }

    private void OnDisable()
    {
        activeTowers.Remove(this);
    }

    public void Setup(EnemySpawner enemySpawner)
    {
        this.enemySpawner = enemySpawner;
        ApplyTowerData();
        ChangeState(WeaponState.SearchTarget);
    }

    public void SetTowerData(TowerData newTowerData)
    {
        towerData = newTowerData;
        ApplyTowerData();
    }

    public void AssignTile(Tile tile)
    {
        placedTile = tile;
        if (placedTile != null)
        {
            placedTile.SetTower(this);
        }
    }

    public void SetUpgradeAvailable(bool isAvailable)
    {
        if (upgradeIndicatorRenderer != null)
        {
            upgradeIndicatorRenderer.gameObject.SetActive(isAvailable);
        }
       
    }

    public void RemoveFromBoard()
    {
        if (placedTile != null)
        {
            placedTile.ClearTower(this);
            placedTile = null;
        }

        SetUpgradeAvailable(false);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public void ChangeState(WeaponState newState)
    {
        StopCoroutine(weaponState.ToString());
        weaponState = newState;
        StartCoroutine(weaponState.ToString());
    }

    private void Update()
    {
        if (attackTarget != null)
        {
            RotateHeadToTarget();
        }
    }

    private void ApplyTowerData()
    {
        if (towerData == null || headRenderer == null || towerData.TowerSprite == null)
        {
            return;
        }

        headRenderer.sprite = towerData.TowerSprite;
    }

    private void RotateHeadToTarget()
    {
        Transform rotationTarget = headTransform != null ? headTransform : transform;

        float dx = attackTarget.position.x - transform.position.x;
        float dy = attackTarget.position.y - transform.position.y;

        float degree = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        rotationTarget.rotation = Quaternion.Euler(0, 0, degree);
    }

    private IEnumerator SearchTarget()
    {
        while (true)
        {
            attackTarget = FindNearestTargetInRange();

            if (attackTarget != null)
            {
                ChangeState(WeaponState.AttackToTarget);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator AttackToTarget()
    {
        while (true)
        {
            attackTarget = FindNearestTargetInRange();
            if (attackTarget == null)
            {
                ChangeState(WeaponState.SearchTarget);
                yield break;
            }

            yield return new WaitForSeconds(CurrentAttackRate);
            PerformAttack();
        }
    }

    private Transform FindNearestTargetInRange()
    {
        if (enemySpawner == null)
        {
            return null;
        }

        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        for (int i = 0; i < enemySpawner.EnemyList.Count; i++)
        {
            Enemy enemy = enemySpawner.EnemyList[i];
            if (enemy == null)
            {
                continue;
            }

            float distance = Vector3.Distance(enemy.transform.position, transform.position);
            if (distance <= CurrentAttackRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = enemy.transform;
            }
        }

        return closestTarget;
    }

    private List<Enemy> GetTargetsInRange()
    {
        List<Enemy> targets = new List<Enemy>();
        if (enemySpawner == null)
        {
            return targets;
        }

        for (int i = 0; i < enemySpawner.EnemyList.Count; i++)
        {
            Enemy enemy = enemySpawner.EnemyList[i];
            if (enemy == null)
            {
                continue;
            }

            float distance = Vector3.Distance(enemy.transform.position, transform.position);
            if (distance <= CurrentAttackRange)
            {
                targets.Add(enemy);
            }
        }

        targets.Sort((left, right) =>
        {
            float leftDistance = Vector3.Distance(left.transform.position, transform.position);
            float rightDistance = Vector3.Distance(right.transform.position, transform.position);
            return leftDistance.CompareTo(rightDistance);
        });

        return targets;
    }

    private void PerformAttack()
    {
        if (CurrentAttackMode == TowerAttackMode.Direct)
        {
            AttackDirect();
            return;
        }

        AttackProjectile();
    }

    private void AttackProjectile()
    {
        if (attackTarget == null || spawnPoint == null)
        {
            return;
        }

        GameObject currentProjectilePrefab = CurrentProjectilePrefab;
        if (currentProjectilePrefab == null)
        {
            return;
        }

        int shotCount = GetProjectileShotCount();
        Vector3 baseDirection = spawnPoint.right.normalized;
        if (baseDirection == Vector3.zero)
        {
            baseDirection = (attackTarget.position - transform.position).normalized;
        }

        float degree = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        for (int i = 0; i < shotCount; i++)
        {
            float laneOffset = GetProjectileLaneOffset(i, shotCount);
            Vector3 spawnPosition = spawnPoint.position + spawnPoint.up * laneOffset;

            GameObject clone = ObjectPoolManager.Instance.GetObject(currentProjectilePrefab, spawnPosition, Quaternion.Euler(0, 0, degree));
            Projectile projectile = clone.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Setup(attackTarget, CurrentAttackDamage, baseDirection, false);
            }
        }
    }

    private int GetProjectileShotCount()
    {
        return CurrentShotAngles != null && CurrentShotAngles.Length > 0 ? CurrentShotAngles.Length : 1;
    }

    private float GetProjectileLaneOffset(int shotIndex, int shotCount)
    {
        if (shotCount <= 1)
        {
            return 0.0f;
        }

        if (shotCount == 2)
        {
            return shotIndex == 0 ? projectileLaneOffset : -projectileLaneOffset;
        }

        if (shotCount == 3)
        {
            if (shotIndex == 0) return projectileLaneOffset;
            if (shotIndex == 1) return 0.0f;
            return -projectileLaneOffset;
        }

        float t = shotCount <= 1 ? 0.5f : (float)shotIndex / (shotCount - 1);
        return Mathf.Lerp(projectileLaneOffset, -projectileLaneOffset, t);
    }

    private void AttackDirect()
    {
        List<Enemy> targets = GetTargetsInRange();
        if (targets.Count == 0)
        {
            return;
        }

        int attackCount = CurrentMaxTargetCount <= 0 ? targets.Count : Mathf.Min(CurrentMaxTargetCount, targets.Count);
        for (int i = 0; i < attackCount; i++)
        {
            EnemyHP enemyHP = targets[i].GetComponent<EnemyHP>();
            if (enemyHP != null)
            {
                enemyHP.TakeDamage(CurrentAttackDamage);
            }
        }
    }
}

using UnityEngine;

public enum TowerType
{
    White = 0,
    Red = 1,
    Blue = 2,
}

public enum TowerGrade
{
    Normal = 0,
    Rare = 1,
    Epic = 2,
}

public enum TowerAttackMode
{
    Projectile = 0,
    Direct = 1,
}

[CreateAssetMenu(fileName = "TowerData_", menuName = "StarDefenseTD/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private TowerType towerType;
    [SerializeField] private TowerGrade towerGrade;

    [Header("Visual")]
    [SerializeField] private Sprite towerSprite;

    [Header("Attack")]
    [SerializeField] private TowerAttackMode attackMode = TowerAttackMode.Projectile;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float attackRate = 0.5f;
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private int maxTargetCount = 1;
    [SerializeField] private float[] shotAngles = new float[] { 0.0f };

    public TowerType TowerType => towerType;
    public TowerGrade TowerGrade => towerGrade;
    public Sprite TowerSprite => towerSprite;
    public TowerAttackMode AttackMode => attackMode;
    public GameObject ProjectilePrefab => projectilePrefab;
    public float AttackRate => attackRate;
    public float AttackRange => attackRange;
    public int AttackDamage => attackDamage;
    public int MaxTargetCount => maxTargetCount;
    public float[] ShotAngles => shotAngles;
}

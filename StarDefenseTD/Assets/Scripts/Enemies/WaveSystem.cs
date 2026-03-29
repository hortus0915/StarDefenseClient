using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{
    [SerializeField] private Wave[] waves;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayerHP playerHP;
    [SerializeField] private bool autoStartWave = true;
    [SerializeField] private float firstWaveDelay = 5.0f;
    [SerializeField] private float waveInterval = 10.0f;

    private int currentWaveIndex = -1;
    private Coroutine autoWaveRoutine;
    private bool isGameFinished;

    public int CurrnetWaveIndex => currentWaveIndex;
    public int CurrentWaveNumber => Mathf.Clamp(currentWaveIndex + 1, 0, waves.Length);
    public int MaxWave => waves.Length;
    public bool IsGameFinished => isGameFinished;

    private void Awake()
    {
        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
        }

        if (playerHP == null)
        {
            playerHP = FindObjectOfType<PlayerHP>();
        }
    }

    private void Start()
    {
        if (autoStartWave)
        {
            autoWaveRoutine = StartCoroutine(AutoStartWave());
        }
    }

    private void Update()
    {
        if (isGameFinished)
        {
            return;
        }

        if (playerHP != null && playerHP.CurrentHP <= 0)
        {
            GameLose();
            return;
        }

        if (enemySpawner == null)
        {
            return;
        }

        bool isLastWaveStarted = currentWaveIndex >= waves.Length - 1;
        if (isLastWaveStarted && enemySpawner.IsSpawning == false && enemySpawner.EnemyList.Count == 0)
        {
            GameWin();
        }
    }

    public void StartWave()
    {
        if (isGameFinished || enemySpawner == null || currentWaveIndex >= waves.Length - 1)
        {
            return;
        }

        currentWaveIndex++;
        enemySpawner.StartWave(waves[currentWaveIndex]);
    }

    public void GameLose()
    {
        if (isGameFinished)
        {
            return;
        }

        isGameFinished = true;
        StopAutoWaveRoutine();

        if (enemySpawner != null)
        {
            enemySpawner.StopAllSpawn();
        }

        Debug.Log("Game Lose");
        // TODO: 게임 패배 UI 연결
    }

    public void GameWin()
    {
        if (isGameFinished)
        {
            return;
        }

        isGameFinished = true;
        StopAutoWaveRoutine();

        if (enemySpawner != null)
        {
            enemySpawner.StopAllSpawn();
        }

        Debug.Log("Game Win");
        // TODO: 게임 승리 UI 연결
    }

    private IEnumerator AutoStartWave()
    {
        yield return new WaitForSeconds(firstWaveDelay);

        while (isGameFinished == false && currentWaveIndex < waves.Length - 1)
        {
            StartWave();

            if (currentWaveIndex >= waves.Length - 1)
            {
                break;
            }

            yield return new WaitForSeconds(waveInterval);
        }
    }

    private void StopAutoWaveRoutine()
    {
        if (autoWaveRoutine != null)
        {
            StopCoroutine(autoWaveRoutine);
            autoWaveRoutine = null;
        }
    }

    private void OnDisable()
    {
        StopAutoWaveRoutine();
    }
}

[System.Serializable]
public struct Wave
{
    public float spawnTime;
    public int maxEnemyCount;
    public int enemyGold;
    public int enemyAttackDamage;
    public GameObject[] enemyPrefabs;
}


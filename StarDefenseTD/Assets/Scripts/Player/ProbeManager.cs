using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProbeManager : MonoBehaviour
{
    private enum ProbeSide
    {
        Left = 0,
        Right = 1,
    }

    [SerializeField] private GameObject probePrefab;
    [SerializeField] private Transform probeParent;
    [SerializeField] private Transform nexusTransform;
    [SerializeField] private Transform leftNexusDock;
    [SerializeField] private Transform rightNexusDock;
    [SerializeField] private Transform leftMineralTarget;
    [SerializeField] private Transform rightMineralTarget;
    [SerializeField] private PlayerGold playerGold;
    [SerializeField] private PlayerMineral playerMineral;
    [SerializeField] private TMP_Text spawnGoldText;
    [SerializeField] private TMP_Text probeCountText;
    [SerializeField] private int initialSpawnGold = 60;
    [SerializeField] private int spawnGoldIncrease = 5;
    [SerializeField] private int maxProbeCount = 20;

    private readonly List<ProbeWorker> activeProbes = new List<ProbeWorker>();
    private int purchasedProbeCount;
    private ProbeSide nextSpawnSide = ProbeSide.Left;

    public int CurrentSpawnGold => initialSpawnGold + (purchasedProbeCount * spawnGoldIncrease);
    public int CurrentProbeCount => activeProbes.Count;
    public int MaxProbeCount => maxProbeCount;

    private void Awake()
    {
        ResolveReferences();
        RegisterExistingProbes();
        ConfigureExistingProbes();
        nextSpawnSide = activeProbes.Count % 2 == 1 ? ProbeSide.Left : ProbeSide.Right;
        RefreshUITexts();
    }

    private void OnValidate()
    {
        RefreshUITexts();
    }

    public bool CanSpawnProbe()
    {
        RemoveNullProbes();

        if (probePrefab == null || playerGold == null)
        {
            return false;
        }

        if (activeProbes.Count >= maxProbeCount)
        {
            return false;
        }

        return playerGold.CurrnetGold >= CurrentSpawnGold;
    }

    public bool TrySpawnProbe()
    {
        ResolveReferences();
        RemoveNullProbes();

        if (probePrefab == null || playerGold == null)
        {
            return false;
        }

        if (activeProbes.Count >= maxProbeCount)
        {
            return false;
        }

        int spawnGold = CurrentSpawnGold;
        if (playerGold.CurrnetGold < spawnGold)
        {
            return false;
        }

        Transform spawnDock = GetDockTransform(nextSpawnSide);
        Transform mineralTarget = GetMineralTransform(nextSpawnSide);
        if (spawnDock == null || mineralTarget == null)
        {
            return false;
        }

        GameObject clone = Instantiate(probePrefab, spawnDock.position, Quaternion.identity, probeParent);
        ProbeWorker probeWorker = clone.GetComponent<ProbeWorker>();
        if (probeWorker == null)
        {
            Destroy(clone);
            return false;
        }

        ConfigureProbe(probeWorker, nextSpawnSide);
        activeProbes.Add(probeWorker);
        playerGold.CurrnetGold -= spawnGold;
        purchasedProbeCount++;
        nextSpawnSide = nextSpawnSide == ProbeSide.Left ? ProbeSide.Right : ProbeSide.Left;
        RefreshUITexts();
        return true;
    }

    public void OnClickSpawnProbe()
    {
        TrySpawnProbe();
    }

    public void RefreshUITexts()
    {
        RemoveNullProbes();
        RefreshSpawnGoldText();
        RefreshProbeCountText();
    }

    public void RefreshSpawnGoldText()
    {
        if (spawnGoldText == null)
        {
            return;
        }

        spawnGoldText.text = CurrentSpawnGold.ToString();
    }

    public void RefreshProbeCountText()
    {
        if (probeCountText == null)
        {
            return;
        }

        probeCountText.text = $"{CurrentProbeCount}/{MaxProbeCount}";
    }

    private void ResolveReferences()
    {
        if (playerGold == null)
        {
            playerGold = FindFirstObjectByType<PlayerGold>(FindObjectsInactive.Include);
        }

        if (playerMineral == null)
        {
            playerMineral = FindFirstObjectByType<PlayerMineral>(FindObjectsInactive.Include);
        }
    }

    private void RegisterExistingProbes()
    {
        activeProbes.Clear();
        ProbeWorker[] probes = FindObjectsByType<ProbeWorker>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < probes.Length; i++)
        {
            ProbeWorker probe = probes[i];
            if (probe == null)
            {
                continue;
            }

            activeProbes.Add(probe);
        }
    }

    private void ConfigureExistingProbes()
    {
        for (int i = 0; i < activeProbes.Count; i++)
        {
            ProbeWorker probe = activeProbes[i];
            if (probe == null)
            {
                continue;
            }

            ProbeSide probeSide = DetermineProbeSide(probe.transform.position.x);
            ConfigureProbe(probe, probeSide);
        }
    }

    private void ConfigureProbe(ProbeWorker probeWorker, ProbeSide probeSide)
    {
        if (probeWorker == null)
        {
            return;
        }

        Transform mineralTarget = GetMineralTransform(probeSide);
        probeWorker.ConfigureRoute(nexusTransform, leftNexusDock, rightNexusDock, mineralTarget, playerMineral);
    }

    private ProbeSide DetermineProbeSide(float probeX)
    {
        if (nexusTransform == null)
        {
            return ProbeSide.Right;
        }

        return probeX < nexusTransform.position.x ? ProbeSide.Left : ProbeSide.Right;
    }

    private Transform GetDockTransform(ProbeSide probeSide)
    {
        if (probeSide == ProbeSide.Left)
        {
            return leftNexusDock != null ? leftNexusDock : nexusTransform;
        }

        return rightNexusDock != null ? rightNexusDock : nexusTransform;
    }

    private Transform GetMineralTransform(ProbeSide probeSide)
    {
        if (probeSide == ProbeSide.Left)
        {
            return leftMineralTarget;
        }

        return rightMineralTarget;
    }

    private void RemoveNullProbes()
    {
        for (int i = activeProbes.Count - 1; i >= 0; i--)
        {
            if (activeProbes[i] == null)
            {
                activeProbes.RemoveAt(i);
            }
        }
    }
}

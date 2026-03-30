using UnityEngine;

public class ProbeWorker : MonoBehaviour
{
    private enum ProbeState
    {
        ToMineral = 0,
        ToNexus = 1,
    }

    [SerializeField] private Transform nexusTransform;
    [SerializeField] private Transform leftNexusDock;
    [SerializeField] private Transform rightNexusDock;
    [SerializeField] private Transform[] mineralTargets;
    [SerializeField] private PlayerMineral playerMineral;
    [SerializeField] private SpriteRenderer carriedMineralRenderer;
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float arriveDistance = 0.05f;

    private ProbeState currentState;
    private Transform assignedMineralTarget;
    private Transform assignedNexusDock;
    private Transform currentTarget;

    private void Awake()
    {
        InitializeRoute();
    }

    private void OnEnable()
    {
        InitializeRoute();
    }

    private void Update()
    {
        if (currentTarget == null)
        {
            return;
        }

        MoveHorizontallyToCurrentTarget();

        if (HasArrivedOnXAxis(currentTarget) == false)
        {
            return;
        }

        if (currentState == ProbeState.ToMineral)
        {
            currentState = ProbeState.ToNexus;
            currentTarget = assignedNexusDock;
            SetCarriedMineralVisible(true);
            return;
        }

        if (currentState == ProbeState.ToNexus)
        {
            if (playerMineral != null)
            {
                playerMineral.CurrentMineral += 1;
            }

            currentState = ProbeState.ToMineral;
            currentTarget = assignedMineralTarget;
            SetCarriedMineralVisible(false);
        }
    }

    public void ConfigureRoute(Transform nexus, Transform leftDock, Transform rightDock, Transform mineralTarget, PlayerMineral mineralCounter)
    {
        nexusTransform = nexus;
        leftNexusDock = leftDock;
        rightNexusDock = rightDock;
        mineralTargets = mineralTarget != null ? new[] { mineralTarget } : null;
        playerMineral = mineralCounter;
        InitializeRoute();
    }

    private void InitializeRoute()
    {
        assignedMineralTarget = FindNearestMineralTarget();
        assignedNexusDock = FindAssignedNexusDock(assignedMineralTarget);
        currentState = ProbeState.ToMineral;
        currentTarget = assignedMineralTarget;
        SetCarriedMineralVisible(false);
    }

    private Transform FindNearestMineralTarget()
    {
        if (mineralTargets == null || mineralTargets.Length == 0)
        {
            return null;
        }

        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        for (int i = 0; i < mineralTargets.Length; i++)
        {
            Transform mineralTarget = mineralTargets[i];
            if (mineralTarget == null)
            {
                continue;
            }

            float distance = Mathf.Abs(mineralTarget.position.x - transform.position.x);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = mineralTarget;
            }
        }

        return closestTarget;
    }

    private Transform FindAssignedNexusDock(Transform mineralTarget)
    {
        if (mineralTarget == null)
        {
            return nexusTransform;
        }

        if (nexusTransform == null)
        {
            return null;
        }

        bool isLeftMineral = mineralTarget.position.x < nexusTransform.position.x;
        if (isLeftMineral)
        {
            return leftNexusDock != null ? leftNexusDock : nexusTransform;
        }

        return rightNexusDock != null ? rightNexusDock : nexusTransform;
    }

    private void MoveHorizontallyToCurrentTarget()
    {
        Vector3 nextPosition = transform.position;
        nextPosition.x = Mathf.MoveTowards(transform.position.x, currentTarget.position.x, moveSpeed * Time.deltaTime);
        transform.position = nextPosition;
    }

    private bool HasArrivedOnXAxis(Transform target)
    {
        return Mathf.Abs(transform.position.x - target.position.x) <= arriveDistance;
    }

    private void SetCarriedMineralVisible(bool isVisible)
    {
        if (carriedMineralRenderer != null)
        {
            carriedMineralRenderer.enabled = isVisible;
        }
    }
}

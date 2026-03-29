using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private Transform enemyPoolRoot;
    [SerializeField] private Transform projectilePoolRoot;

    private static ObjectPoolManager instance;

    private readonly Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> prefabLookup = new Dictionary<GameObject, GameObject>();
    private readonly Dictionary<GameObject, Transform> customPoolRootLookup = new Dictionary<GameObject, Transform>();

    public static ObjectPoolManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ObjectPoolManager>(FindObjectsInactive.Include);
                if (instance == null)
                {
                    GameObject poolManagerObject = new GameObject("ObjectPoolManager");
                    instance = poolManagerObject.AddComponent<ObjectPoolManager>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        EnsurePoolRoots();
    }

    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform poolParentOverride = null)
    {
        if (prefab == null)
        {
            return null;
        }

        EnsurePoolRoots();

        if (poolParentOverride != null)
        {
            customPoolRootLookup[prefab] = poolParentOverride;
        }

        Queue<GameObject> poolQueue = GetPoolQueue(prefab);
        GameObject pooledObject = null;

        while (poolQueue.Count > 0 && pooledObject == null)
        {
            pooledObject = poolQueue.Dequeue();
        }

        Transform poolParent = GetPoolParent(prefab);

        if (pooledObject == null)
        {
            pooledObject = Instantiate(prefab, poolParent);
            prefabLookup[pooledObject] = prefab;
        }

        pooledObject.transform.SetParent(poolParent, false);
        pooledObject.transform.SetPositionAndRotation(position, rotation);
        pooledObject.SetActive(true);
        return pooledObject;
    }

    public void ReturnObject(GameObject pooledObject)
    {
        if (pooledObject == null)
        {
            return;
        }

        if (prefabLookup.TryGetValue(pooledObject, out GameObject prefab) == false || prefab == null)
        {
            Destroy(pooledObject);
            return;
        }

        EnsurePoolRoots();

        pooledObject.SetActive(false);
        pooledObject.transform.SetParent(GetPoolParent(prefab), false);
        GetPoolQueue(prefab).Enqueue(pooledObject);
    }

    public Transform GetOrCreatePoolRoot(string rootName, Transform parent)
    {
        if (parent == null)
        {
            return transform;
        }

        Transform child = parent.Find(rootName);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(rootName);
        child = childObject.transform;
        child.SetParent(parent, false);
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
        return child;
    }

    private Queue<GameObject> GetPoolQueue(GameObject prefab)
    {
        if (poolDictionary.TryGetValue(prefab, out Queue<GameObject> poolQueue) == false)
        {
            poolQueue = new Queue<GameObject>();
            poolDictionary[prefab] = poolQueue;
        }

        return poolQueue;
    }

    private Transform GetPoolParent(GameObject prefab)
    {
        if (prefab == null)
        {
            return transform;
        }

        if (customPoolRootLookup.TryGetValue(prefab, out Transform customRoot) && customRoot != null)
        {
            return customRoot;
        }

        if (prefab.GetComponent<Enemy>() != null)
        {
            return enemyPoolRoot != null ? enemyPoolRoot : transform;
        }

        if (prefab.GetComponent<Projectile>() != null)
        {
            return projectilePoolRoot != null ? projectilePoolRoot : transform;
        }

        return transform;
    }

    private void EnsurePoolRoots()
    {
        if (enemyPoolRoot == null)
        {
            enemyPoolRoot = GetOrCreateChild("EnemyPool");
        }

        if (projectilePoolRoot == null)
        {
            projectilePoolRoot = GetOrCreateChild("ProjectilePool");
        }
    }

    private Transform GetOrCreateChild(string childName)
    {
        Transform child = transform.Find(childName);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(childName);
        child = childObject.transform;
        child.SetParent(transform, false);
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
        return child;
    }
}

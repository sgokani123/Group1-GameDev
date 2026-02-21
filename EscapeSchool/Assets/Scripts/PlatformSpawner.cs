using UnityEngine;
using System.Collections.Generic;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Platform Prefab")]
    public GameObject platformPrefab;

    [Header("Spawn Settings")]
    public float minX           = -2.5f;
    public float maxX           =  2.5f;
    public float minYGap        =  1.0f;   // Minimum vertical gap
    public float maxYGap        =  1.6f;   // Maximum vertical gap — keep reachable
    public int   initialCount   =  12;
    public float spawnLookAhead =  8f;

    [Header("Difficulty")]
    public float difficultyScaleEvery = 30f; // Units climbed before gaps increase

    [Header("Pooling")]
    public int prewarmPoolCount = 30;
    public Transform pooledParent;
    private ObjectPool pool;

    private float highestSpawnedY;
    private List<GameObject> activePlatforms = new List<GameObject>();
    private Camera mainCam;
    private bool lastWasUnreliable = false; // Tracks if last platform was broken/disposable

    void Start()
    {
        mainCam = Camera.main;

        // Initialize pooling
        pool = gameObject.AddComponent<ObjectPool>();
        pool.Init(platformPrefab, prewarmPoolCount, pooledParent);

        SpawnInitialPlatforms();
    }

    void Update()
    {
        // Try to recover mainCam if it's null (handles rare scene reload cases)
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        float camTopY    = mainCam.transform.position.y + mainCam.orthographicSize;
        float camBottomY = mainCam.transform.position.y - mainCam.orthographicSize;

        while (highestSpawnedY < camTopY + spawnLookAhead)
        {
            float gap = CalculateGap(highestSpawnedY);
            SpawnPlatform(highestSpawnedY + gap);
        }

        activePlatforms.RemoveAll(p =>
        {
            if (p != null && p.transform.position.y < camBottomY - 3f)
            {
                pool.Return(p);
                return true;
            }
            return p == null;
        });
    }

    float CalculateGap(float height)
    {
        // Increase gap gradually with height, but cap it so platforms stay reachable
        float scale = Mathf.Floor(height / difficultyScaleEvery) * 0.1f;
        return Mathf.Clamp(Random.Range(minYGap, maxYGap) + scale, minYGap, maxYGap);
    }

    void SpawnInitialPlatforms(float startY = -0.5f)
    {
        // Always place a safe platform directly under the player
        GameObject first = pool.Get();
        first.transform.position = new Vector3(0f, startY - 0.3f, 0f);
        first.transform.rotation = Quaternion.identity;
        SetTileType(first, 0);
        activePlatforms.Add(first);
        highestSpawnedY = startY - 0.3f;

        for (int i = 0; i < initialCount; i++)
        {
            float gap = Random.Range(minYGap, maxYGap);
            SpawnPlatform(highestSpawnedY + gap);
        }
    }

    void SpawnPlatform(float y)
    {
        int type = GetRandomTileType(y);

        // If last platform was unreliable (broken/disposable), force a normal one now
        if (lastWasUnreliable)
        {
            type = 0;
            lastWasUnreliable = false;
        }
        else
        {
            lastWasUnreliable = (type == 1 || type == 2);
        }

        // Limit X spread based on gap size so platforms are always reachable
        float gap = y - highestSpawnedY;
        float xRange = Mathf.Lerp(maxX, maxX * 0.5f, gap / maxYGap);
        float x = Random.Range(-xRange, xRange);

        GameObject p = pool.Get();
        p.transform.position = new Vector3(x, y, 0f);
        p.transform.rotation = Quaternion.identity;
        SetTileType(p, type);
        activePlatforms.Add(p);
        highestSpawnedY = y;
    }

    void SetTileType(GameObject p, int type)
    {
        Tile tile = p.GetComponent<Tile>();
        if (tile != null) tile.ApplyType(type);
    }

    int GetRandomTileType(float height)
    {
        float difficulty = Mathf.Clamp01(height / 150f);
        int r = Random.Range(0, 100);

        int normalChance = (int)Mathf.Lerp(75, 50, difficulty);

        if (r < normalChance)       return 0; // Normal
        if (r < normalChance + 10)  return 4; // Horizontal moving
        if (r < normalChance + 18)  return 1; // Broken
        if (r < normalChance + 23)  return 2; // Disposable
        if (r < normalChance + 27)  return 5; // Vertical moving
        return 3;                             // Spring
    }

    public void ResetSpawner(float startY = -0.5f)
    {
        if (pool == null)
        {
            pool = gameObject.AddComponent<ObjectPool>();
            pool.Init(platformPrefab, prewarmPoolCount, pooledParent);
        }

        foreach (var p in activePlatforms)
            if (p != null) pool.Return(p);
        activePlatforms.Clear();
        highestSpawnedY = startY - 0.5f;
        lastWasUnreliable = false;
        SpawnInitialPlatforms(startY);
    }
}

using UnityEngine;
using System.Collections.Generic;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Platform Prefab")]
    public GameObject platformPrefab;

    // enemy create
    [Header("Monster Spawn Settings")]
    public GameObject enemyPrefab;      // Inspector enemy
    [Range(0, 1)]
    public float enemySpawnChance = 0.15f; //probabillity

    [Header("Rocket Spawn Settings")]
    public GameObject rocketPrefab;      // дк Inspector rocket
    [Range(0, 1)]
    public float rocketSpawnChance = 0.05f; // probabillity


    [Header("Spawn Settings")]
    public float minX = -2.5f;
    public float maxX =  2.5f;
    public float minYGap = 1.0f;
    public float maxYGap = 1.6f;
    public int initialCount = 12;
    public float spawnLookAhead = 8f;

    [Header("Difficulty")]
    public float difficultyHeight = 300f;   // height where difficulty ~ max
    public float extraGapAtMax = 0.9f;      // extra vertical gap at max difficulty
    public float maxTotalGap = 2.3f;        // hard cap for reachability

    [Header("Horizontal Difficulty")]
    public float extraXSpreadAtMax = 1.2f;  // how much wider we allow (in world units) at max
    public float farJumpChanceAtMax = 0.55f;// chance to bias platforms toward edges at max
    public float farJumpEdgeMin = 0.70f;    // edge bias strength (0.7 => between 70%-100% of xRange)

    [Header("Pooling")]
    public int prewarmPoolCount = 30;
    public Transform pooledParent;
    private ObjectPool pool;

    private float highestSpawnedY;
    private float lastX;
    private readonly List<GameObject> activePlatforms = new List<GameObject>();
    private Camera mainCam;
    private bool lastWasUnreliable = false;

    void Start()
    {
        mainCam = Camera.main;

        pool = gameObject.AddComponent<ObjectPool>();
        pool.Init(platformPrefab, prewarmPoolCount, pooledParent);

        SpawnInitialPlatforms();
    }

    void Update()
    {
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
              
                ClearObjectsOnPlatform(p);
                pool.Return(p);
                return true;
            }
            return p == null;
        });
    }

    float Difficulty01(float height)
    {
        return Mathf.Clamp01(height / Mathf.Max(1f, difficultyHeight));
    }

    float CalculateGap(float height)
    {
        float d = Difficulty01(height);

        float baseGap = Random.Range(minYGap, maxYGap);
        float extra = Mathf.Lerp(0f, extraGapAtMax, d);

        float gap = baseGap + extra;
        return Mathf.Clamp(gap, minYGap, maxTotalGap);
    }

    float CurrentXRange(float height)
    {
        float d = Difficulty01(height);

        // Wider horizontal range over time
        float baseRange = maxX;
        float extraRange = Mathf.Lerp(0f, extraXSpreadAtMax, d);
        float range = baseRange + extraRange;

        // Never exceed camera bounds (safe clamp)
        float camHalfWidth = mainCam != null ? mainCam.orthographicSize * mainCam.aspect : range;
        return Mathf.Min(range, camHalfWidth - 0.2f);
    }

    //delete enemy and rocket
    void ClearObjectsOnPlatform(GameObject platform)
    {
       
        if (platform == null) return;

       
        for (int i = platform.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = platform.transform.GetChild(i);

            
            if (child != null && child.gameObject != null)
            {
                if (child.CompareTag("Enemy") || child.name.ToLower().Contains("rocket"))
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    void SpawnInitialPlatforms(float startY = -0.5f)
    {
        GameObject first = pool.Get();
        first.transform.position = new Vector3(0f, startY - 0.3f, 0f);
        first.transform.rotation = Quaternion.identity;

        SetTileType(first, 0, 0f); // safe start
        activePlatforms.Add(first);

        highestSpawnedY = startY - 0.3f;
        lastX = 0f;

        for (int i = 0; i < initialCount; i++)
        {
            float gap = Random.Range(minYGap, maxYGap);
            SpawnPlatform(highestSpawnedY + gap);
        }
    }

    void SpawnPlatform(float y)
    {
        // 1. Determine the difficulty factor (0 to 1) based on height
        float d = Difficulty01(y);
        
        // 2. Determine Tile Type based on probability weights
        int type = GetRandomTileType(y, d);

        // 3. Prevent "Impossible" gaps: 
        // If the last platform was broken (1) or disposable (2), force this one to be solid (0).
        if (lastWasUnreliable)
        {
            type = 0; 
            lastWasUnreliable = false;
        }
        else
        {
            lastWasUnreliable = (type == 1 || type == 2);
        }

        // 4. Calculate Horizontal Range (clamped by camera bounds)
        float xRange = CurrentXRange(y);

        // 5. Calculate X Position
        // Start with a completely random value within the allowed range
        float x = Random.Range(-xRange, xRange);

        // Later in the game: occasionally force platforms toward the edges
        // This creates those wide, satisfying lateral jumps.
        float farChance = Mathf.Lerp(0.10f, farJumpChanceAtMax, d);
        if (Random.value < farChance)
        {
            float edgeMin = Mathf.Clamp01(farJumpEdgeMin);
            float edgeX = Random.Range(edgeMin * xRange, xRange);
            x = (Random.value < 0.5f ? -edgeX : edgeX);
        }

        // 6. Apply "Repel" logic
        // As difficulty rises, bias the platform AWAY from the last platform's X
        // This prevents "ladders" where the player just holds still to climb.
        float repel = Mathf.Lerp(0.0f, 0.35f, d);
        float diff = x - lastX;
        // If they are exactly the same, pick a random direction to push
        float side = (diff == 0) ? (Random.value > 0.5f ? 1 : -1) : Mathf.Sign(diff);
        
        x = Mathf.Lerp(x, x + side * xRange * 0.25f, repel);
        
        // Safety clamp to ensure the repel didn't push it off-screen
        x = Mathf.Clamp(x, -xRange, xRange);

        // 7. Object Pooling: Get the platform and set it up
        GameObject p = pool.Get();
        p.transform.position = new Vector3(x, y, 0f);
        p.transform.rotation = Quaternion.identity;

        // 8. Initialize the Tile component
        SetTileType(p, type, d);

        //decide enmey create
        if ((type == 0 || type == 4 || type == 5) && Random.value < enemySpawnChance)
        {
            SpawnEnemy(p);
        }

        // rocket create on 0paltform
        if (type == 0 && Random.value < rocketSpawnChance)
        {
            GameObject rocket = Instantiate(rocketPrefab);
           
            rocket.transform.position = p.transform.position + new Vector3(0, 0.7f, 0);

            rocket.transform.SetParent(p.transform);
        }


        // 9. Tracking for the next spawn
        activePlatforms.Add(p);
        highestSpawnedY = y;
        lastX = x;
    }

    void SpawnRocket(GameObject platform)
    {
        if (rocketPrefab == null) return;

        // create rocket
        Vector3 spawnPos = platform.transform.position + new Vector3(0, 0.7f, 0);
        GameObject rocket = Instantiate(rocketPrefab, spawnPos, Quaternion.identity);

   
        rocket.transform.SetParent(platform.transform);
    }

    //enemy create
    void SpawnEnemy(GameObject platform)
    {

        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy Prefab is missing on PlatformSpawner!");
            return;
        }

        Vector3 enemyPos = platform.transform.position + new Vector3(0, 0.6f, 0);
        GameObject enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
        enemy.transform.SetParent(platform.transform);
    }

    void SetTileType(GameObject p, int type, float difficulty01)
    {
        Tile tile = p.GetComponent<Tile>();
        if (tile != null) tile.ApplyType(type, difficulty01);
    }

    int GetRandomTileType(float height, float d)
    {
        // Early: mostly normal. Later: more moving + broken/disposable.
        int normalW     = Mathf.RoundToInt(Mathf.Lerp(78, 40, d));
        int movingW     = Mathf.RoundToInt(Mathf.Lerp(10, 26, d)); // split later
        int brokenW     = Mathf.RoundToInt(Mathf.Lerp(6,  18, d));
        int disposableW = Mathf.RoundToInt(Mathf.Lerp(4,  16, d));
        int springW     = Mathf.RoundToInt(Mathf.Lerp(2,   6, d));

        int total = normalW + movingW + brokenW + disposableW + springW;
        int r = Random.Range(0, total);

        if (r < normalW) return 0;

        r -= normalW;
        if (r < movingW)
        {
            // later: more vertical movers (harder)
            return (Random.value < Mathf.Lerp(0.7f, 0.45f, d)) ? 4 : 5;
        }

        r -= movingW;
        if (r < brokenW) return 1;

        r -= brokenW;
        if (r < disposableW) return 2;

        return 3;
    }

    public void ResetSpawner(float startY = -0.5f)
    {
        if (pool == null)
        {
            pool = gameObject.AddComponent<ObjectPool>();
            pool.Init(platformPrefab, prewarmPoolCount, pooledParent);
        }

        foreach (var p in activePlatforms)
        {
            if (p != null)
            {
                //reset enemy
                ClearObjectsOnPlatform(p);

                pool.Return(p);
            }
        }

        activePlatforms.Clear();
        highestSpawnedY = startY - 0.5f;
        lastWasUnreliable = false;
        lastX = 0f;

        SpawnInitialPlatforms(startY);
    }
}
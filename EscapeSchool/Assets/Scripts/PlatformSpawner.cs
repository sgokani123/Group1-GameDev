using UnityEngine;
using System.Collections.Generic;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Platform Prefab")]
    public GameObject platformPrefab;

    // enemy create
    [Header("Monster Spawn Settings")]
    public GameObject enemyPrefab;      // Inspector enemy

    [Header("Gap Enemies (floating in mid-air)")]
    [Tooltip("Scale applied to every enemy instance (0.5 = half size).")]
    public float enemyScale = 0.5f;
    [Tooltip("Gap enemies start appearing at this Y height.")]
    public float gapEnemyMinHeight = 8f;
    [Tooltip("Maximum spawn chance for a gap enemy per platform gap (scaled by difficulty).")]
    [Range(0, 1)]
    public float gapEnemyMaxChance = 0.35f;
    [Tooltip("Minimum vertical distance between any two gap enemies at the start (sparse).")]
    public float gapEnemySpacingStart = 18f;
    [Tooltip("Minimum vertical distance between gap enemies at max difficulty (dense).")]
    public float gapEnemySpacingEnd = 5f;
    [Tooltip("Horizontal patrol distance for gap enemies.")]
    public float gapEnemyPatrol = 1.2f;
    [Tooltip("Minimum vertical gap size required before an enemy is considered. Larger values = only spawn in big gaps.")]
    public float gapEnemyMinGapSize = 1.3f;
    [Tooltip("Half-width of the 'jump path' dead zone around the arc midpoint X. Enemy won't spawn here.")]
    public float gapEnemyJumpExclusion = 0.9f;

    [Header("Rocket Spawn Settings")]
    public GameObject rocketPrefab;      // �� Inspector rocket
    [Range(0, 1)]
    public float rocketSpawnChance = 0.025f; // probability


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
    // Gap enemies are not parented to any platform – tracked separately for cleanup.
    private readonly List<GameObject> activeGapEnemies = new List<GameObject>();
    private float lastGapEnemyY = float.MinValue; // tracks spacing between gap enemies
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

        // Clean up gap enemies that have scrolled below the camera.
        activeGapEnemies.RemoveAll(e =>
        {
            if (e == null || e.transform.position.y < camBottomY - 3f)
            {
                if (e != null) Destroy(e);
                return true;
            }
            return false;
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
        float prevY = highestSpawnedY; // capture before we overwrite it below
        GameObject p = pool.Get();
        p.transform.position = new Vector3(x, y, 0f);
        p.transform.rotation = Quaternion.identity;

        // 8. Initialize the Tile component
        SetTileType(p, type, d);

        // Gap enemy – floats in the space between prevY and y, no platform attachment.
        // Pass both platform X positions so the enemy can dodge the jump arc.
        TrySpawnGapEnemy(prevY, y, lastX, x, d);

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

    // --- Gap enemy spawning -------------------------------------------------

    void TrySpawnGapEnemy(float bottomY, float topY, float bottomX, float topX, float d)
    {
        if (enemyPrefab == null) return;

        float gapSize = topY - bottomY;
        float midY = (bottomY + topY) * 0.5f;

        // Only spawn in gaps large enough to give the enemy room without blocking the path.
        if (gapSize < gapEnemyMinGapSize) return;

        // Not below the minimum height.
        if (midY < gapEnemyMinHeight) return;

        // Enforce progressive spacing: starts large (sparse) and shrinks to dense.
        float minSpacing = Mathf.Lerp(gapEnemySpacingStart, gapEnemySpacingEnd, d);
        if (midY - lastGapEnemyY < minSpacing) return;

        // Chance also ramps with difficulty so the very first gap enemies are rare.
        float ramp = Mathf.Clamp01((midY - gapEnemyMinHeight) / 30f);
        float chance = gapEnemyMaxChance * ramp * Mathf.Lerp(0.3f, 1f, d);
        if (Random.value > chance) return;

        // --- Position away from the player's jump arc ---
        // The arc midpoint X is the horizontal centre between the two platforms.
        float jumpMidX = (bottomX + topX) * 0.5f;
        float xRange   = CurrentXRange(midY);

        // Try up to 8 times to find an X outside the exclusion zone.
        float ex = 0f;
        bool placed = false;
        for (int attempt = 0; attempt < 8; attempt++)
        {
            float candidate = Random.Range(-xRange * 0.85f, xRange * 0.85f);
            if (Mathf.Abs(candidate - jumpMidX) >= gapEnemyJumpExclusion)
            {
                ex = candidate;
                placed = true;
                break;
            }
        }

        // If every attempt landed in the dead zone (very narrow screen), skip spawn.
        if (!placed) return;

        GameObject enemy = Instantiate(enemyPrefab, new Vector3(ex, midY, 0f), Quaternion.identity);

        // Same visual scale as platform enemies.
        float s = Mathf.Clamp(enemyScale, 0.1f, 2f);
        enemy.transform.localScale = new Vector3(s, s, 1f);

        // Patrol distance: use inspector value, also slightly wider at higher difficulty.
        float patrol = Mathf.Lerp(gapEnemyPatrol, gapEnemyPatrol * 1.5f, d);
        Enemy enemyComp = enemy.GetComponent<Enemy>();
        if (enemyComp != null)
            enemyComp.SetPatrolDistance(patrol);

        activeGapEnemies.Add(enemy);
        lastGapEnemyY = midY;
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
        // Destroy any surviving gap enemies.
        foreach (var e in activeGapEnemies)
            if (e != null) Destroy(e);
        activeGapEnemies.Clear();

        highestSpawnedY = startY - 0.5f;
        lastWasUnreliable = false;
        lastX = 0f;
        lastGapEnemyY = float.MinValue;

        SpawnInitialPlatforms(startY);
    }
}
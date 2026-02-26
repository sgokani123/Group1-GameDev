using UnityEngine;

/// <summary>
/// Pure, scene-independent platform type weight table.
/// Extracted from PlatformSpawner so it can be unit-tested without MonoBehaviour scaffolding.
///
/// Platform type indices:
///   0 = normal      1 = broken    2 = disposable
///   3 = spring      4 = horizontal mover   5 = vertical mover
/// </summary>
public static class PlatformWeights
{
    public struct Weights
    {
        public int normal;
        public int moving;    // split into horiz (4) / vert (5) by caller
        public int broken;
        public int disposable;
        public int spring;

        public int Total => normal + moving + broken + disposable + spring;
    }

    /// <summary>
    /// Returns the integer spawn weights for each platform type at a given difficulty (0–1).
    /// </summary>
    public static Weights Calculate(float difficulty01)
    {
        float d = Mathf.Clamp01(difficulty01);
        return new Weights
        {
            normal      = Mathf.RoundToInt(Mathf.Lerp(72, 34, d)),
            moving      = Mathf.RoundToInt(Mathf.Lerp(10, 26, d)),
            broken      = Mathf.RoundToInt(Mathf.Lerp( 8, 20, d)),
            disposable  = Mathf.RoundToInt(Mathf.Lerp( 6, 18, d)),
            spring      = Mathf.RoundToInt(Mathf.Lerp( 8, 16, d)),
        };
    }

    /// <summary>
    /// Picks a platform type index (0–5) from the weight table using the supplied random value (0–1).
    /// Pass a deterministic value in tests; pass Random.value in production.
    /// </summary>
    public static int PickType(Weights w, float randomValue01, float horizVertRatio01)
    {
        int r = Mathf.FloorToInt(randomValue01 * w.Total);
        r = Mathf.Clamp(r, 0, w.Total - 1);

        if (r < w.normal)      return 0;
        r -= w.normal;

        if (r < w.moving)      return horizVertRatio01 < 0.55f ? 4 : 5;
        r -= w.moving;

        if (r < w.broken)      return 1;
        r -= w.broken;

        if (r < w.disposable)  return 2;

        return 3; // spring
    }
}

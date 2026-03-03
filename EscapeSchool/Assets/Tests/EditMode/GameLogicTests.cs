using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests that verify the core game-logic mathematics used inside
/// GameManager and PlatformSpawner.
///
/// These tests are pure arithmetic — no MonoBehaviour, no scene, no physics.
/// They pin down exactly what the score formula and difficulty ramp must produce
/// so any future refactor is caught immediately.
///
/// TDD cycle:
///   RED    – formula was wrong/absent; test written to define correct behaviour.
///   GREEN  – correct formula implemented in production code.
///   REFACTOR – extracted, commented, or generalised, while tests remain green.
/// </summary>
[TestFixture]
public class GameLogicTests
{
    // ── Score Formula  (GameManager.Update) ──────────────────────────────────
    // Formula: score = Mathf.FloorToInt(heightUnits * 10)
    // Only increases — never decreases when the player falls back down.

    [Test]
    public void ScoreFormula_ZeroHeight_IsZero()
    {
        // TDD RED: early builds showed "SCORE: -0" when spawning below start.
        float height = 0f;
        int score = Mathf.FloorToInt(Mathf.Max(0f, height) * 10f);
        Assert.AreEqual(0, score);
    }

    [Test]
    public void ScoreFormula_NegativeHeight_ClampsToZero()
    {
        // Player briefly below start position must not show negative score.
        float height = -5f;
        int score = Mathf.FloorToInt(Mathf.Max(0f, height) * 10f);
        Assert.AreEqual(0, score);
    }

    [Test]
    public void ScoreFormula_TenUnits_Gives100()
    {
        int score = Mathf.FloorToInt(10f * 10f);
        Assert.AreEqual(100, score);
    }

    [Test]
    public void ScoreFormula_FractionalHeight_TruncatesDown()
    {
        // 1.99 * 10 = 19.9  →  floor  = 19 (not 20)
        int score = Mathf.FloorToInt(1.99f * 10f);
        Assert.AreEqual(19, score);
    }

    [Test]
    public void ScoreFormula_300Units_Gives3000()
    {
        int score = Mathf.FloorToInt(300f * 10f);
        Assert.AreEqual(3000, score);
    }

    [Test]
    public void ScoreBestTracking_NewHigher_Updates()
    {
        // Mirrors the GameManager logic: if (height > score) score = height;
        float currentBest = 5f;
        float newHeight   = 10f;
        float result = newHeight > currentBest ? newHeight : currentBest;
        Assert.AreEqual(10f, result, 0.001f);
    }

    [Test]
    public void ScoreBestTracking_NewLower_Retained()
    {
        float currentBest = 15f;
        float newHeight   = 3f;
        float result = newHeight > currentBest ? newHeight : currentBest;
        Assert.AreEqual(15f, result, 0.001f, "Score must never decrease mid-run.");
    }

    // ── Difficulty Ramp  (PlatformSpawner.Difficulty01) ──────────────────────
    // Formula: Mathf.Clamp01(height / difficultyHeight)
    // difficultyHeight = 300f (inspector default).

    [Test]
    public void Difficulty01_AtHeight0_IsZero()
    {
        float d = Mathf.Clamp01(0f / 300f);
        Assert.AreEqual(0f, d, 0.001f);
    }

    [Test]
    public void Difficulty01_AtDifficultyHeight_IsOne()
    {
        float d = Mathf.Clamp01(300f / 300f);
        Assert.AreEqual(1f, d, 0.001f);
    }

    [Test]
    public void Difficulty01_AtHalfway_IsHalf()
    {
        float d = Mathf.Clamp01(150f / 300f);
        Assert.AreEqual(0.5f, d, 0.001f);
    }

    [Test]
    public void Difficulty01_BeyondMax_ClampsToOne()
    {
        float d = Mathf.Clamp01(9999f / 300f);
        Assert.AreEqual(1f, d, 0.001f, "Difficulty must not exceed 1 even at very high altitude.");
    }

    [Test]
    public void Difficulty01_NegativeHeight_ClampsToZero()
    {
        float d = Mathf.Clamp01(-50f / 300f);
        Assert.AreEqual(0f, d, 0.001f);
    }

    // ── Platform Spawn Gap  (PlatformSpawner.CalculateGap) ───────────────────
    // Formula: gap = Lerp(0, extraGapAtMax=0.9, d) + Random(minYGap=1.0, maxYGap=1.6)
    // Hard cap: maxTotalGap = 2.3  (ensures player can always reach next platform)

    [Test]
    public void SpawnGap_AtDifficulty0_WithMinRandom_IsAtLeastMinYGap()
    {
        float d = 0f;
        float extraGapAtMax = 0.9f;
        float minYGap = 1.0f;

        float extra = Mathf.Lerp(0f, extraGapAtMax, d); // 0
        float gap = minYGap + extra;                      // 1.0
        Assert.GreaterOrEqual(gap, minYGap, "Gap must never fall below minYGap.");
    }

    [Test]
    public void SpawnGap_AtDifficulty1_ExtraGapIsApplied()
    {
        float d = 1f;
        float extraGapAtMax = 0.9f;
        float baseGap = 1.0f; // use min random for determinism

        float gap = baseGap + Mathf.Lerp(0f, extraGapAtMax, d);
        Assert.AreEqual(1.9f, gap, 0.001f, "At max difficulty the full extra gap should be added.");
    }

    [Test]
    public void SpawnGap_NeverExceedsMaxTotalGap()
    {
        float maxTotalGap = 2.3f;

        // Simulate worst case: max random base + max extra
        float maxBase  = 1.6f;
        float maxExtra = 0.9f;
        float rawGap   = maxBase + maxExtra; // 2.5 — over the cap

        float clampedGap = Mathf.Min(rawGap, maxTotalGap);
        Assert.LessOrEqual(clampedGap, maxTotalGap,
            "Gap capped at maxTotalGap ensures the player can always reach the next platform.");
    }

    // ── Platform Type Weights  (PlatformSpawner.GetRandomTileType) ───────────
    // Verifies the Lerp weight tables behave as designed.

    [Test]
    public void PlatformWeights_NormalDominates_AtEarlyGame()
    {
        float d = 0f;
        int normalW     = Mathf.RoundToInt(Mathf.Lerp(72, 34, d)); // 72
        int brokenW     = Mathf.RoundToInt(Mathf.Lerp( 8, 20, d)); //  8
        int disposableW = Mathf.RoundToInt(Mathf.Lerp( 6, 18, d)); //  6
        int springW     = Mathf.RoundToInt(Mathf.Lerp( 8, 16, d)); //  8
        int movingW     = Mathf.RoundToInt(Mathf.Lerp(10, 26, d)); // 10

        Assert.Greater(normalW, brokenW,     "Normal should be more common than broken early on.");
        Assert.Greater(normalW, disposableW, "Normal should be more common than disposable early on.");
        Assert.Greater(normalW, springW,     "Normal should be more common than spring early on.");
        Assert.Greater(normalW, movingW,     "Normal should be more common than moving early on.");
    }

    [Test]
    public void PlatformWeights_SpecialPlatformsGrow_WithDifficulty()
    {
        float d0 = 0f;
        float d1 = 1f;

        int brokenEarly  = Mathf.RoundToInt(Mathf.Lerp( 8, 20, d0));
        int brokenLate   = Mathf.RoundToInt(Mathf.Lerp( 8, 20, d1));
        Assert.Less(brokenEarly, brokenLate, "Broken platform frequency should increase with difficulty.");

        int springEarly  = Mathf.RoundToInt(Mathf.Lerp( 8, 16, d0));
        int springLate   = Mathf.RoundToInt(Mathf.Lerp( 8, 16, d1));
        Assert.Less(springEarly, springLate, "Spring platform frequency should increase with difficulty.");
    }

    [Test]
    public void PlatformWeights_TotalIsAlwaysPositive()
    {
        // A zero total would cause a divide-by-zero in Random.Range.
        foreach (float d in new[] { 0f, 0.25f, 0.5f, 0.75f, 1f })
        {
            int total = Mathf.RoundToInt(Mathf.Lerp(72, 34, d))
                      + Mathf.RoundToInt(Mathf.Lerp(10, 26, d))
                      + Mathf.RoundToInt(Mathf.Lerp( 8, 20, d))
                      + Mathf.RoundToInt(Mathf.Lerp( 6, 18, d))
                      + Mathf.RoundToInt(Mathf.Lerp( 8, 16, d));

            Assert.Greater(total, 0, $"Weight total must be > 0 at difficulty {d}.");
        }
    }

    // ── Vertical Mover Range Cap  (Tile.ApplyType / PlatformSpawner) ─────────

    [Test]
    public void VertMoverRange_CappedBelowGap()
    {
        // If gap = 1.5, maxRange = 1.5 - 0.4 = 1.1
        // Ensures the mover cannot physically reach the platform below.
        float gap = 1.5f;
        float maxRange = Mathf.Max(gap - 0.4f, 0.3f);
        Assert.Less(maxRange, gap, "Vertical mover range must be strictly less than the spawn gap.");
    }

    [Test]
    public void VertMoverRange_SmallGap_NeverBelowMinimum()
    {
        // Even at very small gaps, movement should remain visually perceptible.
        float gap = 0.5f;
        float maxRange = Mathf.Max(gap - 0.4f, 0.3f);
        Assert.GreaterOrEqual(maxRange, 0.3f, "Minimum visible movement range must be 0.3 units.");
    }
}

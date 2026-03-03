using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for LeaderboardManager.
/// These are EditMode tests — no scene or MonoBehaviour required.
///
/// TDD cycle documented for each group:
///   RED    – test written; code path did not exist / behaved incorrectly.
///   GREEN  – minimal implementation made the test pass.
///   REFACTOR – code cleaned up; test still passes.
/// </summary>
[TestFixture]
public class LeaderboardManagerTests
{
    // ── Fixture Setup / Teardown ─────────────────────────────────────────────
    // Wipe both PlayerPrefs keys before and after every test so runs are
    // fully isolated regardless of execution order.

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey("current_player_name");
        PlayerPrefs.DeleteKey("leaderboard_v1");
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey("current_player_name");
        PlayerPrefs.DeleteKey("leaderboard_v1");
        PlayerPrefs.Save();
    }

    // ── Player Name ──────────────────────────────────────────────────────────
    // TDD: RED   → GetCurrentPlayerName returned null (key absent).
    //      GREEN  → return PlayerPrefs.GetString(key, "").
    //      REFACTOR → helper extracted; sanitise on Set, not on Get.

    [Test]
    public void GetCurrentPlayerName_WhenNoneSet_ReturnsEmpty()
    {
        Assert.AreEqual("", LeaderboardManager.GetCurrentPlayerName());
    }

    [Test]
    public void SetAndGet_PlayerName_RoundTrips()
    {
        LeaderboardManager.SetCurrentPlayerName("Kai");
        Assert.AreEqual("Kai", LeaderboardManager.GetCurrentPlayerName());
    }

    [Test]
    public void SetPlayerName_LeadingTrailingSpaces_AreTrimmed()
    {
        // TDD REFACTOR: SanitizeName added after UI returned " Kai " from input field.
        LeaderboardManager.SetCurrentPlayerName("  Kai  ");
        Assert.AreEqual("Kai", LeaderboardManager.GetCurrentPlayerName());
    }

    [Test]
    public void SetPlayerName_OverSixteenChars_IsTruncatedTo16()
    {
        // Prevents TextMeshPro labels from overflowing leaderboard rows.
        LeaderboardManager.SetCurrentPlayerName("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        string result = LeaderboardManager.GetCurrentPlayerName();
        Assert.LessOrEqual(result.Length, 16,
            "Name should be capped at 16 characters to fit the UI.");
    }

    [Test]
    public void SetPlayerName_WhitespaceOnly_SavesAsEmpty()
    {
        LeaderboardManager.SetCurrentPlayerName("   ");
        Assert.AreEqual("", LeaderboardManager.GetCurrentPlayerName());
    }

    // ── Score Submission ─────────────────────────────────────────────────────
    // TDD: RED   → no submit method; scores were lost on restart.
    //      GREEN  → JSON-in-PlayerPrefs storage.
    //      REFACTOR → Entry deduplication; SanitizeName guard.

    [Test]
    public void SubmitScore_WithNoName_IsIgnored()
    {
        // Anonymous players must not pollute the leaderboard.
        LeaderboardManager.SubmitScore(500);
        Assert.AreEqual(0, LeaderboardManager.GetGlobalBest());
    }

    [Test]
    public void SubmitScore_WithName_PersistsBestScore()
    {
        LeaderboardManager.SetCurrentPlayerName("Kai");
        LeaderboardManager.SubmitScore(1000);
        Assert.AreEqual(1000, LeaderboardManager.GetBestForCurrentPlayer());
    }

    [Test]
    public void SubmitScore_HigherFollowsUp_UpdatesBest()
    {
        LeaderboardManager.SetCurrentPlayerName("Kai");
        LeaderboardManager.SubmitScore(500);
        LeaderboardManager.SubmitScore(1200);
        Assert.AreEqual(1200, LeaderboardManager.GetBestForCurrentPlayer());
    }

    [Test]
    public void SubmitScore_LowerFollowUp_DoesNotDecreaseBest()
    {
        // Score should never regress — critical for player confidence.
        LeaderboardManager.SetCurrentPlayerName("Kai");
        LeaderboardManager.SubmitScore(1500);
        LeaderboardManager.SubmitScore(300);
        Assert.AreEqual(1500, LeaderboardManager.GetBestForCurrentPlayer());
    }

    [Test]
    public void SubmitScore_MultipleTimes_OnlyOneEntryPerName()
    {
        // TDD REFACTOR: early version added a new row per submission.
        LeaderboardManager.SetCurrentPlayerName("Kai");
        LeaderboardManager.SubmitScore(100);
        LeaderboardManager.SubmitScore(200);
        LeaderboardManager.SubmitScore(150);

        var board = LeaderboardManager.GetLeaderboardSorted();
        Assert.AreEqual(1, board.Count, "Same player name must produce exactly one leaderboard row.");
    }

    // ── Two-Player Isolation ──────────────────────────────────────────────────

    [Test]
    public void TwoPlayers_ScoresStoredIndependently()
    {
        LeaderboardManager.SetCurrentPlayerName("Kai");
        LeaderboardManager.SubmitScore(800);

        LeaderboardManager.SetCurrentPlayerName("Maya");
        LeaderboardManager.SubmitScore(1100);

        // Maya's personal best
        Assert.AreEqual(1100, LeaderboardManager.GetBestForCurrentPlayer(),
            "Maya's personal best should be 1100.");

        // Switch context back to Kai — his score must be unchanged
        LeaderboardManager.SetCurrentPlayerName("Kai");
        Assert.AreEqual(800, LeaderboardManager.GetBestForCurrentPlayer(),
            "Kai's personal best should still be 800.");
    }

    // ── Global Best ───────────────────────────────────────────────────────────

    [Test]
    public void GetGlobalBest_NoEntries_ReturnsZero()
    {
        Assert.AreEqual(0, LeaderboardManager.GetGlobalBest());
    }

    [Test]
    public void GetGlobalBest_ReturnsHighestAcrossAllPlayers()
    {
        LeaderboardManager.SetCurrentPlayerName("Kai");
        LeaderboardManager.SubmitScore(800);
        LeaderboardManager.SetCurrentPlayerName("Maya");
        LeaderboardManager.SubmitScore(2000);
        Assert.AreEqual(2000, LeaderboardManager.GetGlobalBest());
    }

    // ── Sorted Leaderboard ────────────────────────────────────────────────────

    [Test]
    public void GetLeaderboardSorted_OrdersByScoreDescending()
    {
        LeaderboardManager.SetCurrentPlayerName("Alice");  LeaderboardManager.SubmitScore(500);
        LeaderboardManager.SetCurrentPlayerName("Bob");    LeaderboardManager.SubmitScore(1500);
        LeaderboardManager.SetCurrentPlayerName("Charlie");LeaderboardManager.SubmitScore(900);

        var board = LeaderboardManager.GetLeaderboardSorted();

        Assert.AreEqual(3, board.Count);
        Assert.AreEqual("Bob",     board[0].name);
        Assert.AreEqual("Charlie", board[1].name);
        Assert.AreEqual("Alice",   board[2].name);
    }

    [Test]
    public void GetLeaderboardSorted_EqualScores_TieBreaksByNameAscending()
    {
        LeaderboardManager.SetCurrentPlayerName("Zed"); LeaderboardManager.SubmitScore(1000);
        LeaderboardManager.SetCurrentPlayerName("Ada"); LeaderboardManager.SubmitScore(1000);

        var board = LeaderboardManager.GetLeaderboardSorted();
        Assert.AreEqual("Ada", board[0].name, "Tied scores should be broken alphabetically A→Z.");
        Assert.AreEqual("Zed", board[1].name);
    }
}

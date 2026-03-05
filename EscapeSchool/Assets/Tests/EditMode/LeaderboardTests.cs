using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tests for leaderboard functionality including score display and ranking
/// </summary>
public class LeaderboardTests
{
    [SetUp]
    public void Setup()
    {
        PlayerPrefs.DeleteAll();
        LeaderboardManager.SetCurrentPlayerName("TestPlayer");
    }

    [TearDown]
    public void Teardown()
    {
        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void ClickingLeaderboard_DisplaysPreviousScores()
    {
        // Arrange
        LeaderboardManager.SubmitScore(100);
        LeaderboardManager.SubmitScore(150);
        LeaderboardManager.SubmitScore(75);

        // Act
        var leaderboard = LeaderboardManager.GetLeaderboardSorted();

        // Assert
        Assert.AreEqual(3, leaderboard.Count, "Leaderboard should contain 3 scores");
        Assert.IsTrue(leaderboard.Exists(entry => entry.score == 100), 
            "Leaderboard should contain score 100");
        Assert.IsTrue(leaderboard.Exists(entry => entry.score == 150), 
            "Leaderboard should contain score 150");
        Assert.IsTrue(leaderboard.Exists(entry => entry.score == 75), 
            "Leaderboard should contain score 75");
    }

    [Test]
    public void ClickingLeaderboard_DisplaysHighestScore()
    {
        // Arrange
        LeaderboardManager.SubmitScore(100);
        LeaderboardManager.SubmitScore(250);
        LeaderboardManager.SubmitScore(175);

        // Act
        int highestScore = LeaderboardManager.GetGlobalBest();

        // Assert
        Assert.AreEqual(250, highestScore, "Highest score should be 250");
    }

    [Test]
    public void Leaderboard_SortedByScoreDescending()
    {
        // Arrange
        LeaderboardManager.SubmitScore(100);
        LeaderboardManager.SubmitScore(250);
        LeaderboardManager.SubmitScore(175);

        // Act
        var leaderboard = LeaderboardManager.GetLeaderboardSorted();

        // Assert
        Assert.AreEqual(250, leaderboard[0].score, "First entry should be highest score");
        Assert.AreEqual(175, leaderboard[1].score, "Second entry should be middle score");
        Assert.AreEqual(100, leaderboard[2].score, "Third entry should be lowest score");
    }
}

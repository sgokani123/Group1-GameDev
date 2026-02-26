using NUnit.Framework;
using UnityEngine;

public class LeaderboardManagerTests
{
    [SetUp]
    public void Setup()
    {
        // Clear PlayerPrefs before each test
        PlayerPrefs.DeleteAll();
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up after tests
        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void LeaderboardManager_SetCurrentPlayerName_StoresName()
    {
        // Arrange
        string testName = "TestPlayer";

        // Act
        LeaderboardManager.SetCurrentPlayerName(testName);
        string retrievedName = LeaderboardManager.GetCurrentPlayerName();

        // Assert
        Assert.AreEqual(testName, retrievedName);
    }

    [Test]
    public void LeaderboardManager_GetCurrentPlayerName_ReturnsDefaultWhenNotSet()
    {
        // Act
        string name = LeaderboardManager.GetCurrentPlayerName();

        // Assert
        Assert.IsNotNull(name);
        Assert.IsNotEmpty(name);
    }

    [Test]
    public void LeaderboardManager_SubmitScore_UpdatesLeaderboard()
    {
        // Arrange
        LeaderboardManager.SetCurrentPlayerName("Player1");
        int testScore = 1000;

        // Act
        LeaderboardManager.SubmitScore(testScore);
        int bestScore = LeaderboardManager.GetBestForCurrentPlayer();

        // Assert
        Assert.AreEqual(testScore, bestScore);
    }

    [Test]
    public void LeaderboardManager_GetGlobalBest_ReturnsHighestScore()
    {
        // Arrange
        LeaderboardManager.SetCurrentPlayerName("Player1");
        LeaderboardManager.SubmitScore(500);
        
        LeaderboardManager.SetCurrentPlayerName("Player2");
        LeaderboardManager.SubmitScore(1000);

        // Act
        int globalBest = LeaderboardManager.GetGlobalBest();

        // Assert
        Assert.AreEqual(1000, globalBest);
    }
}

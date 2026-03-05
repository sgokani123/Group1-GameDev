using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TMPro;

/// <summary>
/// Tests for game over screen functionality including score display
/// </summary>
public class GameOverTests
{
    private GameManager gameManager;
    private GameObject gameManagerObj;
    private GameObject playerObj;
    private Player player;

    [SetUp]
    public void Setup()
    {
        // Clear PlayerPrefs for consistent testing
        PlayerPrefs.DeleteAll();

        gameManagerObj = new GameObject("GameManager");
        gameManager = gameManagerObj.AddComponent<GameManager>();
        GameManager.Instance = gameManager;

        playerObj = new GameObject("Player");
        player = playerObj.AddComponent<Player>();
        playerObj.AddComponent<Rigidbody2D>();
        gameManager.player = player;
        gameManager.playerStartPosition = new Vector3(0, 1f, 0);

        // Setup UI elements
        gameManager.gameOverPanel = new GameObject("GameOverPanel");
        
        var finalScoreObj = new GameObject("FinalScore");
        gameManager.finalScoreText = finalScoreObj.AddComponent<TextMeshProUGUI>();
        
        var highScoreObj = new GameObject("HighScore");
        gameManager.highScoreText = highScoreObj.AddComponent<TextMeshProUGUI>();
        
        var statusObj = new GameObject("Status");
        gameManager.gameOverStatusText = statusObj.AddComponent<TextMeshProUGUI>();
    }

    [TearDown]
    public void Teardown()
    {
        if (gameManagerObj != null)
            Object.DestroyImmediate(gameManagerObj);
        if (playerObj != null)
            Object.DestroyImmediate(playerObj);
        GameManager.Instance = null;
        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void OnGameOver_CurrentScoreIsDisplayed()
    {
        // Arrange
        gameManager.State = GameManager.GameState.Playing;
        playerObj.transform.position = new Vector3(0, 6f, 0); // 5 units above start = score 50

        // Act
        gameManager.GameOver();

        // Assert
        Assert.IsTrue(gameManager.finalScoreText.text.Contains("50"), 
            "Final score text should display the score of 50");
        Assert.IsTrue(gameManager.finalScoreText.text.Contains("YOUR SCORE"), 
            "Final score text should contain 'YOUR SCORE' label");
    }

    [Test]
    public void OnGameOver_HighestScoreIsDisplayed()
    {
        // Arrange
        PlayerPrefs.SetInt("BestScore", 100);
        gameManager.State = GameManager.GameState.Playing;
        playerObj.transform.position = new Vector3(0, 3f, 0); // Score 20

        // Act
        gameManager.GameOver();

        // Assert
        Assert.IsTrue(gameManager.highScoreText.text.Contains("100"), 
            "High score text should display the highest score of 100");
        Assert.IsTrue(gameManager.highScoreText.text.Contains("HIGHEST SCORE"), 
            "High score text should contain 'HIGHEST SCORE' label");
    }

    [Test]
    public void OnGameOver_BothScoresDisplayed()
    {
        // Arrange
        PlayerPrefs.SetInt("BestScore", 150);
        gameManager.State = GameManager.GameState.Playing;
        playerObj.transform.position = new Vector3(0, 4f, 0); // Score 30

        // Act
        gameManager.GameOver();

        // Assert
        Assert.IsNotNull(gameManager.finalScoreText.text, "Final score should be displayed");
        Assert.IsNotNull(gameManager.highScoreText.text, "High score should be displayed");
        Assert.IsTrue(gameManager.finalScoreText.text.Contains("30"), 
            "Current score should be 30");
        Assert.IsTrue(gameManager.highScoreText.text.Contains("150"), 
            "Highest score should be 150");
    }

    [Test]
    public void OnGameOver_NewPersonalBest_UpdatesHighScore()
    {
        // Arrange
        LeaderboardManager.SetCurrentPlayerName("TestPlayer"); // Set player name for leaderboard
        PlayerPrefs.SetInt("BestScore", 50);
        gameManager.State = GameManager.GameState.Playing;
        playerObj.transform.position = new Vector3(0, 11f, 0); // Score 100

        // Act
        gameManager.GameOver();
        int savedBest = PlayerPrefs.GetInt("BestScore", 0);

        // Assert
        Assert.AreEqual(100, savedBest, "Personal best should be updated to 100");
        Assert.IsNotNull(gameManager.gameOverStatusText.text, "Status text should not be null");
        Assert.IsTrue(gameManager.gameOverStatusText.text.Contains("PERSONAL BEST") || 
                      gameManager.gameOverStatusText.text.Contains("HIGH SCORE"), 
            "Status should indicate new personal best or high score");
    }

    [Test]
    public void OnGameOver_GameStateChangesToGameOver()
    {
        // Arrange
        gameManager.State = GameManager.GameState.Playing;
        playerObj.transform.position = new Vector3(0, 5f, 0);

        // Act
        gameManager.GameOver();

        // Assert
        Assert.AreEqual(GameManager.GameState.GameOver, gameManager.State, 
            "Game state should be GameOver");
        Assert.IsTrue(gameManager.gameOverPanel.activeSelf, 
            "Game over panel should be visible");
    }

    [Test]
    public void OnGameOver_TimeScaleRemainsNormal()
    {
        // Arrange
        gameManager.State = GameManager.GameState.Playing;
        Time.timeScale = 1f;

        // Act
        gameManager.GameOver();

        // Assert
        Assert.AreEqual(1f, Time.timeScale, "Time scale should remain at 1 during game over");
    }
}

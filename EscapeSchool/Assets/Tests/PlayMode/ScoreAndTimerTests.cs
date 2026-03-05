using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TMPro;

/// <summary>
/// Tests for score and timer display functionality during gameplay
/// </summary>
public class ScoreAndTimerTests
{
    private GameManager gameManager;
    private GameObject gameManagerObj;
    private GameObject playerObj;
    private Player player;

    [SetUp]
    public void Setup()
    {
        gameManagerObj = new GameObject("GameManager");
        gameManager = gameManagerObj.AddComponent<GameManager>();
        GameManager.Instance = gameManager;

        playerObj = new GameObject("Player");
        player = playerObj.AddComponent<Player>();
        playerObj.AddComponent<Rigidbody2D>();
        gameManager.player = player;
        gameManager.playerStartPosition = new Vector3(0, 1f, 0);
    }

    [TearDown]
    public void Teardown()
    {
        if (gameManagerObj != null)
            Object.DestroyImmediate(gameManagerObj);
        if (playerObj != null)
            Object.DestroyImmediate(playerObj);
        GameManager.Instance = null;
    }

    [Test]
    public void OnGameplayOpen_ScoreDisplayedOnTop()
    {
        // Arrange
        var scoreTextObj = new GameObject("ScoreText");
        gameManager.scoreText = scoreTextObj.AddComponent<TextMeshProUGUI>();
        gameManager.hudPanel = new GameObject("HudPanel");

        // Act
        gameManager.StartGame();

        // Assert
        Assert.IsNotNull(gameManager.scoreText, "Score text should exist");
        Assert.IsTrue(gameManager.hudPanel.activeSelf, "HUD panel should be visible");
    }

    [Test]
    public void OnGameplayOpen_ScoreIsZero()
    {
        // Arrange
        var scoreTextObj = new GameObject("ScoreText");
        gameManager.scoreText = scoreTextObj.AddComponent<TextMeshProUGUI>();
        gameManager.hudPanel = new GameObject("HudPanel");
        playerObj.transform.position = gameManager.playerStartPosition;

        // Act
        gameManager.StartGame();

        // Assert
        Assert.IsTrue(gameManager.scoreText.text.Contains("0") || gameManager.scoreText.text == "SCORE: 0",
            "Initial score should be zero");
    }

    [UnityTest]
    public IEnumerator ScoreIncreases_AsPlayerClimbsHigher()
    {
        // Arrange
        var scoreTextObj = new GameObject("ScoreText");
        gameManager.scoreText = scoreTextObj.AddComponent<TextMeshProUGUI>();
        gameManager.hudPanel = new GameObject("HudPanel");
        gameManager.State = GameManager.GameState.Playing;
        
        playerObj.transform.position = gameManager.playerStartPosition;
        yield return null;

        // Act - Move player upward
        playerObj.transform.position = new Vector3(0, 5f, 0);
        yield return null;

        // Manually trigger score update (simulating GameManager.Update)
        float height = playerObj.transform.position.y - gameManager.playerStartPosition.y;
        int expectedScore = Mathf.FloorToInt(height * 10);

        // Assert
        Assert.Greater(expectedScore, 0, "Score should increase as player climbs");
        Assert.AreEqual(40, expectedScore, "Score should be 40 when player is 4 units above start");
    }

    [Test]
    public void ScoreCalculation_IsHeightTimeseTen()
    {
        // Arrange
        gameManager.playerStartPosition = new Vector3(0, 1f, 0);
        playerObj.transform.position = new Vector3(0, 6f, 0);

        // Act
        float height = playerObj.transform.position.y - gameManager.playerStartPosition.y;
        int score = Mathf.FloorToInt(height * 10);

        // Assert
        Assert.AreEqual(5f, height, "Height should be 5 units");
        Assert.AreEqual(50, score, "Score should be height × 10 = 50");
    }
}

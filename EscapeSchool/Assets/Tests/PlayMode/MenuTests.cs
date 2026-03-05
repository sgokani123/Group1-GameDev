using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Tests for main menu functionality including Play, Options, and Leaderboard buttons
/// </summary>
public class MenuTests
{
    private GameManager gameManager;
    private GameObject testGameObject;

    [SetUp]
    public void Setup()
    {
        testGameObject = new GameObject("TestGameManager");
        gameManager = testGameObject.AddComponent<GameManager>();
        GameManager.Instance = gameManager;
    }

    [TearDown]
    public void Teardown()
    {
        if (testGameObject != null)
            Object.DestroyImmediate(testGameObject);
        GameManager.Instance = null;
    }

    [Test]
    public void OnGameOpen_DisplaysHomepageWithPlayOptionsLeaderboardButtons()
    {
        // Arrange
        gameManager.menuPanel = new GameObject("MenuPanel");
        gameManager.hudPanel = new GameObject("HudPanel");
        gameManager.gameOverPanel = new GameObject("GameOverPanel");
        gameManager.optionsPanel = new GameObject("OptionsPanel");
        gameManager.scoresPanel = new GameObject("ScoresPanel");

        // Act
        gameManager.ShowMenu();

        // Assert
        Assert.IsTrue(gameManager.menuPanel.activeSelf, "Menu panel should be visible");
        Assert.IsFalse(gameManager.hudPanel.activeSelf, "HUD panel should be hidden");
        Assert.IsFalse(gameManager.gameOverPanel.activeSelf, "Game over panel should be hidden");
        Assert.IsFalse(gameManager.optionsPanel.activeSelf, "Options panel should be hidden");
        Assert.IsFalse(gameManager.scoresPanel.activeSelf, "Scores panel should be hidden");
        Assert.AreEqual(GameManager.GameState.Menu, gameManager.State, "Game state should be Menu");
    }

    [UnityTest]
    public IEnumerator ClickingPlayButton_OpensGameplay()
    {
        // Arrange
        gameManager.menuPanel = new GameObject("MenuPanel");
        gameManager.hudPanel = new GameObject("HudPanel");
        gameManager.gameOverPanel = new GameObject("GameOverPanel");
        
        var playerObj = new GameObject("Player");
        gameManager.player = playerObj.AddComponent<Player>();
        gameManager.playerStartPosition = Vector3.zero;
        
        var spawnerObj = new GameObject("PlatformSpawner");
        gameManager.platformSpawner = spawnerObj.AddComponent<PlatformSpawner>();
        
        var cameraObj = new GameObject("Camera");
        gameManager.cameraFollow = cameraObj.AddComponent<FollowTarget>();

        gameManager.ShowMenu();

        // Act
        gameManager.StartGame();
        yield return null;

        // Assert
        Assert.IsFalse(gameManager.menuPanel.activeSelf, "Menu panel should be hidden");
        Assert.IsTrue(gameManager.hudPanel.activeSelf, "HUD panel should be visible");
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.State, "Game state should be Playing");
        Assert.IsTrue(gameManager.player.enabled, "Player should be enabled");
    }

    [Test]
    public void ClickingOptionsButton_DisplaysOptionsMenu()
    {
        // Arrange
        gameManager.menuPanel = new GameObject("MenuPanel");
        gameManager.optionsPanel = new GameObject("OptionsPanel");
        gameManager.hudPanel = new GameObject("HudPanel");
        
        var optionsController = gameManager.optionsPanel.AddComponent<OptionsMenuController>();
        gameManager.optionsMenuController = optionsController;

        gameManager.ShowMenu();

        // Act
        gameManager.ShowOptions();

        // Assert
        Assert.IsFalse(gameManager.menuPanel.activeSelf, "Menu panel should be hidden");
        Assert.IsTrue(gameManager.optionsPanel.activeSelf, "Options panel should be visible");
        Assert.AreEqual(GameManager.GameState.Options, gameManager.State, "Game state should be Options");
    }

    [Test]
    public void ClickingLeaderboardButton_DisplaysScoresPanel()
    {
        // Arrange
        gameManager.menuPanel = new GameObject("MenuPanel");
        gameManager.scoresPanel = new GameObject("ScoresPanel");
        gameManager.hudPanel = new GameObject("HudPanel");
        
        var scoresController = gameManager.scoresPanel.AddComponent<ScoresMenuController>();
        gameManager.scoresMenuController = scoresController;

        gameManager.ShowMenu();

        // Act
        gameManager.ShowScores();

        // Assert
        Assert.IsFalse(gameManager.menuPanel.activeSelf, "Menu panel should be hidden");
        Assert.IsTrue(gameManager.scoresPanel.activeSelf, "Scores panel should be visible");
        Assert.AreEqual(GameManager.GameState.Scores, gameManager.State, "Game state should be Scores");
    }
}

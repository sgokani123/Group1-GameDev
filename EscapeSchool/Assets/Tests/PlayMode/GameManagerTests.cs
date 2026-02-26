using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameManagerTests
{
    private GameObject gameManagerObject;
    private GameManager gameManager;

    [SetUp]
    public void Setup()
    {
        // Clear any existing instance
        if (GameManager.Instance != null)
        {
            Object.Destroy(GameManager.Instance.gameObject);
        }

        // Create GameManager
        gameManagerObject = new GameObject("TestGameManager");
        gameManager = gameManagerObject.AddComponent<GameManager>();
        
        // Create minimal UI panels
        gameManager.menuPanel = new GameObject("MenuPanel");
        gameManager.hudPanel = new GameObject("HUDPanel");
        gameManager.pausePanel = new GameObject("PausePanel");
        gameManager.gameOverPanel = new GameObject("GameOverPanel");
        gameManager.optionsPanel = new GameObject("OptionsPanel");
        gameManager.scoresPanel = new GameObject("ScoresPanel");
        gameManager.storePanel = new GameObject("StorePanel");
    }

    [TearDown]
    public void Teardown()
    {
        if (gameManager != null)
        {
            if (gameManager.menuPanel != null) Object.Destroy(gameManager.menuPanel);
            if (gameManager.hudPanel != null) Object.Destroy(gameManager.hudPanel);
            if (gameManager.pausePanel != null) Object.Destroy(gameManager.pausePanel);
            if (gameManager.gameOverPanel != null) Object.Destroy(gameManager.gameOverPanel);
            if (gameManager.optionsPanel != null) Object.Destroy(gameManager.optionsPanel);
            if (gameManager.scoresPanel != null) Object.Destroy(gameManager.scoresPanel);
            if (gameManager.storePanel != null) Object.Destroy(gameManager.storePanel);
            
            Object.Destroy(gameManagerObject);
        }
        
        GameManager.Instance = null;
    }

    [Test]
    public void GameManager_Singleton_CreatesInstance()
    {
        // Assert
        Assert.IsNotNull(GameManager.Instance);
        Assert.AreEqual(gameManager, GameManager.Instance);
    }

    [Test]
    public void GameManager_ShowMenu_SetsMenuState()
    {
        // Act
        gameManager.ShowMenu();

        // Assert
        Assert.AreEqual(GameManager.GameState.Menu, gameManager.State);
        Assert.IsTrue(gameManager.menuPanel.activeSelf);
    }

    [Test]
    public void GameManager_ShowOptions_SetsOptionsState()
    {
        // Act
        gameManager.ShowOptions();

        // Assert
        Assert.AreEqual(GameManager.GameState.Options, gameManager.State);
        Assert.IsTrue(gameManager.optionsPanel.activeSelf);
    }

    [Test]
    public void GameManager_ShowScores_SetsScoresState()
    {
        // Act
        gameManager.ShowScores();

        // Assert
        Assert.AreEqual(GameManager.GameState.Scores, gameManager.State);
        Assert.IsTrue(gameManager.scoresPanel.activeSelf);
    }

    [Test]
    public void GameManager_ShowStore_SetsStoreState()
    {
        // Act
        gameManager.ShowStore();

        // Assert
        Assert.AreEqual(GameManager.GameState.Store, gameManager.State);
        Assert.IsTrue(gameManager.storePanel.activeSelf);
    }

    [Test]
    public void GameManager_Pause_FreezesTime()
    {
        // Arrange
        gameManager.State = GameManager.GameState.Playing;

        // Act
        gameManager.Pause();

        // Assert
        Assert.AreEqual(GameManager.GameState.Paused, gameManager.State);
        Assert.AreEqual(0f, Time.timeScale);
    }

    [Test]
    public void GameManager_Resume_UnfreezesTime()
    {
        // Arrange
        gameManager.State = GameManager.GameState.Paused;
        Time.timeScale = 0f;

        // Act
        gameManager.Resume();

        // Assert
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.State);
        Assert.AreEqual(1f, Time.timeScale);
    }
}

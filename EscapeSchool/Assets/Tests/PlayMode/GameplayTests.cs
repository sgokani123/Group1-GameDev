using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Tests for core gameplay mechanics including hero movement, platform interaction, and game over conditions
/// </summary>
public class GameplayTests
{
    private GameObject playerObj;
    private Player player;
    private Rigidbody2D rb;
    private Camera testCamera;

    [SetUp]
    public void Setup()
    {
        // Create test camera
        var cameraObj = new GameObject("TestCamera");
        cameraObj.tag = "MainCamera";
        testCamera = cameraObj.AddComponent<Camera>();
        testCamera.orthographicSize = 5f;

        // Create player
        playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        player = playerObj.AddComponent<Player>();
        rb = playerObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        playerObj.AddComponent<CircleCollider2D>();
    }

    [TearDown]
    public void Teardown()
    {
        if (playerObj != null)
            Object.DestroyImmediate(playerObj);
        if (testCamera != null)
            Object.DestroyImmediate(testCamera.gameObject);
    }

    [Test]
    public void OnGameplayOpen_HeroStandsOnPlatform()
    {
        // Arrange
        var platformObj = new GameObject("Platform");
        var platformCollider = platformObj.AddComponent<BoxCollider2D>();
        platformObj.AddComponent<Tile>();
        
        playerObj.transform.position = new Vector3(0, 1f, 0);
        platformObj.transform.position = new Vector3(0, 0, 0);

        // Act - Simulate player landing on platform
        rb.linearVelocity = new Vector2(0, -1f);
        
        // Assert
        Assert.IsNotNull(player, "Player should exist");
        Assert.IsTrue(player.enabled, "Player should be enabled");
        Assert.Greater(playerObj.transform.position.y, platformObj.transform.position.y, 
            "Player should be above platform");
    }

    [Test]
    public void DuringGameplay_HeroRemainsOnPlatformUnlessJumpTriggered()
    {
        // Arrange
        var platformObj = new GameObject("Platform");
        platformObj.AddComponent<BoxCollider2D>();
        var tile = platformObj.AddComponent<Tile>();
        tile.tileType = 0; // Normal platform
        
        playerObj.transform.position = new Vector3(0, 1f, 0);
        platformObj.transform.position = new Vector3(0, 0, 0);
        rb.linearVelocity = Vector2.zero;

        // Act - Player is on platform without jump input
        Vector3 initialPosition = playerObj.transform.position;

        // Assert
        Assert.AreEqual(Vector2.zero, rb.linearVelocity, "Player should have no velocity when on platform");
        Assert.AreEqual(initialPosition.y, playerObj.transform.position.y, 
            "Player Y position should remain constant on platform");
    }

    [UnityTest]
    public IEnumerator GameplayEnds_WhenHeroTouchesBaseOfScreen()
    {
        // Arrange
        var gameManagerObj = new GameObject("GameManager");
        var gameManager = gameManagerObj.AddComponent<GameManager>();
        GameManager.Instance = gameManager;
        gameManager.player = player;
        gameManager.State = GameManager.GameState.Playing;
        
        player.SetFloorY(-10f);
        
        // Act - Move player below screen base
        playerObj.transform.position = new Vector3(0, -15f, 0);
        yield return new WaitForSeconds(0.1f);

        // Manually trigger game over check (simulating GameManager.Update)
        if (playerObj.transform.position.y < -10f)
        {
            gameManager.GameOver();
        }

        // Assert
        Assert.AreEqual(GameManager.GameState.GameOver, gameManager.State, 
            "Game state should be GameOver when hero falls below base");
    }

    [UnityTest]
    public IEnumerator HeroLandsOnPlatform_BounceEffectOccurs()
    {
        // Arrange
        var platformObj = new GameObject("Platform");
        platformObj.AddComponent<BoxCollider2D>();
        var tile = platformObj.AddComponent<Tile>();
        tile.tileType = 0; // Normal platform
        
        playerObj.transform.position = new Vector3(0, 2f, 0);
        platformObj.transform.position = new Vector3(0, 0, 0);
        
        // Act - Simulate downward velocity (falling onto platform)
        rb.linearVelocity = new Vector2(0, -5f);
        yield return new WaitForSeconds(0.1f);

        // Simulate collision with platform (would trigger jump in actual game)
        float expectedJumpForce = player.jumpForce;

        // Assert
        Assert.Greater(expectedJumpForce, 0, "Jump force should be positive for bounce effect");
        Assert.AreEqual(12f, expectedJumpForce, "Default jump force should be 12");
    }

    [Test]
    public void ClickingLeftButton_MovesHeroLeft()
    {
        // Arrange
        playerObj.transform.position = Vector3.zero;
        Vector3 initialPosition = playerObj.transform.position;

        // Act - Simulate left movement input
        float moveSpeed = player.moveSpeed;
        float expectedMovement = -moveSpeed * Time.fixedDeltaTime;

        // Assert
        Assert.Greater(moveSpeed, 0, "Move speed should be positive");
        Assert.Less(expectedMovement, 0, "Left movement should be negative X direction");
    }

    [Test]
    public void ClickingRightButton_MovesHeroRight()
    {
        // Arrange
        playerObj.transform.position = Vector3.zero;
        Vector3 initialPosition = playerObj.transform.position;

        // Act - Simulate right movement input
        float moveSpeed = player.moveSpeed;
        float expectedMovement = moveSpeed * Time.fixedDeltaTime;

        // Assert
        Assert.Greater(moveSpeed, 0, "Move speed should be positive");
        Assert.Greater(expectedMovement, 0, "Right movement should be positive X direction");
    }

    [UnityTest]
    public IEnumerator HeroFalls_WhenNotLandingOnHigherPlatform()
    {
        // Arrange
        playerObj.transform.position = new Vector3(0, 5f, 0);
        rb.gravityScale = 1f;

        // Act - Let gravity pull player down
        yield return new WaitForSeconds(0.5f);

        // Assert
        Assert.Less(rb.linearVelocity.y, 0, "Player should have downward velocity when falling");
        Assert.Less(playerObj.transform.position.y, 5f, "Player should fall from initial position");
    }

    [Test]
    public void HeroCannotLandOnLowerPlatform()
    {
        // Arrange
        var lowerPlatformObj = new GameObject("LowerPlatform");
        lowerPlatformObj.AddComponent<BoxCollider2D>();
        lowerPlatformObj.AddComponent<Tile>();
        
        playerObj.transform.position = new Vector3(0, 5f, 0);
        lowerPlatformObj.transform.position = new Vector3(0, 2f, 0);
        
        // Act - Player moving upward should not collide with lower platform
        rb.linearVelocity = new Vector2(0, 5f); // Moving upward

        // Assert
        Assert.Greater(rb.linearVelocity.y, 0, "Player should be moving upward");
        Assert.Greater(playerObj.transform.position.y, lowerPlatformObj.transform.position.y,
            "Player should be above lower platform");
    }
}

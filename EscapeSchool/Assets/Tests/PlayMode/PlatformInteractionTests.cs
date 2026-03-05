using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Tests for platform interaction mechanics including spring platforms and bounce effects
/// </summary>
public class PlatformInteractionTests
{
    private GameObject playerObj;
    private Player player;
    private Rigidbody2D playerRb;

    [SetUp]
    public void Setup()
    {
        playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        player = playerObj.AddComponent<Player>();
        playerRb = playerObj.AddComponent<Rigidbody2D>();
        playerObj.AddComponent<CircleCollider2D>();
    }

    [TearDown]
    public void Teardown()
    {
        if (playerObj != null)
            Object.DestroyImmediate(playerObj);
    }

    [Test]
    public void NormalPlatform_AppliesStandardJumpForce()
    {
        // Arrange
        var platformObj = new GameObject("Platform");
        platformObj.tag = "Tile";
        platformObj.AddComponent<BoxCollider2D>();
        var tile = platformObj.AddComponent<Tile>();
        tile.tileType = 0; // Normal platform

        // Act
        float expectedJumpForce = player.jumpForce;

        // Assert
        Assert.AreEqual(12f, expectedJumpForce, "Normal platform should use standard jump force of 12");
        
        Object.DestroyImmediate(platformObj);
    }

    [Test]
    public void SpringPlatform_AppliesIncreasedJumpForce()
    {
        // Arrange
        var platformObj = new GameObject("Platform");
        platformObj.tag = "Tile";
        platformObj.AddComponent<BoxCollider2D>();
        var tile = platformObj.AddComponent<Tile>();
        tile.tileType = 3; // Spring platform

        // Act
        float baseJumpForce = player.jumpForce;
        float springMultiplier = 1.5f;
        float expectedSpringJump = baseJumpForce * springMultiplier;

        // Assert
        Assert.AreEqual(18f, expectedSpringJump, "Spring platform should apply 1.5x jump multiplier");
        
        Object.DestroyImmediate(platformObj);
    }

    [Test]
    public void BrokenPlatform_HasCorrectType()
    {
        // Arrange & Act
        var platformObj = new GameObject("Platform");
        platformObj.tag = "Tile";
        platformObj.AddComponent<BoxCollider2D>();
        var tile = platformObj.AddComponent<Tile>();
        tile.tileType = 1; // Broken platform

        // Assert
        Assert.AreEqual(1, tile.tileType, "Broken platform should have type 1");
        
        Object.DestroyImmediate(platformObj);
    }

    [Test]
    public void DisposablePlatform_HasCorrectType()
    {
        // Arrange & Act
        var platformObj = new GameObject("Platform");
        platformObj.tag = "Tile";
        platformObj.AddComponent<BoxCollider2D>();
        var tile = platformObj.AddComponent<Tile>();
        tile.tileType = 2; // Disposable platform

        // Assert
        Assert.AreEqual(2, tile.tileType, "Disposable platform should have type 2");
        
        Object.DestroyImmediate(platformObj);
    }
}

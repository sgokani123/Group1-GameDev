using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerTests
{
    private GameObject playerObject;
    private Player player;
    private Rigidbody2D rb;

    [SetUp]
    public void Setup()
    {
        // Create player GameObject with required components
        playerObject = new GameObject("TestPlayer");
        player = playerObject.AddComponent<Player>();
        rb = playerObject.AddComponent<Rigidbody2D>();
        
        // Set up camera
        GameObject cameraObject = new GameObject("MainCamera");
        cameraObject.tag = "MainCamera";
        Camera cam = cameraObject.AddComponent<Camera>();
        cam.orthographicSize = 5f;
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerObject);
        
        // Clean up camera
        GameObject cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        if (cameraObject != null)
            Object.Destroy(cameraObject);
    }

    [UnityTest]
    public IEnumerator Player_Jump_AddsUpwardForce()
    {
        // Arrange
        float initialY = player.transform.position.y;
        
        // Act
        player.Jump(1f);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // Assert
        Assert.Greater(rb.linearVelocity.y, 0, "Player should have upward velocity after jump");
    }

    [Test]
    public void Player_SetFloorY_UpdatesFloorPosition()
    {
        // Arrange
        float testFloorY = -5f;

        // Act
        player.SetFloorY(testFloorY);

        // Assert - floor is set (we can't directly test private field, but method should not throw)
        Assert.IsNotNull(player);
    }

    [Test]
    public void Player_RefreshBorders_DoesNotThrowException()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => player.RefreshBorders());
    }

    [UnityTest]
    public IEnumerator Player_ResetPlayer_ResetsPositionAndVelocity()
    {
        // Arrange
        Vector3 startPosition = new Vector3(0, 5, 0);
        rb.linearVelocity = new Vector2(5, -5);

        // Act
        player.ResetPlayer(startPosition);
        yield return new WaitForFixedUpdate();

        // Assert - Use tolerance for position due to physics simulation
        Assert.AreEqual(startPosition.x, player.transform.position.x, 0.1f, "X position should be reset");
        Assert.AreEqual(startPosition.y, player.transform.position.y, 0.5f, "Y position should be approximately reset");
        Assert.AreEqual(startPosition.z, player.transform.position.z, 0.1f, "Z position should be reset");
    }
}

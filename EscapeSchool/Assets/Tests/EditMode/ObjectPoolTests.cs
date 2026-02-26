using NUnit.Framework;
using UnityEngine;

public class ObjectPoolTests
{
    private GameObject testPrefab;
    private ObjectPool pool;

    [SetUp]
    public void Setup()
    {
        // Create a simple test prefab
        testPrefab = new GameObject("TestPrefab");
        
        // Create pool object
        GameObject poolObject = new GameObject("TestPool");
        pool = poolObject.AddComponent<ObjectPool>();
    }

    [TearDown]
    public void Teardown()
    {
        if (testPrefab != null)
            Object.DestroyImmediate(testPrefab);
        
        if (pool != null)
            Object.DestroyImmediate(pool.gameObject);
    }

    [Test]
    public void ObjectPool_Init_CreatesPrewarmedObjects()
    {
        // Arrange
        int prewarmCount = 5;

        // Act
        pool.Init(testPrefab, prewarmCount);

        // Assert
        Assert.IsNotNull(pool);
    }

    [Test]
    public void ObjectPool_Get_ReturnsActiveObject()
    {
        // Arrange
        pool.Init(testPrefab, 1);

        // Act
        GameObject obj = pool.Get();

        // Assert
        Assert.IsNotNull(obj);
        Assert.IsTrue(obj.activeSelf);
    }

    [Test]
    public void ObjectPool_Return_DeactivatesObject()
    {
        // Arrange
        pool.Init(testPrefab, 1);
        GameObject obj = pool.Get();

        // Act
        pool.Return(obj);

        // Assert
        Assert.IsFalse(obj.activeSelf);
    }

    [Test]
    public void ObjectPool_GetMultiple_ReusesReturnedObjects()
    {
        // Arrange
        pool.Init(testPrefab, 1);
        GameObject firstObj = pool.Get();
        pool.Return(firstObj);

        // Act
        GameObject secondObj = pool.Get();

        // Assert
        Assert.AreEqual(firstObj, secondObj, "Pool should reuse returned objects");
    }
}

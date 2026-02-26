# EscapeSchool Test Suite

This directory contains automated tests for the EscapeSchool Unity project.

## Directory Structure

```
Tests/
├── EditMode/           # Unit tests (fast, no Play mode required)
│   ├── EditModeTests.asmdef
│   ├── ObjectPoolTests.cs
│   └── LeaderboardManagerTests.cs
└── PlayMode/           # Integration tests (requires Play mode)
    ├── PlayModeTests.asmdef
    ├── PlayerTests.cs
    └── GameManagerTests.cs
```

## Quick Start

### Running Tests in Unity Editor

1. Open Unity Editor
2. Window > General > Test Runner
3. Select EditMode or PlayMode tab
4. Click "Run All" or select individual tests

### Test Categories

#### Edit Mode Tests

- Fast execution (no scene loading)
- Test pure C# logic
- No MonoBehaviour lifecycle dependencies
- Examples: ObjectPool, LeaderboardManager

#### Play Mode Tests

- Full Unity runtime environment
- Test MonoBehaviour components
- Physics and coroutines
- Examples: Player movement, GameManager state

## Writing New Tests

### Edit Mode Test Template

```csharp
using NUnit.Framework;
using UnityEngine;

public class MyClassTests
{
    [SetUp]
    public void Setup()
    {
        // Initialize before each test
    }

    [Test]
    public void MyClass_Method_ExpectedBehavior()
    {
        // Arrange
        var myObject = new MyClass();
        
        // Act
        var result = myObject.Method();
        
        // Assert
        Assert.AreEqual(expectedValue, result);
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up after each test
    }
}
```

### Play Mode Test Template

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MyComponentTests
{
    private GameObject testObject;

    [SetUp]
    public void Setup()
    {
        testObject = new GameObject("Test");
        testObject.AddComponent<MyComponent>();
    }

    [UnityTest]
    public IEnumerator MyComponent_Action_ProducesResult()
    {
        // Arrange
        var component = testObject.GetComponent<MyComponent>();
        
        // Act
        component.DoAction();
        yield return new WaitForSeconds(0.1f);
        
        // Assert
        Assert.IsTrue(component.ActionCompleted);
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(testObject);
    }
}
```

## Best Practices

1. **Test Naming**: Use `ClassName_MethodName_ExpectedBehavior` format
2. **AAA Pattern**: Arrange, Act, Assert structure
3. **Isolation**: Each test should be independent
4. **Cleanup**: Always destroy created objects in TearDown
5. **Fast Tests**: Keep tests quick; use Edit Mode when possible
6. **Clear Assertions**: One logical assertion per test

## Common Assertions

```csharp
// Equality
Assert.AreEqual(expected, actual);
Assert.AreNotEqual(expected, actual);

// Null checks
Assert.IsNull(obj);
Assert.IsNotNull(obj);

// Boolean
Assert.IsTrue(condition);
Assert.IsFalse(condition);

// Numeric comparisons
Assert.Greater(actual, expected);
Assert.Less(actual, expected);

// Collections
Assert.Contains(item, collection);
Assert.IsEmpty(collection);

// Exceptions
Assert.Throws<ExceptionType>(() => method());
Assert.DoesNotThrow(() => method());
```

## Troubleshooting

### Tests Not Appearing

- Ensure assembly definition files (.asmdef) are present
- Check that test files are in correct folders
- Reimport the Tests folder (right-click > Reimport)

### Tests Failing Unexpectedly

- Check for leftover objects from previous tests
- Verify TearDown methods are cleaning up properly
- Ensure tests don't depend on execution order

### Play Mode Tests Timing Out

- Add appropriate yield statements
- Use `yield return new WaitForFixedUpdate()` for physics
- Increase timeout if needed: `[UnityTest, Timeout(5000)]`

## CI/CD Integration

Run tests from command line:

```bash
Unity.exe -runTests -batchmode -projectPath "." -testResults "results.xml" -testPlatform EditMode
```

Exit code 0 = all tests passed
Exit code 2 = one or more tests failed
Exit code 3 = run error

## Resources

- [Unity Test Framework Documentation](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- [NUnit Documentation](https://docs.nunit.org/)
- [Unity Testing Best Practices](https://unity.com/how-to/unity-test-framework-video-game-development)

# Group1-GameDev

## Overview

This is the group project for the course COMP4024 - Software Engineering Management. The project is focused on game development, and our team has created a game called "ESC School". Below are the details of the project. The game must be developed with the Unity game engine, and the target audience is children between ages 11 and 14 years. The game will be designed to be fun, engaging, and suitable for this age group.

Course code: COMP4024
Course title: Software Engineering Management
Project title: Game Development
Game engine: Unity
Target audience: Children between ages 11 and 14 years

## Game Description

**"ESC School"** is a 2D platformer game where players take on the role of a student trying to escape from school. The game features various levels filled with obstacles, enemies (usually school teachers), and puzzles that players must navigate through to reach the exit. Players can collect power-ups and items to help them along the way, and they must use their skills and quick reflexes to avoid dangers and successfully escape the school. The game will have a fun and engaging storyline, colorful graphics, and catchy music to create an enjoyable gaming experience for players between ages 11 and 14 years.

## Team Members

- [Bianca Anghelina]()
- [Chang Ma]()
- [Oluwatobiloba Akanji]()
- [Rushanth Kalaiarasan]()
- [Shaan Gokani]()
- [Thanaporn Niyomp]()
- [Weijian Guo]()

## Workflow

The project makes use of GitHub for version control and collaboration. Each team member is responsible for specific tasks, such as game design, programming, art, and testing. Regular meetings are held to discuss progress, share updates, and address any challenges that arise during development. The team follows an agile development approach, allowing for flexibility and iterative improvements throughout the project lifecycle.

For code changes, the team uses the **Git Workflow** pull requests to review and merge changes into the main branch. This ensures that all code is reviewed by at least one other team member before being integrated into the project, helping to maintain code quality and consistency. Additionally, the team uses ClickUp to track tasks, bugs, and feature requests, allowing for efficient project management and communication among team members.

### The Git Workflow

1. **Branching**: Each team member creates a new branch for their specific task or feature off of the `main` branch. This allows for isolated development and prevents conflicts with the main codebase. The branch name should be descriptive of the task or feature being worked on, such as `feature/level-design` or `bugfix/player-movement`. This helps to keep the repository organized and makes it easier for team members to understand the purpose of each branch.

2. **Development**: Team members work on their assigned tasks in their respective branches, making commits to track their progress. Commit messages should be clear and descriptive, following a consistent format (e.g., `feat: add new level design` or `fix: resolve player movement bug`). This helps to maintain a clear history of changes and makes it easier for other team members to understand the purpose of each commit.

3. **Pull Requests**: Once a feature or task is completed, the team member creates a pull request to merge their branch into the main branch. This allows for code review and feedback from other team members.

4. **Code Review**: Other team members review the pull request, providing feedback and suggestions for improvements. This helps to ensure code quality and consistency across the project.

5. **Merging**: After the pull request has been reviewed and approved, it is merged into the main branch. This allows the new feature or task to be integrated into the project and made available for testing.

## Automated Testing

The project includes a comprehensive automated test suite to ensure code quality and catch bugs early. Tests are organized into two categories:

### Test Structure

- **Edit Mode Tests** (EscapeSchool/Assets/Tests/EditMode/): Unit tests that run in the Unity Editor without entering Play mode. These tests are fast and ideal for testing game logic, data structures, and utility functions.
- **Play Mode Tests** (EscapeSchool/Assets/Tests/PlayMode/): Integration tests that run in Play mode, allowing testing of MonoBehaviour components, physics, and runtime behavior.

### Running Tests

#### Using Unity Test Runner (Recommended)

1. Open the Unity project in Unity Editor
2. Go to **Window > General > Test Runner**
3. The Test Runner window will show two tabs: **EditMode** and **PlayMode**
4. Click **Run All** in either tab to execute all tests in that category
5. Click individual tests to run them separately
6. View test results, including pass/fail status and execution time

#### Using Unity Command Line

Run all tests from the command line:

```bash
# Windows
Unity.exe -runTests -batchmode -projectPath "path/to/EscapeSchool" -testResults "path/to/results.xml" -testPlatform EditMode

# macOS
/Applications/Unity/Unity.app/Contents/MacOS/Unity -runTests -batchmode -projectPath "path/to/EscapeSchool" -testResults "path/to/results.xml" -testPlatform EditMode
```

Replace -testPlatform EditMode with -testPlatform PlayMode to run Play mode tests.

### Test Coverage

Current test coverage includes:

- **ObjectPool**: Object pooling functionality, reuse behavior, and memory management
- **LeaderboardManager**: Score submission, player name management, and high score tracking
- **Player**: Movement, jumping, position reset, and boundary management
- **GameManager**: State management, UI panel transitions, pause/resume functionality

### Adding New Tests

1. Create a new C# script in the appropriate test folder (EditMode or PlayMode)
2. Add using NUnit.Framework; and using UnityEngine.TestTools; (for PlayMode)
3. Use [Test] attribute for synchronous tests
4. Use [UnityTest] with IEnumerator for asynchronous tests that need to wait for frames
5. Use [SetUp] and [TearDown] for test initialization and cleanup

Example test structure:

```csharp
using NUnit.Framework;
using UnityEngine;

public class MyComponentTests
{
    [SetUp]
    public void Setup()
    {
        // Initialize test objects
    }

    [Test]
    public void MyComponent_DoSomething_ReturnsExpectedValue()
    {
        // Arrange
        // Act
        // Assert
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up test objects
    }
}
```

### Continuous Integration

Tests can be integrated into CI/CD pipelines using Unity's command-line test execution. Configure your CI system to run tests automatically on each commit or pull request to maintain code quality.

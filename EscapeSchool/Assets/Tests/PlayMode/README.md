# Escape School - Test Suite

This directory contains comprehensive unit tests for the Escape School game covering all specified use cases.

## Test Files

### 1. MenuTests.cs

Tests for main menu functionality:

- Homepage displays Play, Options, and Leaderboard buttons
- Clicking Play button opens gameplay
- Clicking Options button displays options menu
- Clicking Leaderboard button displays scores panel

### 2. GameplayTests.cs

Tests for core gameplay mechanics:

- Hero stands on platform when gameplay opens
- Hero remains on platform unless jump is triggered
- Gameplay ends when hero touches base of screen
- Hero lands on platform with bounce effect
- Left/right button movement
- Hero falls when not landing on higher platform
- Hero cannot land on lower platform

### 3. PlatformInteractionTests.cs

Tests for platform interaction:

- Normal platform applies standard jump force
- Spring platform applies increased jump force (1.5x)
- Broken and disposable platform types

### 4. ScoreAndTimerTests.cs

Tests for score and timer display:

- Score displayed on top of screen when gameplay opens
- Score is zero when gameplay opens
- Score increases as player climbs higher
- Score calculation is height × 10

### 5. GameOverTests.cs

Tests for game over screen:

- Current score displayed when gameplay ends
- Highest score displayed when gameplay ends
- Both scores displayed simultaneously
- New personal best updates high score
- Game state changes to GameOver
- Time scale remains normal

### 6. LeaderboardTests.cs

Tests for leaderboard functionality:

- Previous scores displayed
- Highest score displayed
- Scores sorted by descending order

### 7. SoundOptionsTests.cs

Tests for sound options:

- Clicking On button enables sound
- Clicking Off button disables sound
- Music plays when sound is enabled
- On button does nothing when sound already enabled
- Off button does nothing when sound already disabled
- Sound state persists in PlayerPrefs
- Sound manager restores muted state on startup

## Running Tests

### In Unity Editor

1. Open Window > General > Test Runner
2. Select "EditMode" tab
3. Click "Run All" to execute all tests
4. Or select individual test files to run specific test suites

### From Command Line

```bash
Unity.exe -runTests -batchmode -projectPath "path/to/EscapeSchool" -testResults results.xml -testPlatform EditMode
```

## Test Coverage

These tests cover all 30+ use cases specified:

- ✅ Main menu navigation (Play, Options, Leaderboard)
- ✅ Gameplay mechanics (movement, jumping, platforms)
- ✅ Platform interactions (normal, spring, broken, disposable)
- ✅ Score system (display, calculation, persistence)
- ✅ Game over conditions and score display
- ✅ Leaderboard functionality
- ✅ Sound options (on/off, persistence)

## Notes

- Tests use Unity Test Framework (NUnit)
- Some tests are marked with [UnityTest] for coroutine support
- PlayerPrefs are cleared before each test for consistency
- Tests create minimal GameObject hierarchies to isolate functionality
- Mock objects are used where full scene setup is not required

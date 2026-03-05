using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Central game controller. Attach to an empty "GameManager" GameObject in the scene.
/// Wire up all public fields in the Inspector.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ─── State ───────────────────────────────────────────────
    public enum GameState { Menu, Playing, Paused, GameOver, Options, Scores, Store }
    public GameState State { get; private set; } = GameState.Menu;

    // ─── UI Panels ────────────────────────────────────────────
    [Header("UI Panels")]
    public GameObject menuPanel;        // Main menu panel
    public GameObject hudPanel;         // In-game HUD panel
    public GameObject pausePanel;       // Pause overlay panel
    public GameObject gameOverPanel;    // Game over panel
    public GameObject optionsPanel;     // Options page
    public GameObject scoresPanel;      // Scores/leaderboard page
    public GameObject storePanel;       // Store page

    // ─── HUD Text ─────────────────────────────────────────────
    [Header("HUD")]
    public TextMeshProUGUI scoreText;         // Shows live score during play
    public TextMeshProUGUI hudLoggedInText;   // "Logged in as: Name" shown in-game

    // ─── Game Over Text ───────────────────────────────────────
    [Header("Game Over Screen")]
    public TextMeshProUGUI finalScoreText;      // Score at end
    public TextMeshProUGUI highScoreText;       // All-time best
    public TextMeshProUGUI gameOverStatusText;  // "New Personal Best!", "New #1!" etc.

    // ─── Main Menu Text ───────────────────────────────────────
    [Header("Menu / Scores")]
    public TextMeshProUGUI menuHighScoreText;    // Best score shown on menu (optional)
    public TextMeshProUGUI menuLoggedInText;     // "Playing as: Name" shown on main menu
    public TextMeshProUGUI scoresHighScoreText;  // Best score shown on scores page (optional)

    // ─── Scene References ─────────────────────────────────────
    [Header("References")]
    public Player player;
    public FollowTarget cameraFollow;
    public PlatformSpawner platformSpawner;

    [Header("Menu Controllers")]
    public OptionsMenuController optionsMenuController;
    public ScoresMenuController scoresMenuController;

    // ─── Private ──────────────────────────────────────────────
    [Header("Start Position")]
    public Vector3 playerStartPosition = new Vector3(0f, 1f, 0f); // Set just above platform_0 in Inspector
    private float score;
    private float highScore;
    private bool optionsOpenedFromPause = false;



    // ─────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        highScore = PlayerPrefs.GetFloat("HighScore", 0f);
        Time.timeScale = 1f;
    }

    void Start()
    {
        ShowMenu();
    }

    void Update()
    {
        HandlePauseHotkey();

        if (State != GameState.Playing) return;

        // Score = units climbed above start position × 10
        float height = player.transform.position.y - playerStartPosition.y;
        if (height > score) score = height;

        if (scoreText != null)
            scoreText.text = "SCORE: " + Mathf.FloorToInt(score * 10);
    }

    // ─── Main Menu Buttons ───────────────────────────────────

    /// Called by the "Play" button on the main menu
    public void StartGame()
    {
        // click sound
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(0);

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.isGameOver = false;
            MusicManager.Instance.GetComponent<AudioSource>().pitch = 1f;
        }

        score = 0f;
        State = GameState.Playing;
        Time.timeScale = 1f;

        // Activate player first so its Awake/Start can run
        if (player != null) player.gameObject.SetActive(true);

        // Reset platforms starting from just below the player start
        if (platformSpawner != null)
            platformSpawner.ResetSpawner(playerStartPosition.y);

        // Reset camera
        if (cameraFollow != null)
            cameraFollow.ResetCamera(playerStartPosition.y);

        // Set the hard floor at platform_0's Y so the player can never fall through it.
        // platform_0 is always spawned at playerStartPosition.y - 0.3f (see PlatformSpawner).
        if (player != null)
            player.SetFloorY(playerStartPosition.y - 0.3f);

        // Reset player position and give initial jump
        if (player != null)
            player.ResetPlayer(playerStartPosition);

        RefreshLoggedInLabels();
        SetPanels(menu: false, hud: true, pause: false, gameOver: false, options: false, scores: false, store: false);
    }

    /// Called by Player when it falls off the bottom of the screen
    public void GameOver()
    {
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;
        Time.timeScale = 1f;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.isGameOver = true;
        }

        // Capture any last-frame height
        float height = (player != null ? player.transform.position.y : playerStartPosition.y) - playerStartPosition.y;
        if (height > score) score = height;

        // Final score (int)
        int finalScoreInt = Mathf.FloorToInt(score * 10);

        // Personal best = PlayerPrefs (independent of leaderboard position)
        int prevPersonalBest = PlayerPrefs.GetInt("BestScore", 0);
        int prevGlobalBest   = LeaderboardManager.GetGlobalBest();

        // Submit this run to the top-10 (same player can appear multiple times)
        LeaderboardManager.SubmitScore(finalScoreInt);

        // Update personal best PlayerPref
        int savedBest = finalScoreInt > prevPersonalBest ? finalScoreInt : prevPersonalBest;
        if (finalScoreInt > prevPersonalBest)
        {
            PlayerPrefs.SetInt("BestScore", finalScoreInt);
            PlayerPrefs.Save();
        }

        // Post-submit state
        int newRank      = LeaderboardManager.GetRankForCurrentPlayer(); // best rank for this player
        int newGlobalBest = LeaderboardManager.GetGlobalBest();
        int displayBest  = savedBest;

        // Build status message
        bool isNewPersonal = finalScoreInt > prevPersonalBest;
        bool isNewGlobal   = newRank == 1 && finalScoreInt >= newGlobalBest;

        string statusMsg = "";
        if (isNewGlobal)
            statusMsg = "NEW #1 HIGH SCORE!";
        else if (newRank > 0 && isNewPersonal)
            statusMsg = "NEW PERSONAL BEST!  Rank #" + newRank;
        else if (newRank > 0)
            statusMsg = "Rank #" + newRank + "!";  
        else if (isNewPersonal)
            statusMsg = "NEW PERSONAL BEST!";

        if (gameOverStatusText != null)
        {
            gameOverStatusText.text = statusMsg;
            gameOverStatusText.gameObject.SetActive(statusMsg.Length > 0);
        }

        // Keep highScore float in sync
        highScore = (newGlobalBest > 0 ? newGlobalBest : savedBest) / 10f;

        if (finalScoreText != null) finalScoreText.text = "YOUR SCORE: " + finalScoreInt;
        if (highScoreText  != null) highScoreText.text  = "HIGHEST SCORE: " + displayBest;

        SetPanels(menu: false, hud: false, pause: false, gameOver: true, options: false, scores: false, store: false);

        if (player != null) player.gameObject.SetActive(false);
    }

    /// Called by the "Retry" button on the game over screen
    public void RetryGame()
    {
        // click sound
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(0);

        StartGame();
    }

    /// Called by "Menu" buttons to return to the main menu
    public void ShowMenu()
    {
        // click sound
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(0);

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.isGameOver = false;
            MusicManager.Instance.GetComponent<AudioSource>().pitch = 1f;
        }

        State = GameState.Menu;
        Time.timeScale = 1f;

        if (player != null) player.gameObject.SetActive(false);

        // Snap camera to start position immediately so there's no glitch when Play is pressed
        if (cameraFollow != null)
            cameraFollow.ResetCamera(playerStartPosition.y);

        if (menuHighScoreText != null)
            menuHighScoreText.text = "BEST: " + LeaderboardManager.GetGlobalBest();

        RefreshLoggedInLabels();
        SetPanels(menu: true, hud: false, pause: false, gameOver: false, options: false, scores: false, store: false);
    }

    // ─── Navigation Pages ────────────────────────────────────

    public void ShowOptions()
    {
        // click sound
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(0);

        optionsOpenedFromPause = false;
        State = GameState.Options;
        Time.timeScale = 1f;
        SetPanels(menu: false, hud: false, pause: false, gameOver: false, options: true, scores: false, store: false);

        if (optionsMenuController != null)
            optionsMenuController.SyncFromSaved();
    }

    /// <summary>Opens Options from the Pause screen — game stays paused, back returns to pause.</summary>
    public void ShowOptionsFromPause()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(0);

        optionsOpenedFromPause = true;
        State = GameState.Options;
        // Keep timeScale = 0 so the game stays paused
        if (optionsPanel  != null) optionsPanel.SetActive(true);
        if (pausePanel    != null) pausePanel.SetActive(false);
        if (hudPanel      != null) hudPanel.SetActive(false);

        if (optionsMenuController != null)
            optionsMenuController.SyncFromSaved();
    }

    /// <summary>Called by the MenuButton inside OptionsPanel. Returns to pause if that's where we came from.</summary>
    public void CloseOptions()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(0);

        if (optionsOpenedFromPause)
        {
            optionsOpenedFromPause = false;
            State = GameState.Paused;
            // Time stays at 0 — still paused
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (pausePanel   != null) pausePanel.SetActive(true);
            if (hudPanel     != null) hudPanel.SetActive(true);
        }
        else
        {
            ShowMenu();
        }
    }

    public void ShowScores()
    {
        // click sound
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(0);

        State = GameState.Scores;
        Time.timeScale = 1f;

        // Show global best on scores page (optional)
        int globalBest = LeaderboardManager.GetGlobalBest();
        if (scoresHighScoreText != null)
            scoresHighScoreText.text = "BEST: " + globalBest;

        SetPanels(menu: false, hud: false, pause: false, gameOver: false, options: false, scores: true, store: false);

        if (scoresMenuController != null)
            scoresMenuController.Refresh();
    }

    public void ShowStore()
    {
        // click sound
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(0);

        State = GameState.Store;
        Time.timeScale = 1f;
        SetPanels(menu: false, hud: false, pause: false, gameOver: false, options: false, scores: false, store: true);
    }

    // ─── Pause ───────────────────────────────────────────────

    public void Pause()
    {
        if (State != GameState.Playing) return;

        // click sound
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(0);

        State = GameState.Paused;
        Time.timeScale = 0f;

        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;

        // click sound
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(0);

        State = GameState.Playing;
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
    }

    // ─── Helpers ─────────────────────────────────────────────
    /// <summary>Updates the "Logged in as" labels in both HUD and main menu.</summary>
    public void RefreshLoggedInLabels()
    {
        string name = LeaderboardManager.GetCurrentPlayerName();
        if (hudLoggedInText  != null) hudLoggedInText.text  = "Playing as: " + name;
        if (menuLoggedInText != null) menuLoggedInText.text = "Playing as: " + name;
    }
    void SetPanels(bool menu, bool hud, bool pause, bool gameOver, bool options, bool scores, bool store)
    {
        if (menuPanel != null) menuPanel.SetActive(menu);
        if (hudPanel != null) hudPanel.SetActive(hud);
        if (pausePanel != null) pausePanel.SetActive(pause);
        if (gameOverPanel != null) gameOverPanel.SetActive(gameOver);
        if (optionsPanel != null) optionsPanel.SetActive(options);
        if (scoresPanel != null) scoresPanel.SetActive(scores);
        if (storePanel != null) storePanel.SetActive(store);

        // If we're not paused, ensure pause overlay is off
        if (State != GameState.Paused && pausePanel != null)
            pausePanel.SetActive(false);
    }

    void HandlePauseHotkey()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }
    }
}
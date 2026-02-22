using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Central game controller. Attach to an empty "GameManager" GameObject in the scene.
/// Wire up all public fields in the Inspector.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ─── State ───────────────────────────────────────────────
    public enum GameState { Menu, Playing, GameOver }
    public GameState State { get; private set; } = GameState.Menu;

    // ─── UI Panels ────────────────────────────────────────────
    [Header("UI Panels")]
    public GameObject menuPanel;        // Main menu panel
    public GameObject hudPanel;         // In-game HUD panel
    public GameObject gameOverPanel;    // Game over panel

    // ─── HUD Text ─────────────────────────────────────────────
    [Header("HUD")]
    public TextMeshProUGUI scoreText;       // Shows live score during play

    // ─── Game Over Text ───────────────────────────────────────
    [Header("Game Over Screen")]
    public TextMeshProUGUI finalScoreText;  // Score at end
    public TextMeshProUGUI highScoreText;   // All-time best

    // ─── Main Menu Text ───────────────────────────────────────
    [Header("Main Menu")]
    public TextMeshProUGUI menuHighScoreText; // Best score shown on menu (optional)

    // ─── Scene References ─────────────────────────────────────
    [Header("References")]
    public Player          player;
    public FollowTarget    cameraFollow;
    public PlatformSpawner platformSpawner;

    // ─── Private ──────────────────────────────────────────────
    [Header("Start Position")]
    public Vector3 playerStartPosition = new Vector3(0f, 1f, 0f); // Set this just above platform_0 in the Inspector
    private float score;
    private float highScore;

    // ─────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        highScore = PlayerPrefs.GetFloat("HighScore", 0f);
    }

    void Start()
    {
        ShowMenu();
    }

    void Update()
    {
        if (State != GameState.Playing) return;

        // Score = units climbed above start position × 10
        float height = player.transform.position.y - playerStartPosition.y;
        if (height > score) score = height;

        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(score * 10);
    }

    // ─── Public Buttons ──────────────────────────────────────
    
    /// Called by the "Play" button on the main menu
    public void StartGame()
    {
        score = 0f;
        State = GameState.Playing;

        // Activate player first so its Awake/Start can run
        player.gameObject.SetActive(true);

        // Reset platforms starting from just below the player start
        platformSpawner.ResetSpawner(playerStartPosition.y);

        // Reset camera
        cameraFollow.ResetCamera(playerStartPosition.y);

        // Set the hard floor at platform_0's Y so the player can never fall through it.
        // platform_0 is always spawned at playerStartPosition.y - 0.3f (see PlatformSpawner).
        player.SetFloorY(playerStartPosition.y - 0.3f);

        // Reset player position and give initial jump
        player.ResetPlayer(playerStartPosition);

        SetPanels(hud: true, menu: false, gameOver: false);
    }

    /// Called by Player when it falls off the bottom of the screen
    public void GameOver()
    {
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;

        // Ensure we capture any last-frame height (player may have triggered GameOver before GameManager.Update)
        float height = player.transform.position.y - playerStartPosition.y;
        if (height > score) score = height;

        // Save high score
        int finalScoreInt = Mathf.FloorToInt(score * 10);
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetFloat("HighScore", highScore);
            PlayerPrefs.Save();
        }
        int highScoreInt = Mathf.FloorToInt(highScore * 10);

        if (finalScoreText  != null) finalScoreText.text  = "Score: "  + finalScoreInt;
        if (highScoreText   != null) highScoreText.text   = "Best: "   + highScoreInt;

        SetPanels(hud: false, menu: false, gameOver: true);

        player.gameObject.SetActive(false);
    }

    /// Called by the "Retry" button on the game over screen
    public void RetryGame()
    {
        StartGame();
    }

    /// Called by the "Main Menu" button on the game over screen
    public void ShowMenu()
    {
        State = GameState.Menu;
        player.gameObject.SetActive(false);

        // Snap camera to start position immediately so there's no glitch when Play is pressed
        if (cameraFollow != null)
            cameraFollow.ResetCamera(playerStartPosition.y);

        if (menuHighScoreText != null)
            menuHighScoreText.text = "Best: " + Mathf.FloorToInt(highScore * 10);

        SetPanels(hud: false, menu: true, gameOver: false);
    }

    // ─── Helpers ─────────────────────────────────────────────
    void SetPanels(bool hud, bool menu, bool gameOver)
    {
        if (menuPanel     != null) menuPanel.SetActive(menu);
        if (hudPanel      != null) hudPanel.SetActive(hud);
        if (gameOverPanel != null) gameOverPanel.SetActive(gameOver);
    }
}

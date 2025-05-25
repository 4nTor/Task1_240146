using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("Pause Menu UI")]
    public GameObject pauseMenuUI;

    [Header("Health UI")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public GameObject heartUIContainer;

    [Header("Game Over UI")]
    public GameObject gameOverUI;
    public Button restartButton;
    public Button quitButton;
    public TMPro.TextMeshProUGUI gameOverTitle;
    public TMPro.TextMeshProUGUI deathMessage;

    [Header("Victory UI")]
    public GameObject victoryUI;
    public Button victoryRestartButton;
    public Button victoryQuitButton;
    public TMPro.TextMeshProUGUI victoryTitle;

    [Header("Game Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Audio (Optional)")]
    public AudioClip gameOverSound;
    private AudioSource audioSource;

    [Header("Statistics (Optional)")]
    public TMPro.TextMeshProUGUI enemiesKilledText;
    public TMPro.TextMeshProUGUI timePlayedText;
    public int enemiesKilled = 0;
    private float timePlayed = 0f;
    private float gameStartTime;

    private bool gameOver = false;
    private bool gamePaused = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (!gameOver && !gamePaused)
        {
            timePlayed = Time.time - gameStartTime;
        }
    }

    private void InitializeGame()
    {
        currentHealth = maxHealth;
        gameOver = false;
        gamePaused = false;
        gameStartTime = Time.time;
        timePlayed = 0f;
        enemiesKilled = 0;

        if (heartUIContainer != null)
            heartUIContainer.SetActive(true);

        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        if (victoryUI != null)
            victoryUI.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        SetupButtons();

        UpdateHealthUI();
        UpdateStatisticsUI();
    }

    private void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }

        if (victoryRestartButton != null)
        {
            victoryRestartButton.onClick.RemoveAllListeners();
            victoryRestartButton.onClick.AddListener(RestartGame);
        }

        if (victoryQuitButton != null)
        {
            victoryQuitButton.onClick.RemoveAllListeners();
            victoryQuitButton.onClick.AddListener(QuitGame);
        }
    }

    public void TakeDamage(int damage)
    {
        if (gameOver) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    public void Heal(int healAmount)
    {
        if (gameOver) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        Debug.Log($"Player healed {healAmount}. Health: {currentHealth}/{maxHealth}");
    }

    public void AddEnemyKill()
    {
        if (!gameOver)
        {
            enemiesKilled++;
            UpdateStatisticsUI();
            Debug.Log($"Enemy killed! Total: {enemiesKilled}");

            if (enemiesKilled >= 5)
            {
                TriggerVictory();
            }
        }
    }

    public void GameOver()
    {
        if (gameOver) return;

        gameOver = true;
        Debug.Log("=== GAME OVER ===");

        if (gameOverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            UpdateGameOverUI();
        }

        if (heartUIContainer != null)
            heartUIContainer.SetActive(false);

        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void UpdateGameOverUI()
    {
        if (deathMessage != null)
        {
            string[] messages = {
                "You were defeated!",
                "Better luck next time!",
                "The enemies got the best of you!",
                "Don't give up, try again!",
                "You fought bravely!",
                "Skill Issue."
            };
            deathMessage.text = messages[Random.Range(0, messages.Length)];
        }

        UpdateStatisticsUI();
    }

    public void TriggerVictory()
    {
        if (gameOver) return;

        gameOver = true;
        Debug.Log("=== VICTORY ===");

        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (victoryUI != null)
        {
            victoryUI.SetActive(true);
            if (victoryTitle != null)
            {
                victoryTitle.text = "Victory!";
            }
        }

        if (heartUIContainer != null)
            heartUIContainer.SetActive(false);
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Time.timeScale = 1f;
        AudioListener.pause = false;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void PauseGame()
    {
        if (gameOver) return;

        gamePaused = !gamePaused;
        Time.timeScale = gamePaused ? 0f : 1f;
        Debug.Log(gamePaused ? "Game Paused" : "Game Resumed");
        
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(gamePaused);
    }

    private void UpdateHealthUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
            }
        }
    }

    private void UpdateStatisticsUI()
    {
        if (enemiesKilledText != null)
        {
            enemiesKilledText.text = $"Enemies Defeated: {enemiesKilled}";
        }

        if (timePlayedText != null)
        {
            int minutes = Mathf.FloorToInt(timePlayed / 60f);
            int seconds = Mathf.FloorToInt(timePlayed % 60f);
            timePlayedText.text = $"Time Played: {minutes:00}:{seconds:00}";
        }
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    public bool IsGamePaused()
    {
        return gamePaused;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetEnemiesKilled()
    {
        return enemiesKilled;
    }

    public float GetTimePlayed()
    {
        return timePlayed;
    }

    public void OnEnemyDeath()
    {
        AddEnemyKill();
    }
}

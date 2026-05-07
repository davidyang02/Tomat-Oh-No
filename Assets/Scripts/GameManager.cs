using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// GameManager for Tomat-oh No!
/// Handles: loading trivia, managing rounds, boss HP, and win/lose conditions.
/// Boss takes damage on correct answers, heals on wrong answers.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public BulletSpawner bulletSpawner;
    public UILayoutManager uiLayoutManager;
    public AnswerTile[] answerTiles;

    [Header("UI")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI bossHPLabelText;       // "Boss: 8/10"
    public UnityEngine.UI.Image bossHPFillImage;  // fill bar
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject startPanel;
    public TextMeshProUGUI[] optionTexts;

    [Header("Boss Settings")]
    public int bossMaxHP = 10;

    [Header("Settings")]
    public float feedbackDisplayTime = 1.5f;
    public float questionTimeLimit = 12f;

    // ─── Trivia Data ───
    private TriviaQuestion[] questions;
    private int currentQuestionIndex = 0;
    private int bossCurrentHP;
    private bool acceptingAnswers = false;
    private bool gameStarted = false;
    private bool isPaused = false;
    private float questionTimer = 0f;

    // ─── Audio ───
    private AudioClip correctClip;
    private AudioClip wrongClip;
    private AudioClip applauseClip;
    private AudioClip cheerClip;
    private AudioSource bgmSource;

    void Start()
    {
        LoadQuestions();
        correctClip = Resources.Load<AudioClip>("correct02");
        wrongClip = Resources.Load<AudioClip>("wrong02");
        applauseClip = Resources.Load<AudioClip>("applause");
        cheerClip = Resources.Load<AudioClip>("cheer01");

        // Background music
        AudioClip bgmClip = Resources.Load<AudioClip>("monument_music-your-game-comedy-173310");
        if (bgmClip != null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 0.3f;
            bgmSource.playOnAwake = false;
            bgmSource.Play();
        }
        if (player != null)
        {
            player.OnHPChanged += UpdateHPDisplay;
            player.OnPlayerDied += HandleGameOver;
            player.enabled = false; // freeze player until game starts
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        // Show start panel and wire button at runtime
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            Button startBtn = startPanel.GetComponentInChildren<Button>();
            if (startBtn != null) startBtn.onClick.AddListener(StartGame);
        }
    }

    void Update()
    {
        // R key to restart (works anytime)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
            return;
        }

        // P key to pause/resume
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
            return;
        }

        if (isPaused) return;

        if (!acceptingAnswers) return;

        // Countdown timer
        questionTimer -= Time.deltaTime;
        UpdateTimerDisplay();

        if (questionTimer <= 0f)
        {
            // Time's up — penalize and move on
            acceptingAnswers = false;
            TriviaQuestion q = questions[currentQuestionIndex];
            if (q.correctIndex < answerTiles.Length)
                answerTiles[q.correctIndex].ShowCorrect();
            ShowFeedback("Time's Up!", new Color(1f, 0.6f, 0f));

            // Lose a heart + punishment burst
            if (player != null) player.TakeDamage();
            if (bulletSpawner != null) bulletSpawner.SpawnPunishmentBurst();

            currentQuestionIndex++;
            StartCoroutine(NextRoundDelay());
        }
    }

    /// <summary>
    /// Called when the Start button is clicked.
    /// </summary>
    public void StartGame()
    {
        gameStarted = true;
        bossCurrentHP = bossMaxHP;
        if (startPanel != null) startPanel.SetActive(false);
        if (player != null) player.enabled = true;
        UpdateBossHPDisplay();
        StartRound();
    }

    void LoadQuestions()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("questions");
        if (jsonFile != null)
        {
            QuestionList data = JsonUtility.FromJson<QuestionList>(jsonFile.text);
            questions = data.questions;
            ShuffleQuestions();
        }
        else
        {
            Debug.LogWarning("GameManager: questions.json not found in Resources/. Using fallback questions.");
            questions = GetFallbackQuestions();
        }
    }

    void ShuffleQuestions()
    {
        for (int i = questions.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            TriviaQuestion temp = questions[i];
            questions[i] = questions[j];
            questions[j] = temp;
        }
    }

    TriviaQuestion[] GetFallbackQuestions()
    {
        return new TriviaQuestion[]
        {
            new TriviaQuestion
            {
                question = "What is 2 + 2?",
                answers = new string[] { "3", "4", "5", "6" },
                correctIndex = 1
            },
            new TriviaQuestion
            {
                question = "What color is the sky?",
                answers = new string[] { "Green", "Red", "Blue", "Yellow" },
                correctIndex = 2
            },
            new TriviaQuestion
            {
                question = "How many continents are there?",
                answers = new string[] { "5", "6", "7", "8" },
                correctIndex = 2
            }
        };
    }

    /// <summary>
    /// Start the current round: display question, assign answer tiles, enable bullets.
    /// </summary>
    void StartRound()
    {
        if (currentQuestionIndex >= questions.Length)
        {
            currentQuestionIndex = 0; // loop questions
            ShuffleQuestions();
        }

        TriviaQuestion q = questions[currentQuestionIndex];

        // Set question text via UILayoutManager
        if (uiLayoutManager != null)
        {
            uiLayoutManager.SetQuestion(q.question);
        }

        // Update HUD option texts (A-D)
        string[] prefixes = { "A) ", "B) ", "C) ", "D) " };
        for (int i = 0; i < q.answers.Length && i < 4; i++)
        {
            if (optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
            {
                optionTexts[i].text = prefixes[i] + q.answers[i];
            }
        }

        // Assign answers to tiles and randomize positions
        RandomizeTilePositions();
        for (int i = 0; i < answerTiles.Length && i < q.answers.Length; i++)
        {
            answerTiles[i].answerIndex = i;
            answerTiles[i].ResetColor();
            answerTiles[i].gameObject.SetActive(true);
        }

        // Cycle projectile pattern based on question difficulty
        if (bulletSpawner != null)
        {
            BulletSpawner.SpawnPattern[] patterns = {
                BulletSpawner.SpawnPattern.Columns,
                BulletSpawner.SpawnPattern.Diagonal,
                BulletSpawner.SpawnPattern.Spiral,
                BulletSpawner.SpawnPattern.Random
            };
            int patternIdx = q.difficulty - 1;
            if (patternIdx < 0) patternIdx = 0;
            if (patternIdx >= patterns.Length) patternIdx = patterns.Length - 1;
            bulletSpawner.SetPattern(patterns[patternIdx]);
            bulletSpawner.SetActive(true);
        }

        // Reset timer
        questionTimer = questionTimeLimit;
        UpdateTimerDisplay();

        acceptingAnswers = true;
        UpdateBossHPDisplay();
        UpdateHPDisplay(player != null ? player.currentHP : 0);
    }

    /// <summary>
    /// Called by AnswerTile when the player touches it.
    /// </summary>
    public void OnAnswerSelected(int answerIndex)
    {
        if (!acceptingAnswers) return;
        acceptingAnswers = false;

        TriviaQuestion q = questions[currentQuestionIndex];
        bool correct = answerIndex == q.correctIndex;

        if (correct)
        {
            answerTiles[answerIndex].ShowCorrect();
            // Deal damage to the boss
            bossCurrentHP = Mathf.Max(0, bossCurrentHP - 1);
            UpdateBossHPDisplay();
            ShowFeedback("Correct! Boss takes damage!", Color.green);
            if (correctClip != null) AudioSource.PlayClipAtPoint(correctClip, Camera.main.transform.position, 0.8f);
            // Splat on audience!
            if (SplatEffect.Instance != null) SplatEffect.Instance.SpawnSplat();
            // Heal player heart (up to max 5)
            if (player != null) player.Heal();

            // Check for victory
            if (bossCurrentHP <= 0)
            {
                HandleVictory();
                return;
            }
        }
        else
        {
            answerTiles[answerIndex].ShowWrong();
            if (q.correctIndex < answerTiles.Length)
            {
                answerTiles[q.correctIndex].ShowCorrect();
            }
            ShowFeedback("Wrong! Answer: " + q.answers[q.correctIndex], Color.red);
            if (wrongClip != null) AudioSource.PlayClipAtPoint(wrongClip, Camera.main.transform.position, 0.8f);

            // Lose a heart + punishment burst
            if (player != null) player.TakeDamage();
            if (bulletSpawner != null) bulletSpawner.SpawnPunishmentBurst();
        }

        currentQuestionIndex++;
        StartCoroutine(NextRoundDelay());
    }

    IEnumerator NextRoundDelay()
    {
        // Pause briefly to show feedback
        yield return new WaitForSeconds(feedbackDisplayTime);

        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        StartRound();
    }

    void ShowFeedback(string text, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = text;
            feedbackText.color = color;
            feedbackText.gameObject.SetActive(true);
        }
    }

    void UpdateBossHPDisplay()
    {
        if (bossHPLabelText != null)
        {
            bossHPLabelText.text = "Boss: " + bossCurrentHP + "/" + bossMaxHP;
        }
        if (bossHPFillImage != null)
        {
            bossHPFillImage.fillAmount = (float)bossCurrentHP / bossMaxHP;
        }
    }

    void HandleVictory()
    {
        acceptingAnswers = false;
        gameStarted = false;
        StopAllCoroutines();

        if (bulletSpawner != null) bulletSpawner.SetActive(false);
        if (player != null) player.enabled = false;

        // Destroy all remaining bullets
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (Bullet b in bullets) Destroy(b.gameObject);

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            TextMeshProUGUI vicText = victoryPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (vicText != null)
            {
                vicText.text = "YOU WIN!\nBoss Defeated!\n\nPress [R] to Play Again";
            }
        }

        // Play applause + cheer together
        Vector3 camPos = Camera.main.transform.position;
        if (applauseClip != null) AudioSource.PlayClipAtPoint(applauseClip, camPos, 0.9f);
        if (cheerClip != null) AudioSource.PlayClipAtPoint(cheerClip, camPos, 0.8f);

        // Clear audience splats
        if (SplatEffect.Instance != null) SplatEffect.Instance.ClearSplats();
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(Mathf.Max(0, questionTimer));
            timerText.text = seconds.ToString() + "s";
            // Color changes: green > 6s, yellow 3-6s, red < 3s
            if (questionTimer > 6f)
                timerText.color = Color.white;
            else if (questionTimer > 3f)
                timerText.color = new Color(1f, 0.8f, 0f); // yellow
            else
                timerText.color = Color.red;
        }
    }

    void UpdateHPDisplay(int hp)
    {
        if (hpText != null)
        {
            string hearts = "";
            for (int i = 0; i < hp; i++) hearts += "♥ ";
            hpText.text = hearts.TrimEnd();
        }
    }

    void HandleGameOver()
    {
        acceptingAnswers = false;
        gameStarted = false;
        StopAllCoroutines(); // cancel any pending NextRoundDelay

        if (bulletSpawner != null) bulletSpawner.SetActive(false);
        if (player != null) player.enabled = false;

        // Destroy all remaining bullets
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (Bullet b in bullets) Destroy(b.gameObject);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            TextMeshProUGUI goText = gameOverPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (goText != null)
            {
                goText.text = "GAME OVER\nBoss HP: " + bossCurrentHP + "/" + bossMaxHP + "\n\nPress [R] to Restart";
            }
        }

        // Clear audience splats
        if (SplatEffect.Instance != null) SplatEffect.Instance.ClearSplats();
    }

    /// <summary>
    /// Reset the game (called by ModeSwitcher on R key).
    /// </summary>
    public void ResetGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        bossCurrentHP = bossMaxHP;
        currentQuestionIndex = 0;
        ShuffleQuestions();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        if (player != null)
        {
            player.ResetPlayer();
            player.enabled = true;
        }
        gameStarted = true;
        UpdateBossHPDisplay();
        StartRound();

        // Clear audience splats
        if (SplatEffect.Instance != null) SplatEffect.Instance.ClearSplats();
    }

    public void TogglePause()
    {
        if (!gameStarted) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            ShowFeedback("PAUSED — Press [P] to Resume", Color.yellow);
        }
        else
        {
            if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Randomize tile positions each round from a pool of valid arena locations.
    /// </summary>
    void RandomizeTilePositions()
    {
        if (answerTiles == null || answerTiles.Length < 4) return;

        // Pool of valid positions (spread across arena, avoiding center)
        Vector3[] positionPool = new Vector3[]
        {
            new Vector3(-5f, 3f, 0f),    // top left
            new Vector3(5f, 3f, 0f),     // top right
            new Vector3(-5f, -3f, 0f),   // bottom left
            new Vector3(5f, -3f, 0f),    // bottom right
            new Vector3(-6f, 0f, 0f),    // mid left
            new Vector3(6f, 0f, 0f),     // mid right
            new Vector3(-3f, 4f, 0f),    // upper mid-left
            new Vector3(3f, 4f, 0f),     // upper mid-right
            new Vector3(-3f, -4f, 0f),   // lower mid-left
            new Vector3(3f, -4f, 0f),    // lower mid-right
            new Vector3(0f, 4f, 0f),     // top center
            new Vector3(0f, -4f, 0f)     // bottom center
        };

        // Shuffle the pool
        for (int i = positionPool.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector3 temp = positionPool[i];
            positionPool[i] = positionPool[j];
            positionPool[j] = temp;
        }

        // Assign first 4 positions to tiles
        for (int i = 0; i < answerTiles.Length && i < 4; i++)
        {
            answerTiles[i].transform.position = positionPool[i];
        }
    }
}

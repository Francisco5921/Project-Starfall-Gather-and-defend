using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverPanel;
    private bool isPaused = false;
    public static bool isGameActive = true;

    [SerializeField] private AudioClip backgroundMusic;
    private AudioSource audioSource;

    public TextMeshProUGUI finalDaysText; // Text for how many days lasted
    public TextMeshProUGUI gameOverText; // Text for winning or losing

    // private readonly DayNightCycle playerHUD;
    private GameObject playerBase;
    private PlayerController playerController;

    [SerializeField] private EventSystem eventSystem;


    private void Start()
    {
        playerBase = GameObject.Find("PlayerBase");
        playerController = FindObjectOfType<PlayerController>();

        pauseMenu.SetActive(false);
        gameOverPanel.SetActive(false);
        ResumeGameAtStart();
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.volume = 0.2f;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void Update()
    {
        PauseInput();
        CheckBaseHealth();
        CheckWinCondition();
    }

    private void PauseInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    private void CheckBaseHealth()
    {
        if (playerBase == null)
        {
            GameOver(false); // Lose condition
        }
    }

    private void CheckWinCondition()
    {
        if (playerController != null)
        {
            int totalResources = playerController.GetTotalResources();
            if (totalResources >= 400)
            {
                GameOver(true); // Win condition
            }
        }
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isGameActive = false;
        isPaused = true;
        UnlockCursor();
        if (audioSource != null)
        {
            audioSource.Pause();
        }

        // Deselect any selected UI element
        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isGameActive = true;
        isPaused = false;
        LockCursor();
        if (audioSource != null)
        {
            audioSource.UnPause();
        }

        // Deselect any selected UI element
        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(null);
        }

    }

    public void Reset()
    {
        gameOverPanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void GameOver(bool isWin)
    {
       // playerHUD.HideHUD();
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        isGameActive = false;
        UnlockCursor();
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Display the appropriate game-over message
        if (isWin)
        {
            gameOverText.text = "YOU WIN!";
        }
        else
        {
            gameOverText.text = "YOU LOSE!";
        }

        DayNightCycle dayNightCycle = FindObjectOfType<DayNightCycle>(); // Find the DayNightCycle component in the scene
        if (dayNightCycle != null)
        {
            int daysSurvived = dayNightCycle.daySurvived; // Access the daySurvived variable
            finalDaysText.text = "DAYS LASTED: " + daysSurvived; // Update the TMP text
        }

    }

    private void ResumeGameAtStart()
    {
      //  playerHUD.ShowHUD();
        Time.timeScale = 1f;
        isGameActive = true;
        LockCursor();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
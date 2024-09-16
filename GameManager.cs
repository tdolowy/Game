using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Text opponentDefeatedText;
    public Text youLoseText;
    public TurnManager turnManager;
    public PlayerController playerController;
    public AIController aiController;

    public GameObject winPanel;
    public GameObject losePanel;
    public Button restartButton;

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        if (aiController == null) aiController = FindObjectOfType<AIController>();

        if (turnManager == null || playerController == null || aiController == null)
        {
            Debug.LogError("GameManager: Missing required components. Please check the scene.");
        }
        else
        {
            turnManager.StartPlayerTurn();
        }

        restartButton.onClick.AddListener(RestartGame);
    }

    void Update()
    {
        if (!isGameOver)
        {
            CheckHealth();
        }
    }

    private void CheckHealth()
    {
        if (playerController.health <= 0)
        {
            GameOver(false); // Player lost
        }
        else if (aiController.GetHealth() <= 0)
        {
            GameOver(true); // Player won
        }
    }

    private void GameOver(bool playerWon)
    {
        isGameOver = true;

        if (playerWon)
        {
            winPanel.SetActive(true);
            restartButton.gameObject.SetActive(true);
            Debug.Log("Player Won!");
            opponentDefeatedText.text = "Opponent Defeated!";

        }
        else
        {
            losePanel.SetActive(true);
            restartButton.gameObject.SetActive(true);
            Debug.Log("Player Lost!");
            youLoseText.text = "You Lose!";
        }

        DisableGameplay();
    }

    private void DisableGameplay()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (aiController != null)
        {
            aiController.enabled = false;
        }

        if (turnManager != null)
        {
            turnManager.enabled = false;
        }

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            enemy.enabled = false;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

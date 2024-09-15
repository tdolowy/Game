using UnityEngine;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour
{
    public PlayerController playerController; // Assign this in the Inspector or find it in Start()
    public Slider healthSlider;
    public Text healthText;

    void Start()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController not assigned!");
            return;
        }

        // Initialize the UI
        UpdateHealthUI();
    }

    void Update()
    {
        // Continuously update the UI in case of changes
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthSlider == null || healthText == null)
        {
            Debug.LogError("Health UI elements are not assigned!");
            return;
        }

        // Update the health slider value and health text
        healthSlider.value = (float)playerController.health;
        healthText.text = $"Health: {playerController.health}";
    }
}
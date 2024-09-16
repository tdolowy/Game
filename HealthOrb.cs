using UnityEngine;
using UnityEngine.UI;

public class HealthOrbUI : MonoBehaviour
{
    public PlayerController playerController; // Reference to the PlayerController
    public Image healthOrb; // Reference to the health orb image
    public Text healthText; // Reference to the health number text

    void Start()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController not assigned!");
            return;
        }

        if (healthOrb == null)
        {
            Debug.LogError("Health Orb Image is not assigned!");
            return;
        }

        if (healthText == null)
        {
            Debug.LogError("Health Text UI is not assigned!");
            return;
        }

        UpdateHealthUI();
    }

    void Update()
    {
        // Continuously update the orb and text in case of health changes
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthOrb == null || playerController == null || healthText == null)
            return;

        // Calculate fill amount based on health percentage
        float healthPercentage = (float)playerController.health / 500f; // Assuming max health is 100
        healthOrb.fillAmount = healthPercentage;

        // Update health text
        healthText.text = $"{playerController.health}";
    }
}
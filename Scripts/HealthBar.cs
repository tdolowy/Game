using UnityEngine;
using System.Collections; // Required for Coroutines
using TMPro; // Include TextMeshPro namespace

public class HealthBar : MonoBehaviour
{
    public RectTransform foregroundPanel; // RectTransform of the foreground panel
    public TextMeshProUGUI healthText; // Reference to the TextMeshPro component
    private Enemy enemy;
    private float initialWidth; // Store the initial width of the foreground panel

    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
        if (enemy != null && foregroundPanel != null)
        {
            StartCoroutine(InitializeHealthBar());
        }
    }

    IEnumerator InitializeHealthBar()
    {
        // Wait until the end of the frame to ensure the UI layout has been set up
        yield return new WaitForEndOfFrame();

        // Store the initial width of the foreground panel
        initialWidth = 95F;
        Debug.Log($"Initial Foreground Width: {initialWidth}");

        // Initialize to full health (100%)
        SetHealth(1f);
    }

    void Update()
    {
        if (enemy != null)
        {
            UpdateHealthBar();
        }
    }

    public void SetHealth(float healthPercentage)
    {
        if (foregroundPanel != null && initialWidth > 0)
        {
            // Calculate the new width based on health percentage
            float newWidth = initialWidth * Mathf.Clamp01(healthPercentage);
          //  Debug.Log($"Setting Health: {healthPercentage}, New Width: {newWidth}");

            // Update the width of the foreground panel without changing its position
            foregroundPanel.sizeDelta = new Vector2(newWidth, foregroundPanel.sizeDelta.y);
        }

        // Update the health text if it exists
        if (healthText != null && enemy != null)
        {
            healthText.text = $"{enemy.health}"; // Set the text to display the current health
        }
    }

    void UpdateHealthBar()
    {
        if (enemy != null && foregroundPanel != null)
        {
            float healthPercentage = (float)enemy.health / (float)enemy.maxHealth; // Calculate health percentage
            // Debug.Log($"Health Percentage: {healthPercentage}, Health: {enemy.health}, Max Health: {enemy.maxHealth}");
            SetHealth(healthPercentage); // Update health bar
        }
    }
}
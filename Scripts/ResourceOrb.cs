using UnityEngine;
using UnityEngine.UI;

public class ResourceOrbUI : MonoBehaviour
{
    public PlayerController playerController; // Reference to the PlayerController
    public Image resourceOrb; // Reference to the resource orb image
    public Text resourceText; // Reference to the resource number text

    void Start()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController not assigned!");
            return;
        }

        if (resourceOrb == null)
        {
            Debug.LogError("Resource Orb Image is not assigned!");
            return;
        }

        if (resourceText == null)
        {
            Debug.LogError("Resource Text UI is not assigned!");
            return;
        }

        UpdateResourceUI();
    }

    void Update()
    {
        // Continuously update the orb and text in case of resource changes
        UpdateResourceUI();
    }

   public void UpdateResourceUI()
    {
        if (resourceOrb == null || playerController == null || resourceText == null)
            return;

        // Calculate fill amount based on resource percentage
        float resourcePercentage = (float)playerController.resource / 100f; // Assuming max resource is 100
        resourceOrb.fillAmount = resourcePercentage;

        // Update resource text
        resourceText.text = $"{playerController.resource}";
    }
}
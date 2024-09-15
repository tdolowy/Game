using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public PlayerController playerController; // Reference to the PlayerController
    public TargetingSystem targetingSystem; // Reference to the TargetingSystem script
    public CardManager cardManager; // Reference to the CardManager


    void OnSpellButtonClicked(string spellName)
    {
        if (targetingSystem != null)
        {
            Transform target = targetingSystem.GetCurrentTarget(); // Get the selected target
            if (cardManager != null && target != null)
            {
                // Set the target in the CardManager and initiate the spell
                cardManager.SetSelectedTarget(target);
                cardManager.Use(spellName); // Use the selected spell
            }
        }
    }
}

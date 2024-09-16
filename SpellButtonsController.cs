using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpellButtonsController : MonoBehaviour
{
    public SpellButtonManager[] spellButtons;
    public CardManager cardManager;
    public MoveToPosition moveToPosition;
    public Toggle handOfCardsToggle;
    public Toggle AttackToggleHud;
    private SpellManager spellManager;

    private void Start()
    {
        spellManager = FindObjectOfType<SpellManager>();
        if (spellManager == null)
        {
            Debug.LogError("SpellManager not found in the scene.");
        }

        if (spellButtons == null || spellButtons.Length == 0)
        {
            Debug.LogError("No SpellButtonManagers assigned to SpellButtonsController.");
            return;
        }

        // Initially hide all buttons
        foreach (var button in spellButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Null SpellButtonManager found in SpellButtonsController.");
            }
        }
    }

    public void UnlockSpell(string spellName)
    {
        if (spellManager == null)
        {
            Debug.LogError("SpellManager is null in SpellButtonsController.");
            return;
        }

        for (int i = 0; i < spellButtons.Length; i++)
        {
            if (spellButtons[i] != null && !spellButtons[i].gameObject.activeSelf)
            {
                SpellDefinition spellDef = spellManager.GetSpellDefinition(spellName);
                if (spellDef != null)
                {
                    spellButtons[i].gameObject.SetActive(true);
                    spellButtons[i].spellName = spellName;
                    spellButtons[i].cardManager = cardManager;
                    spellButtons[i].moveToPosition = moveToPosition;
                    spellButtons[i].handOfCardsToggle = handOfCardsToggle;
                    spellButtons[i].AttackToggleHud = AttackToggleHud;

                    // Set button text and color
                    Text buttonText = spellButtons[i].GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = spellName;
                    }
                    Image buttonImage = spellButtons[i].GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        buttonImage.color = spellDef.spellColor;
                    }
                }
                break;
            }
        }
    }
}

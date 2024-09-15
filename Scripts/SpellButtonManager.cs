using UnityEngine;
using UnityEngine.UI;

public class SpellButtonManager : MonoBehaviour
{
    public CardManager cardManager;
    public MoveToPosition moveToPosition;
    public Toggle handOfCardsToggle;
    public Toggle AttackToggleHud;
    public string spellName;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnSpellButtonClick);
        }
        else
        {
            Debug.LogError("Button component not found on this GameObject.");
        }

        if (cardManager != null)
        {
            cardManager.OnPlayerReturned += OnPlayerReturned;
        }
        else
        {
            Debug.LogError("CardManager is not assigned to SpellButtonManager.");
        }
    }

    void OnSpellButtonClick()
    {
        if (cardManager.Use(spellName))
        {
            // Spell was cast successfully, perform additional actions
            if (moveToPosition != null)
            {
                moveToPosition.MoveToFixedPosition();
            }
            if (handOfCardsToggle != null)
            {
                handOfCardsToggle.isOn = !handOfCardsToggle.isOn;
            }
            if (AttackToggleHud != null)
            {
                AttackToggleHud.isOn = false;
                Debug.Log("AttackToggleHud turned off.");
            }
        }
    }

    private void OnPlayerReturned()
    {
        if (AttackToggleHud != null)
        {
            AttackToggleHud.isOn = true;
            Debug.Log("Player returned to original position. AttackToggleHud turned on.");
        }
    }

    private void OnDestroy()
    {
        if (cardManager != null)
        {
            cardManager.OnPlayerReturned -= OnPlayerReturned;
        }
    }
}

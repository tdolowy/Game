using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopManager : MonoBehaviour
{
    public GameObject player; // Reference to the player GameObject
    public GameObject shopPanel; // Reference to the shop panel UI
    public Text tooltipText;
    public RectTransform tooltipPanel;
    public Button healthPotionButton;

    private XPManager xpManager;
    private PlayerController playerController;

    void Start()
    {
        if (player != null)
        {
            xpManager = player.GetComponent<XPManager>();
            playerController = player.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogError("Player GameObject not assigned.");
        }

        if (shopPanel == null)
        {
            Debug.LogError("Shop panel not assigned.");
        }

        SetupTooltipTrigger();
    }

    private void SetupTooltipTrigger()
    {
        if (healthPotionButton != null)
        {
            EventTrigger trigger = healthPotionButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { ShowHealthPotionTooltip(); });
            trigger.triggers.Add(enterEntry);

            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { HideTooltip(); });
            trigger.triggers.Add(exitEntry);
        }
    }

    public void ToggleShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(!shopPanel.activeSelf);
        }
        else
        {
            Debug.LogError("Shop panel is not assigned.");
        }
    }

    public void BuyHealthPotion()
    {
        if (xpManager != null && xpManager.gold >= 50)
        {
            xpManager.gold -= 50;
            if (playerController != null)
            {
                if (playerController.health + 50 > 500)
                {
                    playerController.health = 500;
                }
                else
                {
                    playerController.health += 50;
                }
                Debug.Log("Health restored! Current health: " + playerController.health);
            }
            Debug.Log("Health potion bought!");
        }
        else
        {
            Debug.Log("Not enough gold!");
        }
    }

    private void ShowHealthPotionTooltip()
    {
        string tooltipText = "Health Potion\nRestores 50 HP\nCost: 50 Gold";
        if (xpManager != null && xpManager.gold < 50)
        {
            tooltipText += "\nNot enough gold!";
        }
        ShowTooltip(tooltipText);
    }

    private void ShowTooltip(string text)
    {
        if (tooltipText != null && tooltipPanel != null)
        {
            tooltipText.text = text;
            tooltipPanel.gameObject.SetActive(true);
            PositionTooltipAtMouse();
        }
        else
        {
            Debug.LogError("Tooltip Text or Panel is null");
        }
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(false);
        }
    }

    private void PositionTooltipAtMouse()
    {
        if (tooltipPanel != null)
        {
            Vector2 mousePosition = Input.mousePosition;
            Canvas canvas = tooltipPanel.GetComponentInParent<Canvas>();

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    mousePosition,
                    canvas.worldCamera,
                    out Vector2 localPoint
                );
                tooltipPanel.localPosition = localPoint;
            }
            else
            {
                tooltipPanel.position = mousePosition;
            }

            Vector2 offset = new Vector2(10, 10);
            tooltipPanel.anchoredPosition += offset;

            Vector3 tooltipPos = tooltipPanel.position;
            tooltipPos.x = Mathf.Clamp(tooltipPos.x, 0, Screen.width - tooltipPanel.rect.width);
            tooltipPos.y = Mathf.Clamp(tooltipPos.y, 0, Screen.height - tooltipPanel.rect.height);
            tooltipPanel.position = tooltipPos;
        }
    }
}

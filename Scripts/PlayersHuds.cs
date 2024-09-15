using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayersHuds : MonoBehaviour
{
    public XPManager playerXPManager;
    public Slider xpBarSlider;
    public Text xpText;
    public Text goldText;
    public Text tooltipText;
    public RectTransform tooltipPanel;

    void Start()
    {
        if (playerXPManager == null)
        {
            Debug.LogError("XPManager not assigned!");
            return;
        }

        UpdateXPUI();
        UpdateGoldUI();
        SetupXPSliderTooltip();
    }

    void Update()
    {
        UpdateXPUI();
        UpdateGoldUI();
    }

    void UpdateXPUI()
    {
        if (xpBarSlider == null || xpText == null)
        {
            Debug.LogError("XP UI elements are not assigned!");
            return;
        }

        float xpPercentage = (float)playerXPManager.xp / playerXPManager.xpToNextLevel;
        xpBarSlider.value = xpPercentage;
        xpText.text = $"XP: {playerXPManager.xp} / {playerXPManager.xpToNextLevel}";
    }

    void UpdateGoldUI()
    {
        if (goldText == null)
        {
            Debug.LogError("Gold Text is not assigned!");
            return;
        }

        goldText.text = $"Gold: {playerXPManager.gold}";
    }

    void SetupXPSliderTooltip()
    {
        if (xpBarSlider != null)
        {
            EventTrigger trigger = xpBarSlider.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { ShowTooltip("Level up to\ngain a skill point"); });
            trigger.triggers.Add(enterEntry);

            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { HideTooltip(); });
            trigger.triggers.Add(exitEntry);
        }
    }

    void ShowTooltip(string text)
    {
        if (tooltipText != null && tooltipPanel != null)
        {
            tooltipText.text = text;
            tooltipPanel.gameObject.SetActive(true);
            PositionTooltipAtMouse();
        }
    }

    void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(false);
        }
    }

    void PositionTooltipAtMouse()
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

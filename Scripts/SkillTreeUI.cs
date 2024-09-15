using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SkillTreeUI : MonoBehaviour
{
    public GameObject skillTreePanel;
    public Text skillPointsText;
    public Button fireballButton;
    public Button iceBoltButton;
    public Button eruptionButton;
    public Button cryoblastButton;
    public Text tooltipText;
    public RectTransform tooltipPanel;
    public Toggle talentsToggle;
    private XPManager xpManager;
    private CardManager cardManager;
    private SpellButtonsController spellButtonsController;
    private SpellManager spellManager;
    private HashSet<string> defaultUnlockedSkills = new HashSet<string> { "BasicAttack" };

    private Dictionary<string, List<string>> skillPrerequisites = new Dictionary<string, List<string>>
    {
        { "Eruption", new List<string> { "Fireball", "Cryoblast" } },
        { "Cryoblast", new List<string> { "Ice Bolt", "Eruption" } }
    };

    private Dictionary<string, Button> skillButtons = new Dictionary<string, Button>();

    private void Start()
    {
        SetupTalentsToggleTooltip();
        Debug.Log("SkillTreeUI Start called");
        InitializeComponents();
        SetupSkillButtons();
        SetupPrerequisiteTooltips();
        Debug.Log("About to call UpdateUI from Start");
        UpdateUI();
        Debug.Log("UpdateUI called from Start completed");
    }

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.gameObject.activeSelf)
        {
            PositionTooltipAtMouse();
        }
    }

    private void InitializeComponents()
    {
        xpManager = FindObjectOfType<XPManager>();
        cardManager = FindObjectOfType<CardManager>();
        spellButtonsController = FindObjectOfType<SpellButtonsController>();
        spellManager = FindObjectOfType<SpellManager>();

        if (xpManager == null)
            Debug.LogError("XPManager not found in the scene.");
        if (cardManager == null)
            Debug.LogError("CardManager not found in the scene.");
        if (spellButtonsController == null)
            Debug.LogError("SpellButtonsController not found in the scene.");
        if (spellManager == null)
            Debug.LogError("SpellManager not found in the scene.");

        if (cardManager != null)
        {
            cardManager.EnsureSpellsInitialized();
        }

        if (xpManager != null)
        {
            xpManager.OnLevelUp += OnSkillPointsChanged;
        }
    }

    private void SetupSkillButtons()
    {
        AddSkillButton("Fireball", fireballButton);
        AddSkillButton("Ice Bolt", iceBoltButton);
        AddSkillButton("Eruption", eruptionButton);
        AddSkillButton("Cryoblast", cryoblastButton);

        foreach (var kvp in skillButtons)
        {
            string skillName = kvp.Key;
            Button button = kvp.Value;
            button.onClick.AddListener(() => UnlockOrUpgradeSkill(skillName));
        }
    }

    private void AddSkillButton(string skillName, Button button)
    {
        skillButtons[skillName] = button;
    }

    private void SetupPrerequisiteTooltips()
    {
        foreach (var kvp in skillPrerequisites)
        {
            string skillName = kvp.Key;
            if (skillButtons.TryGetValue(skillName, out Button button))
            {
                SetupPrerequisiteTooltip(button, skillName);
            }
        }
    }

    private void SetupPrerequisiteTooltip(Button button, string skillName)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { ShowPrerequisites(skillName); });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { HideTooltip(); });
        trigger.triggers.Add(exitEntry);
    }

    private void OnDestroy()
    {
        if (xpManager != null)
        {
            xpManager.OnLevelUp -= OnSkillPointsChanged;
        }
    }

    private bool CanUnlockSkill(string skillName)
    {
        if (!skillPrerequisites.ContainsKey(skillName))
        {
            return true;
        }

        foreach (string prerequisite in skillPrerequisites[skillName])
        {
            if (xpManager.GetSkillLevel(prerequisite) > 0)
            {
                return true;
            }
        }

        return false;
    }

    private void UnlockOrUpgradeSkill(string skillName)
    {
        Debug.Log($"Attempting to unlock or upgrade skill: {skillName}");
        if (defaultUnlockedSkills.Contains(skillName))
        {
            Debug.Log($"Attempted to upgrade {skillName}, which is a default unlocked skill");
            return;
        }

        if (xpManager == null || cardManager == null || spellButtonsController == null)
        {
            Debug.LogError("XPManager, CardManager, or SpellButtonsController is not assigned in SkillTreeUI.");
            return;
        }

        int currentLevel = xpManager.GetSkillLevel(skillName);
        int cost = 1;

        if (currentLevel == 0 && !CanUnlockSkill(skillName))
        {
            Debug.Log($"Cannot unlock {skillName}. Prerequisites not met.");
            return;
        }

        if (xpManager.skillPoints >= cost && xpManager.UseSkillPoints(cost))
        {
            int newLevel = currentLevel + 1;
            xpManager.UnlockSkill(skillName, newLevel);

            if (spellManager != null)
            {
                SpellDefinition spellDef = spellManager.GetSpellDefinition(skillName);
                if (spellDef != null)
                {
                    int newDamage = spellDef.GetDamageForLevel(newLevel);
                    int newResourceCost = spellDef.GetManaCostForLevel(newLevel);

                    cardManager.UpdateSpell(skillName, newLevel, newDamage, newResourceCost);
                }
            }

            if (newLevel == 1)
            {
                spellButtonsController.UnlockSpell(skillName);
            }

            Debug.Log($"Skill {skillName} unlocked/upgraded to level {newLevel}");
            UpdateUI();
        }
        else
        {
            Debug.Log($"Cannot unlock/upgrade skill {skillName}. Not enough skill points or other conditions not met.");
        }
    }

    private void UpdateUI()
    {
        UpdateSkillPointsText(xpManager.level, xpManager.skillPoints);
        UpdateSkillButtons();
    }

    private void UpdateSkillPointsText(int level, int skillPoints)
    {
        if (skillPointsText != null)
        {
            skillPointsText.text = $"Skill Points: {skillPoints}";
        }
        else
        {
            Debug.LogWarning("Skill Points Text is not assigned in the SkillTreeUI.");
        }
    }

    private void UpdateSkillButtons()
    {
        Debug.Log($"UpdateSkillButtons called. Number of skill buttons: {skillButtons.Count}");
        foreach (var kvp in skillButtons)
        {
            Debug.Log($"Updating button for skill: {kvp.Key}");
            UpdateSkillButton(kvp.Value, kvp.Key);
        }
    }

    private void UpdateSkillButton(Button button, string skillName)
    {
        Debug.Log($"UpdateSkillButton called for {skillName}");
        if (button == null)
        {
            Debug.LogError($"Button is null for skill {skillName}");
            return;
        }
        if (xpManager == null)
        {
            Debug.LogError($"xpManager is null in UpdateSkillButton for {skillName}");
            return;
        }
        if (spellManager == null)
        {
            Debug.LogError($"spellManager is null in UpdateSkillButton for {skillName}");
            return;
        }

        int level = xpManager.GetSkillLevel(skillName);
        bool isDefaultUnlocked = defaultUnlockedSkills.Contains(skillName);
        bool canUnlock = isDefaultUnlocked || level > 0 || CanUnlockSkill(skillName);
        button.interactable = !isDefaultUnlocked && xpManager.skillPoints >= 1 && canUnlock;

        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null && spellManager != null)
        {
            SpellDefinition spellDef = spellManager.GetSpellDefinition(skillName);
            if (spellDef != null)
            {
                int currentDamage = spellDef.GetDamageForLevel(level);
                int currentManaCost = spellDef.GetManaCostForLevel(level);
                int nextLevelDamage = spellDef.GetDamageForLevel(level + 1);
                int nextLevelManaCost = spellDef.GetManaCostForLevel(level + 1);

                if (isDefaultUnlocked)
                {
                    buttonText.text = $"{skillName}\n(Default)\nDamage: {currentDamage}\nMana: {currentManaCost}";
                }
                else if (level == 0)
                {
                    buttonText.text = $"Unlock\n{skillName}\nDamage\n{nextLevelDamage}\nMana\n{nextLevelManaCost}";
                    if (!canUnlock)
                    {
                        buttonText.text += "\n(Locked)";
                    }
                }
                else
                {
                    buttonText.text = $"Upgrade\n{skillName}\n(Level {level})\nDamage\n{currentDamage} → {nextLevelDamage}\nMana\n{currentManaCost} → {nextLevelManaCost}";
                }
            }
        }
    }

    private void ShowPrerequisites(string skillName)
    {
        if (skillPrerequisites.ContainsKey(skillName) && !CanUnlockSkill(skillName))
        {
            string prereqText = "Requires: " + string.Join("\nor\n", skillPrerequisites[skillName]);
            ShowTooltip(prereqText);
        }
    }

    private void ShowTooltip(string text)
    {
        if (tooltipText != null && tooltipPanel != null)
        {
            tooltipText.text = text;
            tooltipPanel.gameObject.SetActive(true);
            PositionTooltipAtMouse();
            Debug.Log($"Showing tooltip: {text} at position {tooltipPanel.position}");
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

            Vector2 offset = new Vector2(-tooltipPanel.rect.width, -tooltipPanel.rect.height);
            tooltipPanel.anchoredPosition += offset;

            Vector3 tooltipPos = tooltipPanel.position;
            tooltipPos.x = Mathf.Clamp(tooltipPos.x, 0, Screen.width - tooltipPanel.rect.width);
            tooltipPos.y = Mathf.Clamp(tooltipPos.y, 0, Screen.height - tooltipPanel.rect.height);
            tooltipPanel.position = tooltipPos;
        }
    }

    public void ToggleSkillTreePanel()
    {
        skillTreePanel.SetActive(!skillTreePanel.activeSelf);
        if (skillTreePanel.activeSelf)
        {
            UpdateUI();
        }
    }

    private void OnSkillPointsChanged(int level, int skillPoints)
    {
        UpdateUI();
    }

    private void OnEnable()
    {
        Debug.Log("SkillTreeUI OnEnable called");
        if (xpManager == null)
        {
            xpManager = FindObjectOfType<XPManager>();
            Debug.Log($"xpManager found in OnEnable: {xpManager != null}");
        }
        if (xpManager != null)
        {
            xpManager.OnLevelUp += OnSkillPointsChanged;
        }
        Debug.Log("About to call UpdateUI from OnEnable");
        UpdateUI();
        Debug.Log("UpdateUI called from OnEnable completed");
    }

    private void OnDisable()
    {
        if (xpManager != null)
        {
            xpManager.OnLevelUp -= OnSkillPointsChanged;
        }
    }
    public void SetupTalentsToggleTooltip()
    {
        if (talentsToggle != null)
        {
            EventTrigger trigger = talentsToggle.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = talentsToggle.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { ShowTooltip("Unlock or Upgrade\nnew spells"); });
            trigger.triggers.Add(enterEntry);

            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { HideTooltip(); });
            trigger.triggers.Add(exitEntry);
        }
        else
        {
            Debug.LogWarning("Talents Toggle is not assigned in the SkillTreeUI.");
        }
    }

}

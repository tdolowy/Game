using System.Collections.Generic;
using UnityEngine;

public class XPManager : MonoBehaviour
{
    public int level = 1;
    public int xp = 0;
    public int xpToNextLevel = 100;
    public int gold = 0;
    public int attackPower = 10;
    public int skillPoints = 1;
    private Dictionary<string, int> unlockedSkills = new Dictionary<string, int>();
    private PlayerController playerController;
    private CardManager cardManager;
    private HashSet<string> defaultUnlockedSkills = new HashSet<string> { "BasicAttack" };

    public delegate void OnLevelUpDelegate(int newLevel, int newSkillPoints);
    public event OnLevelUpDelegate OnLevelUp;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        cardManager = FindObjectOfType<CardManager>();
        if (playerController == null)
            Debug.LogError("PlayerController not found!");
        if (cardManager == null)
            Debug.LogError("CardManager not found in the scene.");

        // Initialize default unlocked skills
        foreach (var skill in defaultUnlockedSkills)
        {
            if (!IsSkillUnlocked(skill))
            {
                UnlockSkill(skill, 1);
                Debug.Log($"{skill} initialized as unlocked by default");
            }
        }
    }

    public void GainXP(int amount)
    {
        xp += amount;
        if (xp >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    public void UpdateSpell(string skillName, int newLevel)
    {
        SpellManager spellManager = FindObjectOfType<SpellManager>();
        if (spellManager != null)
        {
            SpellDefinition spellDef = spellManager.GetSpellDefinition(skillName);
            if (spellDef != null)
            {
                int newDamage = spellDef.GetDamageForLevel(newLevel);
                int newResourceCost = spellDef.GetManaCostForLevel(newLevel);
                CardManager cardManager = FindObjectOfType<CardManager>();
                if (cardManager != null)
                {
                    cardManager.UpdateSpell(skillName, newLevel, newDamage, newResourceCost);
                }
                else
                {
                    Debug.LogError("CardManager not found in the scene.");
                }
            }
            else
            {
                Debug.LogError($"SpellDefinition for {skillName} not found.");
            }
        }
        else
        {
            Debug.LogError("SpellManager not found in the scene.");
        }
    }

    void LevelUp()
    {
        level++;
        xp -= xpToNextLevel;
        xpToNextLevel += 50;
        attackPower += 5;
        skillPoints++;

        if (playerController != null)
        {
            playerController.health += 10;
            Debug.Log($"Leveled up! Level: {level}, Attack Power: {attackPower}, Health: {playerController.health}, Skill Points: {skillPoints}");
        }
        else
        {
            Debug.Log("PlayerController reference is missing!");
        }

        OnLevelUp?.Invoke(level, skillPoints);
    }

    public void GainGold(int amount)
    {
        gold += amount;
        Debug.Log($"Gold earned: {amount}, Total Gold: {gold}");
    }

    public bool CanUnlockSkill(string skillName, int cost)
    {
        return skillPoints >= cost;
    }

    public bool UnlockSkill(string skillName, int skillLevel)
    {
        if (defaultUnlockedSkills.Contains(skillName) && IsSkillUnlocked(skillName))
        {
            Debug.Log($"Attempted to unlock {skillName}, but it's already unlocked by default");
            return false;
        }

        if (unlockedSkills.ContainsKey(skillName))
        {
            unlockedSkills[skillName] = skillLevel;
        }
        else
        {
            unlockedSkills.Add(skillName, skillLevel);
        }

        UpdateSpellInCardManager(skillName, skillLevel);
        Debug.Log($"Skill {skillName} unlocked/upgraded to level {skillLevel}");
        return true;
    }

    private void UpdateSpellInCardManager(string skillName, int skillLevel)
    {
        CardManager cardManager = FindObjectOfType<CardManager>();
        SpellManager spellManager = FindObjectOfType<SpellManager>();

        if (cardManager != null && spellManager != null)
        {
            SpellDefinition spellDef = spellManager.GetSpellDefinition(skillName);
            if (spellDef != null)
            {
                int newDamage = spellDef.GetDamageForLevel(skillLevel);
                int newResourceCost = spellDef.GetManaCostForLevel(skillLevel);
                cardManager.UpdateSpell(skillName, skillLevel, newDamage, newResourceCost);
            }
            else
            {
                Debug.LogError($"SpellDefinition for {skillName} not found.");
            }
        }
        else
        {
            Debug.LogError("CardManager or SpellManager not found in the scene.");
        }
    }

    public bool IsSkillUnlocked(string skillName)
    {
        return unlockedSkills.ContainsKey(skillName) || defaultUnlockedSkills.Contains(skillName);
    }

    public int GetSkillLevel(string skillName)
    {
        if (defaultUnlockedSkills.Contains(skillName))
        {
            if (unlockedSkills.TryGetValue(skillName, out int defaultSkillLevel))
            {
                return defaultSkillLevel;
            }
            return 1;
        }

        if (unlockedSkills.TryGetValue(skillName, out int unlockedSkillLevel))
        {
            return unlockedSkillLevel;
        }

        return 0;
    }
    public bool UseSkillPoints(int amount)
    {
        if (skillPoints >= amount)
        {
            skillPoints -= amount;
            Debug.Log($"Used {amount} skill point(s). Remaining skill points: {skillPoints}");
            OnLevelUp?.Invoke(level, skillPoints);
            return true;
        }

        Debug.Log($"Not enough skill points. Required: {amount}, Available: {skillPoints}");
        return false;
    }
}

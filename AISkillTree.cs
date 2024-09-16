using UnityEngine;
using System.Collections.Generic;

public class AISkillTree : MonoBehaviour
{
    private Dictionary<string, int> unlockedSkills = new Dictionary<string, int>();
    private XPManager xpManager;
    private AIController aiController;
    private SpellManager spellManager;

    private void Start()
    {
        xpManager = GetComponent<XPManager>();
        aiController = GetComponent<AIController>();

        if (xpManager == null)
        {
            Debug.LogError("XPManager not found on the same GameObject as AISkillTree.");
        }
        if (aiController == null)
        {
            Debug.LogError("AIController not found on the same GameObject as AISkillTree.");
        }
        spellManager = FindObjectOfType<SpellManager>();
        if (spellManager == null)
        {
            Debug.LogError("SpellManager not found in the scene.");
        }
    }

    public int GetSkillPoints()
    {
        return xpManager != null ? xpManager.skillPoints : 0;
    }

    public bool CanUnlockSkill(string skillName, int cost)
    {
        if (!CheckPrerequisites(skillName))
        {
            return false;
        }
        return GetSkillPoints() >= cost;
    }

    public bool UseSkillPoints(int amount)
    {
        if (xpManager != null && xpManager.skillPoints >= amount)
        {
            xpManager.skillPoints -= amount;
            Debug.Log($"AI used {amount} skill point(s). Remaining: {xpManager.skillPoints}");
            if (aiController != null)
            {
                aiController.UpdateAISkillPointsText();
            }
            return true;
        }
        return false;
    }

    public bool UnlockSkill(string skillName, int level)
    {
        if (IsDefaultSkill(skillName))
        {
            Debug.Log($"Cannot unlock or upgrade {skillName} as it is a default skill.");
            return false;
        }

        if (!CheckPrerequisites(skillName))
        {
            Debug.Log($"Cannot unlock {skillName}: prerequisites not met");
            return false;
        }

        if (unlockedSkills.ContainsKey(skillName))
        {
            unlockedSkills[skillName] = level;
        }
        else
        {
            unlockedSkills.Add(skillName, level);
        }

        Debug.Log($"AI unlocked/upgraded {skillName} to level {level}");

        // Update the spell in SpellManager
        if (spellManager != null)
        {
            spellManager.UpdateSpellLevel(skillName, level, false);
        }

        return true;
    }

    public bool IsDefaultSkill(string skillName)
    {
        return skillName == "BasicAttack";
    }

    public bool IsSkillUnlocked(string skillName)
    {
        return GetSkillLevel(skillName) > 0;
    }


    public int GetSkillLevel(string skillName)
    {
        return unlockedSkills.TryGetValue(skillName, out int level) ? level : 0;
    }

    public bool CheckPrerequisites(string skillName)
    {
        if (spellManager == null) return false;

        SpellDefinition spellDef = spellManager.GetSpellDefinition(skillName);
        if (spellDef == null) return false;

        if (spellDef.Prerequisites.Count == 0) return true;

        foreach (string prerequisite in spellDef.Prerequisites)
        {
            if (IsSkillUnlocked(prerequisite))
            {
                Debug.Log($"Prerequisite {prerequisite} met for {skillName}");
                return true; // If any prerequisite is met, return true
            }
        }

        Debug.Log($"No prerequisites met for {skillName}");
        return false;
    }

}

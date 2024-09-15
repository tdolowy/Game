using UnityEngine;
using System.Collections.Generic;

public class SpellManager : MonoBehaviour
{
    public List<SpellDefinition> spellDefinitions;

    private CardManager playerCardManager;
    private AICardManager aiCardManager;

    private void Awake()
    {
        playerCardManager = FindObjectOfType<CardManager>();
        aiCardManager = FindObjectOfType<AICardManager>();

        if (playerCardManager == null)
        {
            Debug.LogError("CardManager not found in the scene.");
        }

        if (aiCardManager == null)
        {
            Debug.LogError("AICardManager not found in the scene.");
        }

        if (spellDefinitions == null || spellDefinitions.Count == 0)
        {
            Debug.LogError("No SpellDefinitions assigned to SpellManager.");
        }
        else
        {
            InitializeSpells();
        }
    }

    private void InitializeSpells()
    {
        foreach (var spellDef in spellDefinitions)
        {
            if (spellDef == null)
            {
                Debug.LogError("Null SpellDefinition found in SpellManager.");
                continue;
            }

            // Initialize for player
            if (playerCardManager != null)
            {
                playerCardManager.AddSpell(spellDef.spellName, spellDef.baseDamage, spellDef.spellColor, spellDef.baseManaCost);
            }

            // Initialize for AI
            if (aiCardManager != null)
            {
                aiCardManager.AddSpell(spellDef.spellName, spellDef.baseDamage, spellDef.spellColor, spellDef.baseManaCost);
            }
        }
    }

    public void UpdateSpellLevel(string spellName, int newLevel, bool isPlayer)
    {
        SpellDefinition spellDef = GetSpellDefinition(spellName);
        if (spellDef != null)
        {
            int newDamage = spellDef.GetDamageForLevel(newLevel);
            int newManaCost = spellDef.GetManaCostForLevel(newLevel);

            if (isPlayer && playerCardManager != null)
            {
                playerCardManager.UpdateSpell(spellName, newLevel, newDamage, newManaCost);
            }
            else if (!isPlayer && aiCardManager != null)
            {
                aiCardManager.AddSpell(spellName, newDamage, spellDef.spellColor, newManaCost);
            }
        }
        else
        {
            Debug.LogWarning($"Spell '{spellName}' not found in SpellManager.");
        }
    }

    public SpellDefinition GetSpellDefinition(string spellName)
    {
        if (spellDefinitions == null || spellDefinitions.Count == 0)
        {
            Debug.LogError("SpellDefinitions list is null or empty in SpellManager.");
            return null;
        }

        SpellDefinition spellDef = spellDefinitions.Find(s => s != null && s.spellName == spellName);
        if (spellDef == null)
        {
            Debug.LogWarning($"SpellDefinition for '{spellName}' not found in SpellManager.");
        }
        return spellDef;
    }
}

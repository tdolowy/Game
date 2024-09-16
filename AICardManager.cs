using System.Collections.Generic;
using UnityEngine;

public class AICardManager : MonoBehaviour
{
    private Dictionary<string, Spell> spells = new Dictionary<string, Spell>();
    private XPManager xpManager;

    private void Start()
    {
        xpManager = GetComponent<XPManager>();
        if (xpManager == null)
        {
            Debug.LogError("XPManager not found on the same GameObject as AICardManager.");
        }

        InitializeSpells();
    }

    private void InitializeSpells()
    {
        {
            Debug.Log("Initializing spells in CardManager");
            spells.Clear(); // Clear existing spells to ensure a fresh start
            AddSpell("BasicAttack", 10, Color.white, 0);
            AddSpell("Fireball", 30, Color.red, 40);
            AddSpell("Ice Bolt", 25, Color.cyan, 30);
            Debug.Log($"Spells initialized. Count: {spells.Count}");
        }
    }

    public void AddSpell(string name, int damage, Color color, int resourceCost)
    {
        if (spells.ContainsKey(name))
        {
            spells[name].damage = damage;
            spells[name].color = color;
            spells[name].resourceCost = resourceCost;
        }
        else
        {
            spells.Add(name, new Spell(name, damage, color, resourceCost));
        }
    }

    public int GetSpellDamage(string spellName)
    {
        if (spells.TryGetValue(spellName, out Spell spell))
        {
            return spell.damage;
        }
        Debug.LogWarning($"Spell '{spellName}' not found for AI. Returning 0 damage.");
        return 0;
    }

    public int GetSpellResourceCost(string spellName)
    {
        if (spells.TryGetValue(spellName, out Spell spell))
        {
            return spell.resourceCost;
        }
        Debug.LogWarning($"Spell '{spellName}' not found. Returning 0 resource cost.");
        return 0;
    }

    private class Spell
    {
        public string name;
        public int damage;
        public Color color;
        public int resourceCost;

        public Spell(string name, int damage, Color color, int resourceCost)
        {
            this.name = name;
            this.damage = damage;
            this.color = color;
            this.resourceCost = resourceCost;
        }
    }
    public void UpdateSpell(string spellName, int level, int newDamage, int newResourceCost)
    {
        if (spells.TryGetValue(spellName, out Spell spell))
        {
            spell.damage = newDamage;
            spell.resourceCost = newResourceCost;
            Debug.Log($"Updated AI spell: {spellName}, Level: {level}, Damage: {newDamage}, Resource Cost: {newResourceCost}");
        }
        else
        {
            Debug.LogWarning($"Spell '{spellName}' not found in AICardManager.");
        }
    }

}

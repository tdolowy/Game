using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spells/Spell Definition")]
public class SpellDefinition : ScriptableObject
{
    public string spellName;
    public int baseDamage;
    public int baseManaCost;
    public int damageIncreasePerLevel;
    public int manaIncreasePerLevel;
    public Color spellColor = Color.white;
    public bool isUnlockedByDefault = false;
    [SerializeField] private List<string> _prerequisites = new List<string>();

    public IReadOnlyList<string> Prerequisites => _prerequisites;

    public int GetDamageForLevel(int level)
    {
        return baseDamage + (level - 1) * damageIncreasePerLevel;
    }

    public int GetManaCostForLevel(int level)
    {
        return baseManaCost + (level - 1) * manaIncreasePerLevel;
    }

    public void SetPrerequisites(params string[] spells)
    {
        _prerequisites = new List<string>(spells.Where(s => !string.IsNullOrEmpty(s)));
    }

    public void ClearPrerequisites()
    {
        _prerequisites.Clear();
    }

    public bool MeetsPrerequisites(System.Func<string, bool> isSpellUnlocked)
    {
        return _prerequisites.Count == 0 || _prerequisites.Any(isSpellUnlocked);
    }

    public string GetPrerequisitesString()
    {
        return _prerequisites.Count == 0 ? "No prerequisites" : string.Join(" OR ", _prerequisites);
    }
}

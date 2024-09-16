using UnityEngine;

public class AIMob : Mob
{
    protected override void CounterAttack(GameObject attacker)
    {
        AIController aiController = attacker.GetComponent<AIController>();
        if (aiController != null)
        {
            int aiHealth = aiController.GetHealth();
            int damage = CalculateDamage(aiController.GetComponent<XPManager>());
            aiHealth -= damage;
            Debug.Log($"{GetType().Name} counterattacked AI for {damage} damage! AI's health: {aiHealth}");
            ShowFloatingDamage(aiController.transform.position, damage);
            aiHealth = Mathf.Max(0, aiHealth);
            aiController.SetHealth(aiHealth);
            if (aiHealth <= 0)
            {
                Debug.Log("AI was defeated by " + GetType().Name + "'s counterattack!");
            }
        }
    }

    private int CalculateDamage(XPManager xpManager)
    {
        int levelDifference = xpManager.level - level;
        if (levelDifference >= 0)
        {
            return Mathf.CeilToInt(baseDamage / (1 + levelDifference));
        }
        else
        {
            return baseDamage * (1 + Mathf.Abs(levelDifference));
        }
    }
}

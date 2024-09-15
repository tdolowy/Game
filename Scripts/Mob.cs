using UnityEngine;

public class Mob : Enemy
{
    public int baseDamage = 10;
    public GameObject floatingTextPrefab;

    protected override void Start()
    {
        health = 50;
        maxHealth = 50;
        xpReward = 50;
        goldReward = 20;
        level = 1;
        base.Start();
    }

    protected override void Respawn()
    {
        health = 50 + (level * 15);
        maxHealth = health;
        xpReward = 50 + (level * 10);
        goldReward = 20 + (level * 5);

        level++;

        isDead = false;
        gameObject.SetActive(true);

        InstantiateHealthBar();
    }

    protected override void CounterAttack(GameObject attacker)
    {
        PlayerController playerController = attacker.GetComponent<PlayerController>();
        AIController aiController = attacker.GetComponent<AIController>();
        XPManager xpManager = attacker.GetComponent<XPManager>();

        if (xpManager != null)
        {
            int levelDifference = xpManager.level - level;
            int damage;

            if (levelDifference >= 0)
            {
                // Player/opponent is equal or higher level than the mob
                damage = Mathf.CeilToInt(baseDamage / (1 + levelDifference));
            }
            else
            {
                // Mob is higher level than the player/opponent
                damage = baseDamage * (1 + Mathf.Abs(levelDifference));
            }

            if (playerController != null)
            {
                playerController.health -= damage;
                Debug.Log($"{GetType().Name} counterattacked Player for {damage} damage! Player's health: {playerController.health}");

                // Show floating damage
                ShowFloatingDamage(playerController.transform.position, damage);

                // Ensure health doesn't go below 0
                playerController.health = Mathf.Max(0, playerController.health);

                if (playerController.health <= 0)
                {
                    Debug.Log("Player was defeated by " + GetType().Name + "'s counterattack!");
                    // You might want to call a method here to handle the player's defeat
                }
            }
            else if (aiController != null)
            {
                int aiHealth = aiController.GetHealth();
                aiHealth -= damage;
                Debug.Log($"{GetType().Name} counterattacked AI for {damage} damage! AI's health: {aiHealth}");

                // Show floating damage
                ShowFloatingDamage(aiController.transform.position, damage);

                // Ensure health doesn't go below 0
                aiHealth = Mathf.Max(0, aiHealth);
                aiController.SetHealth(aiHealth);

                if (aiHealth <= 0)
                {
                    Debug.Log("AI was defeated by " + GetType().Name + "'s counterattack!");
                    // You might want to call a method here to handle the AI's defeat
                }
            }
        }
    }


    public void ShowFloatingDamage(Vector3 position, int damage)
    {
        if (floatingTextPrefab != null)
        {
            GameObject floatingTextObject = Instantiate(floatingTextPrefab, position, Quaternion.identity);
            FloatingDamageText floatingDamageText = floatingTextObject.GetComponent<FloatingDamageText>();
            if (floatingDamageText != null)
            {
                floatingDamageText.SetDamageText(damage);
                floatingTextObject.transform.position = position + Vector3.up * 2f; // Adjust the height as needed
            }
            else
            {
                Debug.LogError("FloatingDamageText component is missing on the instantiated prefab.");
            }
        }
        else
        {
            Debug.LogWarning("Floating text prefab is not assigned in the Mob script.");
        }
    }
}

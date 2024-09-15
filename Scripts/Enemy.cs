using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
    public int health;
    public int maxHealth;
    public int xpReward;
    public int goldReward;
    public int level;
    public GameObject healthBarPrefab;
    public float knockbackDistance = 0.5f;
    public float knockbackDuration = 0.2f;

    protected GameObject healthBarInstance;
    protected HealthBar healthBar;
    protected bool isDead = false;

    protected virtual void Start()
    {
        InstantiateHealthBar();
    }

    void Update()
    {
        if (isDead)
        {
            Respawn();
        }
    }

    public void TakeDamage(int damage, GameObject attacker)
    {
        if (isDead) return;

        // Counterattack before taking damage
        CounterAttack(attacker);

        health -= damage;
        if (health <= 0)
        {
            Die(attacker);
        }
        else
        {
            UpdateHealthBar();
            StartCoroutine(ApplyKnockback(attacker));
        }
    }

    protected abstract void CounterAttack(GameObject attacker);

    protected void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            float healthPercentage = (float)health / maxHealth;
            healthBar.SetHealth(healthPercentage);
        }
    }

    protected virtual void Die(GameObject attacker)
    {
        isDead = true;

        XPManager xpManager = attacker.GetComponent<XPManager>();
        if (xpManager != null)
        {
            xpManager.GainXP(xpReward);
            xpManager.GainGold(goldReward);
        }
        else
        {
            Debug.LogWarning("XPManager not found on attacker.");
        }

        gameObject.SetActive(false);

        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }

        Invoke("Respawn", 1f);
    }

    protected abstract void Respawn();

    protected void InstantiateHealthBar()
    {
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position, Quaternion.identity, transform);
            healthBar = healthBarInstance.GetComponent<HealthBar>();
            if (healthBar != null)
            {
                healthBar.foregroundPanel = healthBarInstance.transform.Find("Foreground").GetComponent<RectTransform>();
                UpdateHealthBar();
            }
            else
            {
                Debug.LogError("HealthBar component not found in the health bar prefab.");
            }
        }
        else
        {
            Debug.LogError("Health bar prefab not assigned.");
        }
    }

    private IEnumerator ApplyKnockback(GameObject attacker)
    {
        Vector3 knockbackDirection = (transform.position - attacker.transform.position).normalized;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + knockbackDirection * knockbackDistance;

        float elapsedTime = 0f;
        while (elapsedTime < knockbackDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / knockbackDuration;

            // Move back
            if (t <= 0.5f)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, t * 2);
            }
            // Move forward
            else
            {
                transform.position = Vector3.Lerp(endPosition, startPosition, (t - 0.5f) * 2);
            }

            yield return null;
        }

        // Ensure the enemy is back at the starting position
        transform.position = startPosition;
    }
}

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public bool isLocalPlayer = true;
    private NavMeshAgent agent;
    public int health = 100;
    public int resource = 100;

    public Text opponentDefeatedText;
    public Text youLoseText;

    private Vector3 originalPosition;
    private Vector3 attackPosition;
    private bool isAttacking = false;
    private bool canTakeInput = true;

    private TurnManager turnManager;

    public float knockbackDistance = 0.5f;
    public float knockbackDuration = 0.2f;

    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        if (turnManager == null)
        {
            Debug.LogError("TurnManager not found in the scene. Please add a TurnManager.");
        }

        agent = GetComponent<NavMeshAgent>();
        originalPosition = transform.position;

        if (opponentDefeatedText != null) opponentDefeatedText.text = "";
        if (youLoseText != null) youLoseText.text = "";
    }

    void Update()
    {
        if (isLocalPlayer && canTakeInput)
        {
            if (isAttacking && !agent.pathPending && agent.remainingDistance < 0.5f)
            {
                PerformAttack();
            }
        }
    }

    public void SetTarget(Transform target)
    {
        Vector3 targetPosition = target.position;
        attackPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z) + (targetPosition - transform.position).normalized * 1f;
        MoveToAttackPosition();
    }

    public void MoveToAttackPosition()
    {
        if (agent != null)
        {
            agent.SetDestination(attackPosition);
            isAttacking = true;
        }
    }

    void PerformAttack()
    {
        Debug.Log("Attacking target!");
        agent.SetDestination(originalPosition);
        isAttacking = false;
        turnManager.PerformAction();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        ApplyKnockback(FindObjectOfType<AIController>().transform.position); // Assuming the AI is the attacker
        if (health <= 0)
        {
            if (isLocalPlayer)
            {
                Debug.Log("You lose!");
                if (youLoseText != null) youLoseText.text = "You Lose!";
            }
            else
            {
                Debug.Log("Opponent defeated!");
                if (opponentDefeatedText != null) opponentDefeatedText.text = "Opponent Defeated!";
            }
        }
    }

    public void EnablePlayerInput()
    {
        canTakeInput = true;
        Debug.Log("Player input enabled.");
    }

    public void DisablePlayerInput()
    {
        canTakeInput = false;
        Debug.Log("Player input disabled.");
    }

    public void BuyItemAction(string itemName)
    {
        Debug.Log($"Attempting to buy item: {itemName}");
        if (itemName == "HealthPotion")
        {
            ShopManager shopManager = FindObjectOfType<ShopManager>();
            if (shopManager != null)
            {
                shopManager.BuyHealthPotion();
            }
        }
        turnManager.PerformAction();
    }

    public void ApplyKnockback(Vector3 attackerPosition)
    {
        StartCoroutine(PerformKnockback(attackerPosition));
    }

    private IEnumerator PerformKnockback(Vector3 attackerPosition)
    {
        Vector3 knockbackDirection = (transform.position - attackerPosition).normalized;
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

        // Ensure the player is back at the starting position
        transform.position = startPosition;
    }
}

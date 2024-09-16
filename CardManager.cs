using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public Transform playerTransform;
    public NavMeshAgent playerAgent;
    public Vector3 attackPosition = new Vector3(50f, 3f, 47f);
    public float leniencyRadius = 1.0f;
    public float returnThreshold = 0.1f;
    private bool isMovingToAttackPosition = false;
    private bool hasAttacked = false;
    private Vector3 exactOriginalPosition;
    private Transform target;
    private PlayerController playerController;
    public TargetingSystem targetingSystem;
    public GameObject floatingTextPrefab;
    public ResourceOrbUI resourceOrbUI;
    public Text noResourcesText;
    public float noResourcesDisplayTime = 2f;

    private Dictionary<string, Spell> spells = new Dictionary<string, Spell>();
    private Spell currentSpell;
    private float noResourcesTimer;
    private TurnManager turnManager;
    private bool canCastSpells = true;

    private XPManager xpManager;

    public delegate void PlayerReturnedDelegate();
    public event PlayerReturnedDelegate OnPlayerReturned;

    void Awake()
    {
        Debug.Log("CardManager Awake called");
        InitializeSpells();
        turnManager = FindObjectOfType<TurnManager>();
        if (turnManager == null)
        {
            Debug.LogError("TurnManager not found in the scene.");
        }

        xpManager = FindObjectOfType<XPManager>();
        if (xpManager == null)
        {
            Debug.LogError("XPManager not found in the scene.");
        }
    }

    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform not assigned in the Inspector.");
            return;
        }
        if (playerAgent == null)
        {
            Debug.LogError("Player NavMeshAgent not assigned in the Inspector.");
            return;
        }
        exactOriginalPosition = playerTransform.position;
        playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController component not found on the player object.");
            return;
        }
        if (targetingSystem == null)
        {
            targetingSystem = FindObjectOfType<TargetingSystem>();
            if (targetingSystem == null)
            {
                Debug.LogError("TargetingSystem not found in the scene.");
                return;
            }
        }
        if (floatingTextPrefab == null)
        {
            Debug.LogWarning("Floating text prefab is not assigned in the CardManager.");
        }

        if (noResourcesText == null)
        {
            Debug.LogWarning("No resources text UI is not assigned in the CardManager.");
        }
        else
        {
            noResourcesText.gameObject.SetActive(false);
        }
    }

    public void InitializeSpells()
    {
        Debug.Log("Initializing spells in CardManager");
        spells.Clear(); // Clear existing spells to ensure a fresh start
        AddSpell("BasicAttack", 10, Color.white, 0);
        AddSpell("Fireball", 30, Color.red, 40);
        AddSpell("Ice Bolt", 25, Color.cyan, 30);
        Debug.Log($"Spells initialized. Count: {spells.Count}");
    }

    public void AddSpell(string name, int damage, Color color, int resourceCost)
    {
        Debug.Log($"AddSpell called with name: {name}, damage: {damage}, resourceCost: {resourceCost}");
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
        Debug.Log($"Spell added/updated: {name} with damage: {spells[name].damage} and resource cost: {spells[name].resourceCost}");
    }

    public void EnsureSpellsInitialized()
    {
        if (spells.Count == 0)
        {
            Debug.Log("Spells not initialized, initializing now");
            InitializeSpells();
        }
        else
        {
            Debug.Log("Spells already initialized");
        }
    }

    public void SetSelectedTarget(Transform targetTransform)
    {
        target = targetTransform;
        Debug.Log($"Target set in CardManager: {(target != null ? target.name : "null")}");
    }

    public void EnableSpellCasting()
    {
        canCastSpells = true;
    }

    public void DisableSpellCasting()
    {
        canCastSpells = false;
    }

    public bool Use(string spellName)
    {
        if (!canCastSpells)
        {
            Debug.LogWarning("Spell casting is currently disabled.");
            return false;
        }

        // Check if the spell exists in the spells dictionary
        if (spells.TryGetValue(spellName, out Spell spell))
        {
            // Check if the skill is unlocked in XPManager
            if (spellName == "BasicAttack" || (xpManager != null && xpManager.IsSkillUnlocked(spellName)))
            {
                if (playerController == null)
                {
                    Debug.LogError("PlayerController is null. Cannot cast spell.");
                    return false;
                }
                Debug.Log($"Attempting to cast {spellName}. Cost: {spell.resourceCost}, Current resources: {playerController.resource}");
                if (playerController.resource >= spell.resourceCost || spellName == "BasicAttack")
                {
                    currentSpell = spell;
                    target = targetingSystem.GetCurrentTarget();
                    if (target == null)
                    {
                        Debug.LogWarning("No target selected. Please select a target before casting a spell.");
                        return false;
                    }

                    // Deduct resource cost immediately
                    if (spellName != "BasicAttack")
                    {
                        playerController.resource -= spell.resourceCost;
                        if (resourceOrbUI != null)
                        {
                            resourceOrbUI.UpdateResourceUI();
                        }
                    }

                    UseSelectedSpell();
                    return true;
                }
                else
                {
                    Debug.LogWarning($"Not enough resource to cast {spellName}. Required: {spell.resourceCost}, Current: {playerController.resource}");
                    ShowNoResourcesText();
                    return false;
                }
            }
            else
            {
                Debug.LogWarning($"Spell '{spellName}' is not unlocked yet.");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"Spell '{spellName}' not found in the spells dictionary.");
            return false;
        }
    }

    private void UseSelectedSpell()
    {
        if (currentSpell != null && !isMovingToAttackPosition)
        {
            exactOriginalPosition = playerTransform.position;
            playerAgent.SetDestination(attackPosition);
            isMovingToAttackPosition = true;
            hasAttacked = false;
            Debug.Log($"Moving to cast {currentSpell.name}. Resource remaining: {playerController.resource}");
            Debug.Log($"Exact original position: {exactOriginalPosition}");


            if (turnManager != null)
            {
                turnManager.PerformAction();

            }
            else
            {
                Debug.LogError("TurnManager is null. Action not recorded in turn system.");
            }

        }
        else if (currentSpell == null)
        {
            Debug.LogWarning("No spell selected");
        }
    }

    private void ShowNoResourcesText()
    {
        if (noResourcesText != null)
        {
            noResourcesText.gameObject.SetActive(true);
            noResourcesTimer = noResourcesDisplayTime;
        }
        else
        {
            Debug.LogWarning("No resources text UI is not assigned in the CardManager.");
        }
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

    public int GetSpellDamage(string spellName)
    {
        Debug.Log($"GetSpellDamage called for {spellName}");
        if (spells.Count == 0)
        {
            Debug.LogWarning("Spells dictionary is empty. Initializing spells.");
            InitializeSpells();
        }
        if (spells.TryGetValue(spellName, out Spell spell))
        {
            Debug.Log($"Damage for {spellName}: {spell.damage}");
            return spell.damage;
        }
        Debug.LogWarning($"Spell '{spellName}' not found. Returning 0 damage.");
        return 0;
    }

    void Update()
    {
        if (isMovingToAttackPosition && !playerAgent.pathPending)
        {
            float distanceToAttackPosition = Vector3.Distance(playerTransform.position, attackPosition);
            if (distanceToAttackPosition <= leniencyRadius)
            {
                if (!hasAttacked)
                {
                    Debug.Log("Attempting to perform attack");
                    PerformAttack();
                    hasAttacked = true;
                }
                playerAgent.SetDestination(exactOriginalPosition);
                isMovingToAttackPosition = false;
            }
        }
        else if (!isMovingToAttackPosition && hasAttacked)
        {
            float distanceToOriginal = Vector3.Distance(playerTransform.position, exactOriginalPosition);
            if (distanceToOriginal <= returnThreshold)
            {
                playerAgent.ResetPath();
                playerTransform.position = exactOriginalPosition;
                hasAttacked = false;
                Debug.Log($"Player returned to exact original position: {playerTransform.position}");
                OnPlayerReturned?.Invoke();
            }
        }

        if (noResourcesTimer > 0)
        {
            noResourcesTimer -= Time.deltaTime;
            if (noResourcesTimer <= 0)
            {
                if (noResourcesText != null)
                {
                    noResourcesText.gameObject.SetActive(false);
                }
            }
        }
    }

private void PerformAttack()
{
    if (target != null && currentSpell != null)
    {
        Debug.Log($"Performing attack with {currentSpell.name} on {target.name}");

        // Store the target information before clearing
        Transform targetTransform = target;
        Vector3 targetPosition = target.position;

        // Clear the target
        TargetingSystem targetingSystem = FindObjectOfType<TargetingSystem>();
        if (targetingSystem != null)
        {
            targetingSystem.ClearTarget();
        }

        // Proceed with the attack using the stored target information
        var enemy = targetTransform.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(currentSpell.damage, playerTransform.gameObject);
            Debug.Log($"Player cast {currentSpell.name} on: {targetTransform.name} with damage: {currentSpell.damage}");
            VisualizeSpellEffect(targetPosition);
            Debug.Log($"Attempting to show floating damage at position: {targetPosition}");
            ShowFloatingDamage(targetPosition, currentSpell.damage);
        }
        else
        {
            var opponent = targetTransform.GetComponent<PlayerController>();
            if (opponent != null)
            {
                opponent.TakeDamage(currentSpell.damage);
                Debug.Log($"Player cast {currentSpell.name} on opponent: {targetTransform.name} with damage: {currentSpell.damage}");
                VisualizeSpellEffect(targetPosition);
                Debug.Log($"Attempting to show floating damage at position: {targetPosition}");
                ShowFloatingDamage(targetPosition, currentSpell.damage);
            }
            else
            {
                Debug.LogWarning("Target does not have an Enemy or PlayerController component.");
            }
        }

        // Clear the stored target and spell after the attack
        target = null;
        currentSpell = null;
    }
    else
    {
        Debug.LogWarning($"Cannot perform attack. Target: {(target != null ? target.name : "null")}, Current Spell: {(currentSpell != null ? currentSpell.name : "null")}");
    }
}


    private void VisualizeSpellEffect(Vector3 position)
    {
        GameObject visualEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visualEffect.transform.position = position;
        visualEffect.transform.localScale = Vector3.one * 0.5f;
        Renderer renderer = visualEffect.GetComponent<Renderer>();
        renderer.material.color = currentSpell.color;
        Destroy(visualEffect, 1f);
    }

    private void ShowFloatingDamage(Vector3 position, int damage)
    {
        if (floatingTextPrefab != null)
        {
            GameObject floatingTextObject = Instantiate(floatingTextPrefab, position, Quaternion.identity);
            FloatingDamageText floatingDamageText = floatingTextObject.GetComponent<FloatingDamageText>();
            if (floatingDamageText != null)
            {
                floatingDamageText.SetDamageText(damage);
                floatingTextObject.transform.position = position + floatingDamageText.offset;
                Debug.Log($"Floating damage text created at position: {floatingTextObject.transform.position}");
            }
            else
            {
                Debug.LogError("FloatingDamageText component is missing on the instantiated prefab.");
            }
        }
        else
        {
            Debug.LogWarning("Floating text prefab is not assigned in the CardManager.");
        }
    }

    public void UpdateSpell(string spellName, int level, int newDamage, int newResourceCost)
    {
        if (spells.TryGetValue(spellName, out Spell spell))
        {
            spell.damage = newDamage;
            spell.resourceCost = newResourceCost;
            Debug.Log($"Updated spell: {spellName}, Level: {level}, Damage: {newDamage}, Resource Cost: {newResourceCost}");
        }
        else
        {
            Debug.LogWarning($"Spell '{spellName}' not found in CardManager.");
        }
    }

}
[System.Serializable]
public class Spell
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

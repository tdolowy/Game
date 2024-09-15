using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AIController : MonoBehaviour
{
    public NavMeshAgent agent;
    public Vector3 attackPosition = new Vector3(50f, 3f, 47f);
    public Vector3 enemyMobAttackPosition = new Vector3(45f, 3f, 55f);
    public float leniencyRadius = 1.0f;
    public float returnThreshold = 0.1f;
    public GameObject floatingTextPrefab;
    public int resourcePerTurn = 15;
    public int maxResource = 100;
    public Text aiSkillPointsText;

    private TurnManager turnManager;
    private AICardManager aiCardManager;
    private ShopManager shopManager;
    private PlayerController playerController;
    private XPManager xpManager;
    private AISkillTree aiSkillTree;
    private Vector3 originalPosition;
    private bool isMovingToAttackPosition = false;
    private bool hasActed = false;
    private List<string> availableActions = new List<string> { "BasicAttack" };
    private List<string> availableItems = new List<string> { "HealthPotion" };
    private SpellManager spellManager;

    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        aiCardManager = GetComponent<AICardManager>();
        shopManager = FindObjectOfType<ShopManager>();
        playerController = GetComponent<PlayerController>();
        xpManager = GetComponent<XPManager>();
        aiSkillTree = GetComponent<AISkillTree>();
        agent = GetComponent<NavMeshAgent>();
        originalPosition = transform.position;
        spellManager = FindObjectOfType<SpellManager>();

        if (turnManager == null || aiCardManager == null || shopManager == null || agent == null ||
            playerController == null || xpManager == null || aiSkillTree == null || spellManager == null)
        {
            Debug.LogError("AIController: Missing required components. Please check the scene.");
        }

        if (xpManager != null)
        {
            xpManager.OnLevelUp += OnAILevelUp;
        }

        UpdateAISkillPointsText();
        InitializeAvailableSpells();
    }

    private void OnDestroy()
    {
        if (xpManager != null)
        {
            xpManager.OnLevelUp -= OnAILevelUp;
        }
    }

    private void InitializeAvailableSpells()
    {
        availableActions.Clear();
        availableActions.Add("BasicAttack"); // Always add BasicAttack
        foreach (var spellDef in spellManager.spellDefinitions)
        {
            if (spellDef.isUnlockedByDefault && spellDef.spellName != "BasicAttack")
            {
                availableActions.Add(spellDef.spellName);
                aiSkillTree.UnlockSkill(spellDef.spellName, 1); // Unlock default skills at level 1
            }
        }
    }

    private void OnAILevelUp(int newLevel, int newSkillPoints)
    {
        Debug.Log($"AI leveled up to {newLevel}. New skill points: {newSkillPoints}");
        AttemptSkillUnlockOrUpgrade();
    }

    public int GetHealth()
    {
        return playerController.health;
    }

    public void SetHealth(int newHealth)
    {
        playerController.health = newHealth;
    }

    public void StartTurn()
    {
        AddAIResource();
        AttemptSkillUnlockOrUpgrade();
        StartCoroutine(PerformAITurn());
    }

    private void AddAIResource()
    {
        int currentResource = playerController.resource;
        int newResource = Mathf.Min(currentResource + resourcePerTurn, maxResource);
        playerController.resource = newResource;
        Debug.Log($"AI resource increased from {currentResource} to {newResource}");
    }

    private IEnumerator PerformAITurn()
    {
        int actionsRemaining = 2;
        while (actionsRemaining > 0)
        {
            float waitTime = actionsRemaining == 2 ? 1f : 2f;
            yield return new WaitForSeconds(waitTime);
            PerformRandomAction();
            yield return new WaitUntil(() => hasActed);
            hasActed = false;
            isMovingToAttackPosition = false;
            actionsRemaining--;
            Debug.Log($"AI action completed. Actions remaining: {actionsRemaining}");
        }
        yield return new WaitForSeconds(2f);
        Debug.Log("AI turn completed. Ending turn.");
        turnManager.EndAITurn();
    }

    private void AttemptSkillUnlockOrUpgrade()
    {
        Debug.Log($"AIController: Attempting to unlock or upgrade skills. Current skill points: {aiSkillTree.GetSkillPoints()}");
        while (aiSkillTree.GetSkillPoints() > 0)
        {
            string skillToUpgrade = ChooseSkillToUpgrade();
            if (!string.IsNullOrEmpty(skillToUpgrade))
            {
                UnlockOrUpgradeSkill(skillToUpgrade);
            }
            else
            {
                Debug.Log("AIController: No more skills to upgrade");
                break;
            }
        }
        UpdateAISkillPointsText();
    }

    private string ChooseSkillToUpgrade()
    {
        List<string> unlockedSpells = new List<string>();
        List<string> lockedSpells = new List<string>();
        foreach (var spellDef in spellManager.spellDefinitions)
        {
            if (aiSkillTree.IsSkillUnlocked(spellDef.spellName))
            {
                unlockedSpells.Add(spellDef.spellName);
            }
            else if (aiSkillTree.CanUnlockSkill(spellDef.spellName, 1))
            {
                lockedSpells.Add(spellDef.spellName);
            }
        }

        if (lockedSpells.Count > 0)
        {
            return lockedSpells[Random.Range(0, lockedSpells.Count)];
        }
        else if (unlockedSpells.Count > 0)
        {
            return unlockedSpells[Random.Range(0, unlockedSpells.Count)];
        }
        return null;
    }

    private void UnlockOrUpgradeSkill(string skillName)
    {
        if (aiSkillTree.IsDefaultSkill(skillName))
        {
            Debug.Log($"AIController: Cannot unlock or upgrade {skillName} as it is a default skill.");
            return;
        }

        SpellDefinition spellDef = spellManager.GetSpellDefinition(skillName);
        if (spellDef == null) return;

        int currentLevel = aiSkillTree.GetSkillLevel(skillName);
        int cost = 1;

        if (aiSkillTree.CanUnlockSkill(skillName, cost) && aiSkillTree.UseSkillPoints(cost))
        {
            int newLevel = currentLevel + 1;
            if (aiSkillTree.UnlockSkill(skillName, newLevel))
            {
                int newDamage = spellDef.GetDamageForLevel(newLevel);
                int newResourceCost = spellDef.GetManaCostForLevel(newLevel);
                Debug.Log($"AIController: AI spent 1 skill point to {(currentLevel == 0 ? "unlock" : "upgrade")} {skillName} to level {newLevel} with damage {newDamage} and resource cost {newResourceCost}");
                aiCardManager.AddSpell(skillName, newDamage, spellDef.spellColor, newResourceCost);
                if (!availableActions.Contains(skillName))
                {
                    availableActions.Add(skillName);
                }
            }
            else
            {
                // Refund the skill point if unlock failed
                aiSkillTree.UseSkillPoints(-1);
            }
        }
        else
        {
            Debug.Log($"AIController: Failed to unlock or upgrade {skillName}");
        }
    }

    private void PerformRandomAction()
    {
        int action = Random.Range(0, 2);
        switch (action)
        {
            case 0:
                PerformAttackAction();
                break;
            case 1:
                BuyRandomItem();
                break;
        }
    }

    private void PerformAttackAction()
    {
        List<string> availableAttacks = new List<string>(availableActions);
        availableAttacks.RemoveAll(action => !aiSkillTree.IsSkillUnlocked(action) && action != "BasicAttack");

        string randomAction = availableAttacks[Random.Range(0, availableAttacks.Count)];
        int resourceCost = GetResourceCost(randomAction);

        if (playerController.resource >= resourceCost)
        {
            Debug.Log($"AI using action: {randomAction}");
            playerController.resource -= resourceCost;
            string targetTag = ChooseSmartTarget();
            MoveToAttackPosition(() => PerformAttack(randomAction, targetTag), targetTag);
        }
        else
        {
            Debug.Log("AI doesn't have enough resource. Using Basic Attack instead.");
            string targetTag = ChooseSmartTarget();
            MoveToAttackPosition(() => PerformAttack("BasicAttack", targetTag), targetTag);
        }
    }

    private int GetResourceCost(string actionName)
    {
        return aiCardManager.GetSpellResourceCost(actionName);
    }

    private void BuyRandomItem()
    {
        if (availableItems.Count > 0)
        {
            string randomItem = availableItems[Random.Range(0, availableItems.Count)];
            int itemCost = GetItemCost(randomItem);
            if (xpManager.gold >= itemCost)
            {
                Debug.Log($"AI buying item: {randomItem}");
                xpManager.gold -= itemCost;
                UseItem(randomItem);
                hasActed = true;
                isMovingToAttackPosition = false;
                Debug.Log("AI completed buy item action.");
            }
            else
            {
                Debug.Log($"AI doesn't have enough gold to buy {randomItem}. Performing attack instead.");
                PerformAttackAction();
            }
        }
        else
        {
            Debug.Log("No items available to buy. Performing attack instead.");
            PerformAttackAction();
        }
    }

    private int GetItemCost(string itemName)
    {
        switch (itemName)
        {
            case "HealthPotion":
                return 50;
            default:
                return 0;
        }
    }

    private string ChooseSmartTarget()
    {
        int aiLevel = xpManager.level;
        int miniBossLevel = FindObjectOfType<MiniBoss>().level;
        int levelDifference = miniBossLevel - aiLevel;
        float randomValue = Random.value;

        if (levelDifference >= 2)
        {
            if (randomValue < 0.7f) return "EnemyMob";
            else if (randomValue < 0.98f) return "Player";
            else return "MiniBoss";
        }
        else if (levelDifference == 1)
        {
            if (randomValue < 0.55f) return "EnemyMob";
            else if (randomValue < 0.9f) return "Player";
            else return "MiniBoss";
        }
        else
        {
            if (randomValue < 0.15f) return "EnemyMob";
            else if (randomValue < 0.4f) return "Player";
            else return "MiniBoss";
        }
    }

    private void MoveToAttackPosition(System.Action onReachedPosition, string targetTag)
    {
        isMovingToAttackPosition = true;
        Vector3 targetPosition = targetTag == "EnemyMob" ? enemyMobAttackPosition : attackPosition;
        agent.speed = targetTag == "EnemyMob" ? 8f : 10f;
        agent.SetDestination(targetPosition);
        StartCoroutine(WaitForDestination(onReachedPosition));
    }

    private IEnumerator WaitForDestination(System.Action onReachedPosition)
    {
        while (agent.pathPending || agent.remainingDistance > leniencyRadius)
        {
            yield return null;
        }
        onReachedPosition();
        ReturnToOriginalPosition();
    }

    private void ReturnToOriginalPosition()
    {
        agent.speed = 10f;
        agent.SetDestination(originalPosition);
        StartCoroutine(WaitForReturn());
    }

    private IEnumerator WaitForReturn()
    {
        while (agent.pathPending || agent.remainingDistance > returnThreshold)
        {
            yield return null;
        }
        isMovingToAttackPosition = false;
        hasActed = false;
    }

    private void PerformAttack(string actionName, string targetTag)
    {
        GameObject target = GameObject.FindWithTag(targetTag);
        if (target != null)
        {
            SpellDefinition spellDef = spellManager.GetSpellDefinition(actionName);
            int damage = spellDef != null ? spellDef.GetDamageForLevel(aiSkillTree.GetSkillLevel(actionName)) : aiCardManager.GetSpellDamage(actionName);

            if (targetTag == "Player")
            {
                PlayerController targetPlayerController = target.GetComponent<PlayerController>();
                if (targetPlayerController != null)
                {
                    targetPlayerController.TakeDamage(damage);
                    Debug.Log($"AI used {actionName} on Player for {damage} damage!");
                    ShowFloatingDamage(target.transform.position, damage);
                }
            }
            else if (targetTag == "EnemyMob")
            {
                AIMob aiMob = target.GetComponent<AIMob>();
                if (aiMob != null)
                {
                    aiMob.TakeDamage(damage, gameObject);
                    Debug.Log($"AI used {actionName} on EnemyMob for {damage} damage!");
                    ShowFloatingDamage(target.transform.position, damage);
                }
            }
            else if (targetTag == "MiniBoss")
            {
                MiniBoss miniBoss = target.GetComponent<MiniBoss>();
                if (miniBoss != null)
                {
                    miniBoss.TakeDamage(damage, gameObject);
                    Debug.Log($"AI used {actionName} on MiniBoss for {damage} damage!");
                    ShowFloatingDamage(target.transform.position, damage);
                }
            }
        }
        else
        {
            Debug.LogWarning($"Target {targetTag} not found in the scene.");
        }
        hasActed = true;
    }

    private void UseItem(string itemName)
    {
        if (itemName == "HealthPotion")
        {
            if (playerController.health + 50 > 500)
            {
                playerController.health = 500;
            }
            else
            {
                playerController.health += 50;
            }
            Debug.Log($"AI used Health Potion. New health: {playerController.health}");
        }
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
            }
            else
            {
                Debug.LogError("FloatingDamageText component not found on the instantiated prefab.");
            }
        }
        else
        {
            Debug.LogWarning("Floating text prefab is not assigned in the AIController.");
        }
    }

    public void UpdateAISkillPointsText()
    {
        if (aiSkillPointsText != null)
        {
            aiSkillPointsText.text = $"AI Skill Points: {aiSkillTree.GetSkillPoints()}";
        }
        else
        {
            Debug.LogWarning("AI Skill Points Text is not assigned in the AIController.");
        }
    }
}

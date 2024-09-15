using UnityEngine;
using UnityEngine.UI;

public class LevelDisplay : MonoBehaviour
{
    public Text playerLevelText;
    public Text mobLevelText;
    public Text miniBossLevelText;
    public Text opponentAILevelText;

    public XPManager playerXPManager;
    public XPManager opponentAIXPManager;
    public Mob mob;
    public MiniBoss miniBoss;
    public Text aiMobLevelText;
    private AIMob aiMob;

    private void Start()
    {
        if (playerXPManager == null) playerXPManager = FindObjectOfType<PlayerController>()?.GetComponent<XPManager>();
        if (opponentAIXPManager == null) opponentAIXPManager = FindObjectOfType<AIController>()?.GetComponent<XPManager>();
        if (mob == null) mob = FindObjectOfType<Mob>();
        if (miniBoss == null) miniBoss = FindObjectOfType<MiniBoss>();
        if (aiMob == null) aiMob = FindObjectOfType<AIMob>();
        if (playerLevelText == null || mobLevelText == null || miniBossLevelText == null || opponentAILevelText == null)
        {
            Debug.LogError("One or more Text components are not assigned in the LevelDisplay script.");
        }

        if (playerXPManager == null || opponentAIXPManager == null)
        {
            Debug.LogError("XPManager components not found for player or AI opponent.");
        }
    }

    private void Update()
    {
        UpdateLevelTexts();
    }

    private void UpdateLevelTexts()
    {
        if (playerXPManager != null && playerLevelText != null)
        {
            playerLevelText.text = $"Level {playerXPManager.level}";
        }

        if (opponentAIXPManager != null && opponentAILevelText != null)
        {
            opponentAILevelText.text = $"Level {opponentAIXPManager.level}";
        }

        if (mob != null && mobLevelText != null)
        {
            mobLevelText.text = $"Level {mob.level}";
        }

        if (miniBoss != null && miniBossLevelText != null)
        {
            miniBossLevelText.text = $"Level {miniBoss.level}";
        }
        if (aiMob != null && aiMobLevelText != null)
        {
            aiMobLevelText.text = $"Level {aiMob.level}";
        }
    }
}

using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public PlayerController playerController;
    public AIController aiController;
    public CardManager cardManager;
    private bool isPlayerTurn = true;
    private int playerActionsRemaining = 2;
    public float aiTurnDelay = 2f;
    public int resourcePerTurn = 15;
    public int maxResource = 100; // Assuming 100 is the maximum resource

    void Start()
    {
        StartPlayerTurn();
    }

    public void PerformAction()
    {
        if (isPlayerTurn)
        {
            playerActionsRemaining--;
            Debug.Log($"Player has {playerActionsRemaining} action(s) remaining.");
            if (playerActionsRemaining <= 0)
            {
                EndPlayerTurn();
            }
        }
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        playerActionsRemaining = 2;
        AddPlayerResource();
        playerController.EnablePlayerInput();
        cardManager.EnableSpellCasting();
        Debug.Log("Player's turn started. 2 actions available.");
    }

    private void AddPlayerResource()
    {
        int currentResource = playerController.resource;
        int newResource = Mathf.Min(currentResource + resourcePerTurn, maxResource);
        playerController.resource = newResource;
        Debug.Log($"Player resource increased from {currentResource} to {newResource}");
    }

    private void EndPlayerTurn()
    {
        isPlayerTurn = false;
        playerController.DisablePlayerInput();
        cardManager.DisableSpellCasting();
        StartCoroutine(StartAITurnAfterDelay());
    }

    private IEnumerator StartAITurnAfterDelay()
    {
        Debug.Log($"Waiting {aiTurnDelay} seconds before starting AI turn...");
        yield return new WaitForSeconds(aiTurnDelay);
        StartAITurn();
    }

    private void StartAITurn()
    {
        Debug.Log("AI's turn started.");
        aiController.StartTurn();
    }

    public void EndAITurn()
    {
        StartPlayerTurn();
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }

    public int GetPlayerActionsRemaining()
    {
        return playerActionsRemaining;
    }
}

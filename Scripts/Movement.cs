using UnityEngine;
using UnityEngine.AI;

public class MoveToPosition : MonoBehaviour
{
    public NavMeshAgent playerAgent; // Reference to the player's NavMeshAgent
    public Vector3 fixedPosition = new Vector3(50f, 3f, 47f); // Fixed position to move to

    public void MoveToFixedPosition()
    {
        if (playerAgent != null)
        {
            playerAgent.SetDestination(fixedPosition);
            Debug.Log("Player is moving to position: " + fixedPosition);
        }
        else
        {
            Debug.LogError("Player NavMeshAgent is not assigned.");
        }
    }
}

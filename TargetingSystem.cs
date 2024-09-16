using UnityEngine;
using UnityEngine.UI;

public class TargetingSystem : MonoBehaviour
{
    private Transform currentTarget;
    public CardManager cardManager;
    public Material glowMaterial;
    public float glowScale = 1.05f;
    public Text targetInfoText; // New public variable for the UI Text

    private GameObject currentGlowObject;

    void Start()
    {
        if (cardManager == null)
        {
            cardManager = FindObjectOfType<CardManager>();
            if (cardManager == null)
            {
                Debug.LogError("CardManager not found in the scene.");
            }
        }
        UpdateTargetInfoText(); // Initialize the target info text
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Transform hitTransform = hit.transform;
                Transform targetRoot = FindTargetRoot(hitTransform);

                if (targetRoot != null)
                {
                    SetNewTarget(targetRoot);
                }
            }
        }
    }

    private Transform FindTargetRoot(Transform hitTransform)
    {
        while (hitTransform != null)
        {
            if (hitTransform.CompareTag("Opponent") ||
                hitTransform.CompareTag("Mob") ||
                hitTransform.CompareTag("MiniBoss"))
            {
                return hitTransform;
            }
            hitTransform = hitTransform.parent;
        }
        return null;
    }

    private void SetNewTarget(Transform newTarget)
    {
        if (currentTarget != newTarget)
        {
            ClearTarget();
            currentTarget = newTarget;
            Debug.Log("Target selected: " + currentTarget.name);

            // Create glow effect
            currentGlowObject = CreateGlowObject(currentTarget);

            // Set the target in CardManager
            if (cardManager != null)
            {
                cardManager.SetSelectedTarget(currentTarget);
            }
            else
            {
                Debug.LogError("CardManager is null in TargetingSystem.");
            }

            UpdateTargetInfoText(); // Update the target info text
        }
    }

    public void ClearTarget()
    {
        if (currentGlowObject != null)
        {
            Destroy(currentGlowObject);
        }
        currentTarget = null;
        if (cardManager != null)
        {
            cardManager.SetSelectedTarget(null);
        }
        UpdateTargetInfoText(); // Update the target info text
    }

    private void UpdateTargetInfoText()
    {
        if (targetInfoText != null)
        {
            targetInfoText.text = currentTarget == null ? "Select A Target" : currentTarget.name;
        }
    }

    private GameObject CreateGlowObject(Transform target)
    {
        // Find the first child with a MeshFilter or SkinnedMeshRenderer
        Transform glowTarget = FindGlowTarget(target);

        if (glowTarget == null)
        {
            Debug.LogWarning("No suitable object found for glow effect on " + target.name);
            return null;
        }

        GameObject glowObject = new GameObject(target.name + "_Glow");
        glowObject.transform.SetParent(glowTarget);
        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localRotation = Quaternion.identity;
        glowObject.transform.localScale = Vector3.one * glowScale;

        MeshFilter meshFilter = glowObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = glowObject.AddComponent<MeshRenderer>();

        MeshFilter targetMeshFilter = glowTarget.GetComponent<MeshFilter>();
        SkinnedMeshRenderer skinnedMeshRenderer = glowTarget.GetComponent<SkinnedMeshRenderer>();

        if (targetMeshFilter != null)
        {
            meshFilter.mesh = targetMeshFilter.mesh;
        }
        else if (skinnedMeshRenderer != null)
        {
            meshFilter.mesh = skinnedMeshRenderer.sharedMesh;
        }
        else
        {
            Debug.LogWarning("Target does not have a MeshFilter or SkinnedMeshRenderer component.");
            Destroy(glowObject);
            return null;
        }

        meshRenderer.material = glowMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        return glowObject;
    }

    private Transform FindGlowTarget(Transform root)
    {
        MeshFilter meshFilter = root.GetComponentInChildren<MeshFilter>();
        if (meshFilter != null)
        {
            return meshFilter.transform;
        }

        SkinnedMeshRenderer skinnedMeshRenderer = root.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null)
        {
            return skinnedMeshRenderer.transform;
        }

        return null;
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }
}

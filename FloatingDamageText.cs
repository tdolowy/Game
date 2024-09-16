using UnityEngine;
using TMPro;

public class FloatingDamageText : MonoBehaviour
{
    public float destroyTime = 1f;
    public Vector3 offset = new Vector3(0, 2f, 0);
    public Vector3 randomizeIntensity = new Vector3(0.5f, 0, 0);

    private TextMeshPro textMesh;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMeshPro>();
            Debug.Log("Added TextMeshPro component to FloatingDamageText");
        }
        else
        {
            Debug.Log("TextMeshPro component found on FloatingDamageText");
        }
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 36;
    }

    public void SetDamageText(int damage)
    {
        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
            Debug.Log($"Setting damage text to: {damage}");
        }
        else
        {
            Debug.LogError("TextMeshPro component is missing on FloatingDamageText.");
        }
    }

    void Start()
    {
        Destroy(gameObject, destroyTime);

        transform.localPosition += offset;
        transform.localPosition += new Vector3(
            Random.Range(-randomizeIntensity.x, randomizeIntensity.x),
            Random.Range(-randomizeIntensity.y, randomizeIntensity.y),
            Random.Range(-randomizeIntensity.z, randomizeIntensity.z)
        );
    }

    void Update()
    {
        transform.position += Vector3.up * Time.deltaTime;
    }
}
public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);
    }
}

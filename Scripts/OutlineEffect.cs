using UnityEngine;

public class OutlineEffect : MonoBehaviour
{
    public enum Mode
    {
        OutlineAll,
        OutlineVisible,
        OutlineHidden,
        OutlineAndSilhouette,
        SilhouetteOnly
    }

    public Mode OutlineMode { get; set; }
    public Color OutlineColor { get; set; }
    public float OutlineWidth { get; set; }

    private Material outlineMaskMaterial;
    private Material outlineFillMaterial;
    private Renderer[] renderers;
    private bool needsUpdate;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        outlineMaskMaterial = CreateMaterial(new Color(0, 0, 0, 0));
        outlineFillMaterial = CreateMaterial(Color.black);

        needsUpdate = true;
    }

    void OnValidate()
    {
        needsUpdate = true;
    }

    void Update()
    {
        if (needsUpdate)
        {
            needsUpdate = false;

            UpdateMaterialProperties();
        }
    }

    void OnRenderObject()
    {
        if (OutlineMode == Mode.OutlineAll || OutlineMode == Mode.OutlineVisible)
        {
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);

            foreach (var renderer in renderers)
            {
                if (renderer.enabled)
                {
                    DrawOutline(renderer);
                }
            }

            GL.PopMatrix();
        }
    }

    void DrawOutline(Renderer renderer)
    {
        var materials = renderer.sharedMaterials;

        for (int i = 0; i < materials.Length; i++)
        {
            if (OutlineMode == Mode.OutlineAll || OutlineMode == Mode.OutlineVisible)
            {
                Graphics.DrawMesh(renderer.GetComponent<MeshFilter>().sharedMesh, renderer.transform.localToWorldMatrix, outlineMaskMaterial, 0, Camera.current, i);
                Graphics.DrawMesh(renderer.GetComponent<MeshFilter>().sharedMesh, renderer.transform.localToWorldMatrix, outlineFillMaterial, 0, Camera.current, i);
            }
        }
    }

    Material CreateMaterial(Color color)
    {
        var material = new Material(Shader.Find("Hidden/Internal-Colored"));
        material.hideFlags = HideFlags.HideAndDontSave;
        material.SetColor("_Color", color);
        return material;
    }

    void UpdateMaterialProperties()
    {
        outlineFillMaterial.SetColor("_Color", OutlineColor);
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_OutlineWidth", OutlineWidth);
    }
}

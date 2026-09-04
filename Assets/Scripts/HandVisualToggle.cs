using UnityEngine;

public class HandVisualToggle : MonoBehaviour
{
    [Header("Skinned Mesh Renderers for Hands")]
    [SerializeField] private SkinnedMeshRenderer[] handMeshes;

    private bool isVisible = true;

    public void ToggleHands()
    {
        isVisible = !isVisible;
        foreach (var mesh in handMeshes)
        {
            if (mesh != null)
            {
                mesh.enabled = isVisible;
            }
        }
    }
}
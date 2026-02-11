using UnityEngine;

namespace Game.Client.Resources
{
    /// <summary>
    /// Візуальне представлення блоку ресурсу.
    /// </summary>
    public sealed class ResourceBlockView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        public void Apply(ResourceLibrary.Entry visual)
        {
            if (meshRenderer != null && visual.material != null)
                meshRenderer.sharedMaterial = visual.material;

            if (visual.scale != Vector3.zero)
                transform.localScale = visual.scale;
        }
    }
}
using UnityEngine;

namespace PipeGuideTwin.Visualization
{
    // Model3D uses a material override; validation mode adds an independent cyan outline.
    // SetBaseMaterials updates the underlying visual state even while selected.
    public sealed class SelectionHighlight : MonoBehaviour
    {
        [SerializeField] private Material selectionMaterial;
        private Renderer[] targets;
        private Material[][] baseMaterials;
        private bool overlayMode;
        private LineRenderer outline;
        public bool IsSelected { get; private set; }
        public void ConfigureSelectionMaterial(Material material) => selectionMaterial = material;

        private void Awake() => Cache();
        private void Cache()
        {
            if (targets != null) return;
            targets = GetComponentsInChildren<Renderer>(true);
            baseMaterials = new Material[targets.Length][];
            for (int i = 0; i < targets.Length; i++) baseMaterials[i] = targets[i].sharedMaterials;
        }
        public void SetSelected(bool selected)
        {
            Cache();
            IsSelected = selected;
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null) continue;
                if (!selected || overlayMode || selectionMaterial == null) targets[i].sharedMaterials = baseMaterials[i];
                else
                {
                    var materials = new Material[baseMaterials[i].Length];
                    for (int j = 0; j < materials.Length; j++) materials[j] = selectionMaterial;
                    targets[i].sharedMaterials = materials;
                }
            }
            if (outline != null) outline.enabled = selected && overlayMode;
            if (selected && overlayMode) UpdateOutline();
        }
        public void SetOverlayMode(bool enabled)
        {
            Cache(); overlayMode = enabled;
            if (enabled && outline == null)
            {
                var child = new GameObject("SelectionOutline");
                child.transform.SetParent(transform, false);
                outline = child.AddComponent<LineRenderer>();
                outline.sharedMaterial = selectionMaterial;
                outline.useWorldSpace = true; outline.widthMultiplier = .04f;
                outline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                outline.receiveShadows = false; outline.positionCount = 16;
            }
            SetSelected(IsSelected);
            if (outline != null) outline.enabled = IsSelected && overlayMode;
        }
        private void LateUpdate() { if (IsSelected && overlayMode) UpdateOutline(); }
        private void UpdateOutline()
        {
            if (outline == null || targets.Length == 0) return;
            var bounds = targets[0].bounds;
            foreach (var target in targets) if (target != null) bounds.Encapsulate(target.bounds);
            bounds.Expand(.10f);
            var lo = bounds.min; var hi = bounds.max;
            var p = new[] { new Vector3(lo.x,lo.y,lo.z), new Vector3(hi.x,lo.y,lo.z), new Vector3(hi.x,hi.y,lo.z), new Vector3(lo.x,hi.y,lo.z), new Vector3(lo.x,lo.y,hi.z), new Vector3(hi.x,lo.y,hi.z), new Vector3(hi.x,hi.y,hi.z), new Vector3(lo.x,hi.y,hi.z) };
            var order = new[] {0,1,2,3,0,4,5,1,5,6,2,6,7,3,7,4};
            for (int i=0;i<order.Length;i++) outline.SetPosition(i,p[order[i]]);
            outline.enabled = true;
        }
        public void SetBaseMaterials(Renderer target, Material[] materials)
        {
            Cache();
            for (int i = 0; i < targets.Length; i++)
                if (targets[i] == target)
                {
                    baseMaterials[i] = (Material[])materials.Clone();
                    if (!IsSelected || overlayMode) target.sharedMaterials = baseMaterials[i];
                    return;
                }
        }
        private void OnDisable() => SetSelected(false);
    }
}

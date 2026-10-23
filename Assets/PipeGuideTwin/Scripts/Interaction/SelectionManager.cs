using System;
using UnityEngine;
using UnityEngine.InputSystem;
using PipeGuideTwin.Visualization;

namespace PipeGuideTwin.Interaction
{
    public sealed class SelectionManager : MonoBehaviour
    {
        [SerializeField] private Camera viewerCamera;
        public PipeGuideSelectable Current { get; private set; }
        public event Action<PipeGuideSelectable> SelectionChanged;
        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                SelectAtScreenPoint(mouse.position.ReadValue());
            if (!ReferenceEquals(Current, null) && Current == null) SetSelection(null);
        }
        public void SelectAtScreenPoint(Vector2 position)
        {
            if (viewerCamera == null || !viewerCamera.pixelRect.Contains(position) || ViewerPointer.IsOverUI(position)) return;
            PipeGuideSelectable target = null;
            if (Physics.Raycast(viewerCamera.ScreenPointToRay(position), out var hit, viewerCamera.farClipPlane, ~0, QueryTriggerInteraction.Ignore))
                target = hit.collider.GetComponentInParent<PipeGuideSelectable>();
            SetSelection(target);
        }
        public void Select(PipeGuideSelectable target) => SetSelection(target);
        private void SetSelection(PipeGuideSelectable target)
        {
            if (ReferenceEquals(Current, target)) { SelectionChanged?.Invoke(Current); return; }
            if (Current != null) Current.GetComponent<SelectionHighlight>()?.SetSelected(false);
            Current = target;
            if (Current != null) Current.GetComponent<SelectionHighlight>()?.SetSelected(true);
            SelectionChanged?.Invoke(Current);
        }
        private void OnDisable() => SetSelection(null);
    }
}

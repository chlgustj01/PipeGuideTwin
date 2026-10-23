using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PipeGuideTwin.Core;
using PipeGuideTwin.Data;
using PipeGuideTwin.Interaction;
using PipeGuideTwin.Validation;

namespace PipeGuideTwin.Visualization
{
    public sealed class ValidationVisualController : MonoBehaviour
    {
        [SerializeField] private PipeGuideAppManager app;
        [SerializeField] private ValidationResultController results;
        [SerializeField] private Transform markerRoot;
        [SerializeField] private Material warningMaterial;
        [SerializeField] private Material mismatchMaterial;
        [SerializeField] private Material duplicateMaterial;
        [SerializeField] private Material missingMaterial;
        [SerializeField] private Material selectionMaterial;
        private readonly Dictionary<Renderer, Material[]> originals = new Dictionary<Renderer, Material[]>();
        private readonly Dictionary<string, SelectionHighlight> markers = new Dictionary<string, SelectionHighlight>();
        public bool IsActive => app.CurrentStage == WorkflowStage.ConsistencyCheck;
        public SelectionHighlight MarkerFor(string issueId) => markers.TryGetValue(issueId, out var marker) ? marker : null;
        private void Awake()
        {
            foreach (var item in UnityEngine.Object.FindObjectsByType<PipeGuideSelectable>(FindObjectsSortMode.None))
                foreach (var renderer in item.GetComponentsInChildren<Renderer>()) originals[renderer] = renderer.sharedMaterials;
        }
        private void OnEnable()
        {
            app.StageChanged += StageChanged;
            results.ResultsChanged += Apply;
            results.IssueSelectionChanged += HighlightMarker;
        }
        private void Start() => Apply();
        private void OnDisable()
        {
            app.StageChanged -= StageChanged;
            results.ResultsChanged -= Apply;
            results.IssueSelectionChanged -= HighlightMarker;
        }
        private void StageChanged(WorkflowStage stage) => Apply();
        private Material MaterialFor(ValidationStatus status) => status switch
        {
            ValidationStatus.Warning => warningMaterial, ValidationStatus.Mismatch => mismatchMaterial,
            ValidationStatus.Duplicate => duplicateMaterial, _ => null
        };
        public void Apply()
        {
            foreach (var item in UnityEngine.Object.FindObjectsByType<PipeGuideSelectable>(FindObjectsSortMode.None))
            {
                var highlight = item.GetComponent<SelectionHighlight>();
                var issue = results.IssueFor(item);
                var material = IsActive && issue != null ? MaterialFor(issue.Status) : null;
                foreach (var renderer in item.GetComponentsInChildren<Renderer>())
                {
                    if (!originals.TryGetValue(renderer, out var original)) continue;
                    var current = material == null ? original : Enumerable.Repeat(material, original.Length).ToArray();
                    highlight.SetBaseMaterials(renderer, current);
                }
                highlight.SetOverlayMode(IsActive);
            }
            RebuildMarkers();
        }
        private void RebuildMarkers()
        {
            foreach (var marker in markers.Values)
                if (marker != null) { marker.gameObject.SetActive(false); Destroy(marker.gameObject); }
            markers.Clear();
            markerRoot.gameObject.SetActive(IsActive);
            if (!IsActive) return;
            foreach (var issue in results.Issues)
            {
                if (issue.Status != ValidationStatus.Missing || !issue.HasExpectedPosition) continue;
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = "Missing_" + issue.ObjectId;
                go.transform.SetParent(markerRoot, false);
                go.transform.position = EngineeringUnitConverter.PositionMmToUnity(issue.ExpectedPositionMm);
                go.transform.localScale = Vector3.one * .55f;
                Destroy(go.GetComponent<Collider>());
                go.GetComponent<Renderer>().sharedMaterial = missingMaterial;
                var highlight = go.AddComponent<SelectionHighlight>();
                highlight.ConfigureSelectionMaterial(selectionMaterial);
                highlight.SetOverlayMode(true);
                markers[issue.IssueId] = highlight;
            }
            HighlightMarker();
        }
        private void HighlightMarker()
        {
            foreach (var pair in markers)
                pair.Value.SetSelected(IsActive && results.SelectedIssue != null && pair.Key == results.SelectedIssue.IssueId);
        }
    }
}

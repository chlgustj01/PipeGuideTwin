using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PipeGuideTwin.Data;
using PipeGuideTwin.Data.Contracts;
using PipeGuideTwin.Data.Mock;
using PipeGuideTwin.Interaction;

namespace PipeGuideTwin.Validation
{
    public sealed class ValidationResultController : MonoBehaviour
    {
        [SerializeField] private SelectionManager selection;
        private ValidationIssue[] issues = Array.Empty<ValidationIssue>();
        private bool selectingIssue;
        public IReadOnlyList<ValidationIssue> Issues => issues;
        public ValidationIssue SelectedIssue { get; private set; }
        public event Action ResultsChanged;
        public event Action IssueSelectionChanged;
        public int ProblemCount => issues.Count(x => x.Status != ValidationStatus.Match && x.Status != ValidationStatus.Unknown);
        public int HighCount => issues.Count(x => x.Status != ValidationStatus.Match && x.Severity == "High");
        private void Awake() => ReplaceResults(MockValidationDataProvider.GetDemoIssues());
        private void OnEnable() { if (selection != null) selection.SelectionChanged += OnObjectSelected; }
        private void OnDisable() { if (selection != null) selection.SelectionChanged -= OnObjectSelected; }
        // Future B output enters here. This accepts reported statuses; it never compares design data.
        public void ReplaceResults(IEnumerable<ValidationIssue> results)
        {
            issues = results == null ? Array.Empty<ValidationIssue>() : results.Where(x => x != null).ToArray();
            SelectedIssue = IssueFor(selection != null ? selection.Current : null);
            ResultsChanged?.Invoke(); IssueSelectionChanged?.Invoke();
        }
        public ValidationIssue IssueFor(PipeGuideSelectable item) => item == null ? null : issues.FirstOrDefault(x => x.ObjectId == item.ObjectId);
        public void SelectIssue(ValidationIssue issue)
        {
            if (issue == null || !issues.Contains(issue)) return;
            var target = UnityEngine.Object.FindObjectsByType<PipeGuideSelectable>(FindObjectsSortMode.None).FirstOrDefault(x => x.ObjectId == issue.ObjectId);
            selectingIssue = true;
            try { selection.Select(target); } finally { selectingIssue = false; }
            SelectedIssue = issue;
            IssueSelectionChanged?.Invoke();
        }
        private void OnObjectSelected(PipeGuideSelectable item)
        {
            if (selectingIssue) return;
            SelectedIssue = IssueFor(item);
            IssueSelectionChanged?.Invoke();
        }
    }
}

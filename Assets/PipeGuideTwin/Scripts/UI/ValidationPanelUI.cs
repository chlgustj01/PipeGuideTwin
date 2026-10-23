using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using PipeGuideTwin.Data;
using PipeGuideTwin.Validation;

namespace PipeGuideTwin.UI
{
    public sealed class ValidationPanelUI : MonoBehaviour
    {
        [SerializeField] private ValidationResultController results;
        [SerializeField] private Text summary;
        [SerializeField] private Transform listRoot;
        [SerializeField] private Button rowTemplate;
        [SerializeField] private float rowSpacing=100;
        [SerializeField] private PipeGuideTheme theme;
        private void OnEnable()
        {
            results.ResultsChanged += Rebuild;
            Rebuild();
        }
        private void OnDisable() => results.ResultsChanged -= Rebuild;
        private void Rebuild()
        {
            foreach (Transform child in listRoot)
                if (child != rowTemplate.transform) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var all = results.Issues;
            summary.text = $"검사 대상: {all.Count}   정상: {all.Count(x=>x.Status == ValidationStatus.Match)}\n문제: {results.ProblemCount}   높음: {results.HighCount}\n누락 {all.Count(x=>x.Status == ValidationStatus.Missing)} · 불일치 {all.Count(x=>x.Status == ValidationStatus.Mismatch)}\n중복 {all.Count(x=>x.Status == ValidationStatus.Duplicate)} · 확인 필요 {all.Count(x=>x.Status == ValidationStatus.Warning)}";
            int index=0;
            foreach (var issue in all.Where(x=>x.Status != ValidationStatus.Match && x.Status != ValidationStatus.Unknown).OrderByDescending(x=>x.Severity == "High"))
            {
                var row=Instantiate(rowTemplate,listRoot);
                row.name = "Issue_" + issue.IssueId;
                row.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-index++ * rowSpacing);
                row.GetComponentInChildren<Text>().text = $"{issue.ObjectId}\n{issue.Title}\n{ValidationLabels.Value(issue.ExpectedValue)} → {ValidationLabels.Value(issue.ActualValue)}";
                var chip=row.transform.Find("SeverityChip");
                if(chip!=null){var label=chip.GetComponentInChildren<Text>();label.text=ValidationLabels.Severity(issue.Severity);label.color=issue.Severity=="High"?theme.DangerRed:theme.WarningYellow;}
                row.onClick.AddListener(()=>results.SelectIssue(issue));
                row.gameObject.SetActive(true);
            }
        }
    }
}

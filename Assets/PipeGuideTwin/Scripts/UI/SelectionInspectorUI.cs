using UnityEngine;
using UnityEngine.UI;
using PipeGuideTwin.Interaction;

namespace PipeGuideTwin.UI
{
    public sealed class SelectionInspectorUI : MonoBehaviour
    {
        [SerializeField] private SelectionManager selection;
        [SerializeField] private Text inspectorText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text debugText;
        [SerializeField] private Core.PipeGuideAppManager app;
        [SerializeField] private Validation.ValidationResultController validation;
        private void OnEnable()
        {
            if (selection == null) return;
            selection.SelectionChanged += Refresh;
            if (app != null) app.StageChanged += OnStageChanged;
            if (validation != null)
            {
                validation.IssueSelectionChanged += OnValidationChanged;
                validation.ResultsChanged += OnValidationChanged;
            }
            Refresh(selection.Current);
        }
        private void OnDisable()
        {
            if (selection != null) selection.SelectionChanged -= Refresh;
            if (app != null) app.StageChanged -= OnStageChanged;
            if (validation != null)
            {
                validation.IssueSelectionChanged -= OnValidationChanged;
                validation.ResultsChanged -= OnValidationChanged;
            }
        }
        private void OnStageChanged(Data.WorkflowStage stage) => Refresh(selection.Current);
        private void OnValidationChanged() => Refresh(selection.Current);
        private void Refresh(PipeGuideSelectable item)
        {
            if (app != null && app.CurrentStage != Data.WorkflowStage.Model3D && app.CurrentStage != Data.WorkflowStage.ConsistencyCheck) return;
            if (app != null && validation != null && app.CurrentStage == Data.WorkflowStage.ConsistencyCheck)
            {
                RefreshValidation(item);
                return;
            }
            string id = item != null ? item.ObjectId : "없음";
            if (inspectorText != null)
            {
                inspectorText.text = item == null ? "객체 선택\n\n배관 또는 장비를\n선택하세요." :
                    $"선택한 객체\n\n식별자:\n{item.ObjectId}\n\n이름:\n{item.DisplayName}\n\n유형: {TypeLabel(item.ObjectType)}";
                if (item != null && item.ObjectType == Data.PIDComponentType.Pipe)
                    inspectorText.text += $"\n\n직경: {item.DiameterMm:0.##} mm\n재질: {(item.Material == "Carbon Steel" ? "탄소강" : item.Material)}";
            }
            if (statusText != null) statusText.text = (item == null ? "상태: 준비 완료" : "선택: " + id) + "   |   데모 데이터   |   설계 단위: mm / 화면 단위: m";
            if (debugText != null) debugText.text = $"파이프가이드 트윈\n\n단계: 3D 모델\n데이터: 데모\n선택: {id}\n상태: 준비 완료";
        }
        private void RefreshValidation(PipeGuideSelectable item)
        {
            var issue = validation.SelectedIssue ?? validation.IssueFor(item);
            if (issue == null)
                inspectorText.text = item == null ? "정합성 검사 결과\n\n문제 목록 또는 3D 배관을\n선택하세요." : $"정합성 검사 결과\n\n객체: {item.ObjectId}\n\n전달된 검사 결과 없음";
            else
                inspectorText.text = $"정합성 검사 결과\n\n객체: {issue.ObjectId}\n상태: {ValidationLabels.Status(issue.Status)}\n항목: {ValidationLabels.Category(issue.Category)}\n\n설계값:\n{ValidationLabels.Value(issue.ExpectedValue)}\n\n3D 값:\n{ValidationLabels.Value(issue.ActualValue)}\n\n심각도: {ValidationLabels.Severity(issue.Severity)}\n\n{issue.Description}";
            statusText.text = issue == null ? $"정합성 검사 | 문제 {validation.ProblemCount}건 | 높음 {validation.HighCount}건 | 데모 데이터" : $"선택: {issue.ObjectId} | {issue.Title} | 심각도: {ValidationLabels.Severity(issue.Severity)}";
            debugText.text = $"파이프가이드 트윈\n\n단계: 정합성 검사\n데이터: 데모\n문제: {validation.ProblemCount}건\n선택 문제: {issue?.IssueId ?? "없음"}";
        }
        private static string TypeLabel(Data.PIDComponentType type) => type switch
        {
            Data.PIDComponentType.Pipe => "배관",
            Data.PIDComponentType.Valve => "밸브",
            Data.PIDComponentType.Pump => "펌프",
            Data.PIDComponentType.Tank => "탱크",
            Data.PIDComponentType.Equipment => "장비",
            Data.PIDComponentType.Junction => "분기점",
            Data.PIDComponentType.Nozzle => "노즐",
            _ => "미분류"
        };
    }
}

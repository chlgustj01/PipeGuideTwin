using UnityEngine;
using UnityEngine.UI;
using PipeGuideTwin.Core;
using PipeGuideTwin.Data;
using PipeGuideTwin.Interaction;
using PipeGuideTwin.Validation;

namespace PipeGuideTwin.UI
{
    public sealed class StructuredInspectorUI : MonoBehaviour
    {
        public PipeGuideAppManager App;
        public SelectionManager Selection;
        public ValidationResultController Validation;
        public PipeGuideTheme Theme;
        public Text Heading,Identifier,Name,Chip,FirstLabel,FirstValue,SecondLabel,SecondValue,ThirdLabel,ThirdValue,FourthLabel,FourthValue;
        public Text Health;
        public Button FocusButton;
        public EngineeringCameraController CameraController;
        private void OnEnable()
        {
            Selection.SelectionChanged+=Selected;Validation.IssueSelectionChanged+=Refresh;Validation.ResultsChanged+=Refresh;App.StageChanged+=Stage;
            FocusButton.onClick.AddListener(Focus);Refresh();
        }
        private void OnDisable()
        {
            Selection.SelectionChanged-=Selected;Validation.IssueSelectionChanged-=Refresh;Validation.ResultsChanged-=Refresh;App.StageChanged-=Stage;
            FocusButton.onClick.RemoveListener(Focus);
        }
        private void Selected(PipeGuideSelectable item)=>Refresh();
        private void Stage(WorkflowStage stage)=>Refresh();
        private void Refresh()
        {
            bool validation=App.CurrentStage==WorkflowStage.ConsistencyCheck;
            var item=Selection.Current;var issue=Validation.SelectedIssue??Validation.IssueFor(item);
            Heading.text=validation?"이슈 상세":"선택한 객체";
            Identifier.text=validation?issue?.IssueId??"이슈 선택":item?.ObjectId??"객체 선택";
            Name.text=validation?issue==null?"문제 목록에서 선택하세요":issue.ObjectId+" · "+issue.Title:item?.DisplayName??"배관 또는 장비를 클릭하세요";
            Chip.text=validation?issue==null?"검토 대기":ValidationLabels.Status(issue.Status):item==null?"선택 대기":item.ObjectType==PIDComponentType.Pipe?"배관":"장비";
            Chip.color=validation&&issue!=null?(issue.Severity=="High"?Theme.DangerRed:Theme.WarningYellow):Theme.SelectionCyan;
            FirstLabel.text=validation?"설계값":"직경";FirstValue.text=validation?ValidationLabels.Value(issue?.ExpectedValue)??"—":item!=null&&item.ObjectType==PIDComponentType.Pipe?item.DiameterMm+" mm":"—";
            SecondLabel.text=validation?"3D 값":"재질";SecondValue.text=validation?ValidationLabels.Value(issue?.ActualValue)??"—":item==null?"—":ValidationLabels.Value(item.Material);
            ThirdLabel.text=validation?"차이":"시작 노드";ThirdValue.text=validation?issue?.DifferenceValue??"—":string.IsNullOrEmpty(item?.StartNodeId)?"—":item.StartNodeId;
            FourthLabel.text=validation?"심각도":"끝 노드";FourthValue.text=validation?ValidationLabels.Severity(issue?.Severity??"None"):string.IsNullOrEmpty(item?.EndNodeId)?"—":item.EndNodeId;
            Health.text=validation?issue==null?$"검사 결과 {Validation.Issues.Count}건":"전달된 검사 결과":"상태  ●  활성";
            FocusButton.interactable=item!=null || validation&&issue!=null&&issue.HasExpectedPosition;
        }
        private void Focus()
        {
            var item=Selection.Current;
            if(item!=null){CameraController.SetPivot(item.transform.position);return;}
            var issue=Validation.SelectedIssue;
            if(issue!=null&&issue.HasExpectedPosition)CameraController.SetPivot(Visualization.EngineeringUnitConverter.PositionMmToUnity(issue.ExpectedPositionMm));
        }
    }
}

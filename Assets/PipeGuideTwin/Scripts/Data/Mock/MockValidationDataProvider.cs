using PipeGuideTwin.Data.Contracts;
using UnityEngine;

namespace PipeGuideTwin.Data.Mock
{
    public static class MockValidationDataProvider
    {
        public static ValidationIssue[] GetDemoIssues() => new[]
        {
            Issue("VAL_001", "PIPE_001", ValidationStatus.Match, "General", "설계값 일치", "전달받은 데모 결과에서 설계값과 모델값이 일치합니다.", "200 mm", "200 mm", "None"),
            Issue("VAL_002", "PIPE_002", ValidationStatus.Warning, "Material", "재질 확인", "재질 확인이 필요한 데모 결과입니다.", "Carbon Steel", "Stainless Steel", "Medium"),
            Issue("VAL_003", "PIPE_003", ValidationStatus.Mismatch, "Diameter", "직경 불일치", "설계 직경과 모델 직경이 다른 데모 결과입니다.", "200 mm", "150 mm", "High", "-50 mm"),
            Issue("VAL_004", "PIPE_004", ValidationStatus.Duplicate, "Duplicate Pipe", "중복 배관", "동일 배관이 중복된 것으로 전달받은 데모 결과입니다.", "1 Pipe", "2 Pipes", "Medium"),
            new ValidationIssue { IssueId = "VAL_005", ObjectId = "PIPE_005", Status = ValidationStatus.Missing, Category = "Missing Pipe", Title = "배관 누락", Description = "설계 데이터에는 있지만 3D 모델에는 없는 배관의 예상 위치입니다.", ExpectedValue = "PIPE_005 exists", ActualValue = "Not found", Severity = "High", HasExpectedPosition = true, ExpectedPositionMm = new Vector3(-1800, 1700, -2400) }
        };
        private static ValidationIssue Issue(string id, string objectId, ValidationStatus status, string category, string title, string description, string expected, string actual, string severity, string difference="—")
            => new ValidationIssue { IssueId=id, ObjectId=objectId, Status=status, Category=category, Title=title, Description=description, ExpectedValue=expected, ActualValue=actual, Severity=severity, DifferenceValue=difference };
    }
}

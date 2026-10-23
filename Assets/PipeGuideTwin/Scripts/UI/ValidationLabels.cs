using PipeGuideTwin.Data;

namespace PipeGuideTwin.UI
{
    public static class ValidationLabels
    {
        public static string Status(ValidationStatus value) => value switch
        {
            ValidationStatus.Match => "정상", ValidationStatus.Warning => "확인 필요",
            ValidationStatus.Mismatch => "불일치", ValidationStatus.Missing => "누락",
            ValidationStatus.Duplicate => "중복", _ => "미확인"
        };
        public static string Severity(string value) => value switch { "High" => "높음", "Medium" => "보통", "Low" => "낮음", "None" => "없음", _ => value };
        public static string Category(string value) => value switch { "General" => "일반", "Material" => "배관 재질", "Diameter" => "배관 직경", "Duplicate Pipe" => "중복 배관", "Missing Pipe" => "배관 누락", _ => value };
        public static string Value(string value) => value switch { "Carbon Steel" => "탄소강", "Stainless Steel" => "스테인리스강", "1 Pipe" => "배관 1개", "2 Pipes" => "배관 2개", "Not found" => "없음", _ => value != null && value.EndsWith(" exists") ? "존재 (" + value.Substring(0, value.Length - 7) + ")" : value };
    }
}

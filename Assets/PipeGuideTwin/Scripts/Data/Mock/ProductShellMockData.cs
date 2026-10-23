using System;
using UnityEngine;

namespace PipeGuideTwin.Data.Mock
{
    [Serializable] public sealed class ShellCardData
    {
        public string Title;
        [TextArea(3,14)] public string Body;
        public string Badge;
    }
    [Serializable] public sealed class ShellPageData
    {
        public WorkflowStage Stage;
        public string PageName;
        public string NavigationLabel;
        public string Title;
        public string Subtitle;
        public bool ShowsWorld;
        public bool SplitCenter;
        public ShellCardData[] Left;
        public ShellCardData[] Center;
        public ShellCardData[] Right;
    }
    [CreateAssetMenu(menuName="PipeGuide Twin/Product Shell Mock Data")]
    public sealed class ProductShellMockData : ScriptableObject
    {
        public string DataSource = "MOCK";
        public ShellPageData[] Pages;
    }
}

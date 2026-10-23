using UnityEngine;

namespace PipeGuideTwin.UI
{
    [CreateAssetMenu(menuName="PipeGuide Twin/UI Theme")]
    public sealed class PipeGuideTheme : ScriptableObject
    {
        public Color Background = new Color32(13,21,34,255);
        public Color Panel = new Color32(21,33,51,255);
        public Color Card = new Color32(27,42,62,255);
        public Color Border = new Color32(51,70,92,255);
        public Color PrimaryOrange = new Color32(255,153,61,255);
        public Color TextPrimary = new Color32(236,242,249,255);
        public Color TextSecondary = new Color32(162,181,203,255);
        public Color SuccessGreen = new Color32(90,210,153,255);
        public Color WarningYellow = new Color32(247,214,81,255);
        public Color DangerRed = new Color32(241,92,106,255);
        public Color SelectionCyan = new Color32(51,235,255,255);
        public Color CandidateBlue = new Color32(100,158,255,255);
        public Color OrangeHover = new Color32(255,182,99,255);
        public Color OrangeDuplicate = new Color32(242,112,48,255);
        public Color BlueInfo => CandidateBlue;
        public Color BackgroundDark => Background;
        public Color PanelDark => Panel;
        public Color PanelRaised => Card;
        public Color BorderSubtle => Border;
        public Color OrangePrimary => PrimaryOrange;
        public Color CyanSelection => SelectionCyan;
        public Color GreenSuccess => SuccessGreen;
        public Color YellowWarning => WarningYellow;
        public Color RedCritical => DangerRed;
    }
}

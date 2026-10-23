using UnityEngine;
using UnityEngine.UI;

namespace PipeGuideTwin.UI
{
    [ExecuteAlways]
    public sealed class ShellFont : MonoBehaviour
    {
        private Font font;
        private void OnEnable()
        {
            // Use installed Korean fonts without redistributing OS font files.
            font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Apple SD Gothic Neo", "Noto Sans CJK KR", "Arial" }, 24);
            foreach (var label in GetComponentsInChildren<Text>(true)) label.font = font;
        }
        private void OnDisable()
        {
            if (font == null) return;
            if (Application.isPlaying) Destroy(font); else DestroyImmediate(font);
        }
    }
}

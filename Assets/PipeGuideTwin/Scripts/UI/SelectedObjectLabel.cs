using UnityEngine;
using UnityEngine.UI;
using PipeGuideTwin.Interaction;

namespace PipeGuideTwin.UI
{
    public sealed class SelectedObjectLabel : MonoBehaviour
    {
        public SelectionManager Selection;
        public Camera ViewerCamera;
        public RectTransform Canvas;
        public Text Label;
        public Image Background;
        private Image[] images;
        private void Awake()=>images=GetComponentsInChildren<Image>(true);
        private void LateUpdate()
        {
            var item=Selection.Current;
            var p=item!=null?ViewerCamera.WorldToScreenPoint(item.transform.position+Vector3.up*1.65f):Vector3.zero;
            bool visible=item!=null&&p.z>0&&p.x>Screen.width*.18f&&p.x<Screen.width*.77f&&p.y>Screen.height*.12f&&p.y<Screen.height*.85f;
            Label.enabled=visible;foreach(var image in images)image.enabled=visible;if(!visible)return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Canvas,p,null,out var local);
            GetComponent<RectTransform>().anchoredPosition=local+new Vector2(0,28);
            Label.text=item.ObjectId+"\n"+item.DisplayName;
        }
    }
}

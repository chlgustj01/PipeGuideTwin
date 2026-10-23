using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PipeGuideTwin.Interaction
{
    public static class ViewerPointer
    {
        // Explicit screen-position raycast avoids stale IsPointerOverGameObject state
        // when InputSystem events and MonoBehaviour updates happen in different order.
        private static readonly List<RaycastResult> Hits = new List<RaycastResult>();
        public static bool IsOverUI(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            var pointer = new PointerEventData(EventSystem.current) { position = position };
            Hits.Clear();
            EventSystem.current.RaycastAll(pointer, Hits);
            for (int i = 0; i < Hits.Count; i++)
                if (Hits[i].module is UnityEngine.UI.GraphicRaycaster) return true;
            return false;
        }
    }
}

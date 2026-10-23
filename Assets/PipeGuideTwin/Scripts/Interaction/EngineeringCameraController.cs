using UnityEngine;
using UnityEngine.InputSystem;

namespace PipeGuideTwin.Interaction
{
    [RequireComponent(typeof(Camera))]
    public sealed class EngineeringCameraController : MonoBehaviour
    {
        [SerializeField] private Vector3 pivot = new Vector3(0, 1, 0);
        [SerializeField] private float minimumDistance = 3;
        [SerializeField] private float maximumDistance = 45;
        [SerializeField] private float orbitDegreesPerPixel = 0.22f;
        [SerializeField] private float zoomSensitivity = 0.0015f;
        private Camera viewerCamera;
        private float distance;
        private float yaw;
        private float pitch;
        private int dragButton;
        public Vector3 Pivot => pivot;
        public float Distance => distance;

        private void Awake()
        {
            viewerCamera = GetComponent<Camera>();
            SetPivot(pivot);
        }
        // Extension point for future selected-object focus; no automatic focus in STEP 1.
        public void SetPivot(Vector3 worldPosition)
        {
            pivot = worldPosition;
            var offset = transform.position - pivot;
            distance = Mathf.Clamp(offset.magnitude, minimumDistance, maximumDistance);
            if (offset.sqrMagnitude < 0.001f) offset = new Vector3(0, 1, -1);
            pitch = Mathf.Clamp(Mathf.Asin(offset.normalized.y) * Mathf.Rad2Deg, -80, 80);
            yaw = Mathf.Atan2(-offset.x, -offset.z) * Mathf.Rad2Deg;
            ApplyPose();
        }
        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            var position = mouse.position.ReadValue();
            bool available = viewerCamera.pixelRect.Contains(position) && !ViewerPointer.IsOverUI(position);
            if (!available) dragButton = 0;
            else if (mouse.rightButton.wasPressedThisFrame) dragButton = 1;
            else if (mouse.middleButton.wasPressedThisFrame) dragButton = 2;
            var delta = mouse.delta.ReadValue();
            if (dragButton == 1 && mouse.rightButton.isPressed)
            {
                yaw += delta.x * orbitDegreesPerPixel;
                pitch = Mathf.Clamp(pitch - delta.y * orbitDegreesPerPixel, -80, 80);
            }
            else if (dragButton == 2 && mouse.middleButton.isPressed)
            {
                float metersPerPixel = 2 * distance * Mathf.Tan(viewerCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) / Mathf.Max(1, viewerCamera.pixelHeight);
                pivot -= (transform.right * delta.x + transform.up * delta.y) * metersPerPixel;
            }
            else dragButton = 0;
            if (available)
                distance = Mathf.Clamp(distance * Mathf.Exp(-mouse.scroll.ReadValue().y * zoomSensitivity), minimumDistance, maximumDistance);
            ApplyPose();
        }
        private void ApplyPose()
        {
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            transform.position = pivot - transform.forward * distance;
        }
        private void OnDisable() => dragButton = 0;
    }
}

using UnityEngine;

namespace Presentation
{
    /// <summary>
    /// Combat stage root in Gameplay.
    /// </summary>
    public class CombatArea : MonoBehaviour
    {
        [SerializeField] private Camera _combatCamera;
        [SerializeField] private Vector3 _cameraLocalPosition = new Vector3(0f, 0f, -10f);
        [SerializeField] private float _orthographicSize = 5f;
        [SerializeField] private bool _lockCameraEveryFrame = true;

        public Camera CombatCamera => _combatCamera;

        private void Awake()
        {
            ApplyCameraSetup();
            SnapCameraToAnchor();
        }

        private void LateUpdate()
        {
            if (_lockCameraEveryFrame)
                SnapCameraToAnchor();
        }

        /// <summary>
        /// Applies settings for the combat view.
        /// </summary>
        public void ApplyCameraSetup()
        {
            if (_combatCamera == null)
                return;

            _combatCamera.orthographic = true;
            _combatCamera.orthographicSize = _orthographicSize;
            _combatCamera.nearClipPlane = 0.1f;
            _combatCamera.farClipPlane = 100f;
            _combatCamera.transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Forces the camera back to the fixed combat pose.
        /// </summary>
        public void SnapCameraToAnchor()
        {
            if (_combatCamera == null)
                return;

            var worldPos = transform.TransformPoint(_cameraLocalPosition);
            _combatCamera.transform.SetPositionAndRotation(worldPos, Quaternion.identity);
        }
    }
}
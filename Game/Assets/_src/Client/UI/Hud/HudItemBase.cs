using UnityEngine;

namespace Game.Client.UI.Hud
{
    public abstract class HudItemBase : MonoBehaviour, IHudItem
    {
        [Header("Placement")]
        [SerializeField] private Vector3 worldOffset;
        [SerializeField] private PlacementMode mode = PlacementMode.BillboardYaw;
        [SerializeField] private float yawOffsetDeg = 180f; // 0 або 180 — залежить від префаба

        [Tooltip("Для ground режимів: 90 = ідеально горизонтально, 80-85 = більш читабельно.")]
        [SerializeField] private float groundTiltDeg = 90f;
        
        private Transform _anchor;
        public bool IsVisible => gameObject.activeSelf;

        public void BindPlacement(Transform anchorTransform)
        {
            _anchor = anchorTransform;
        }

        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf == visible) return;
            gameObject.SetActive(visible);

            // важливо: коли знову показали — інколи треба “форс-рефреш” UI
            OnVisibilityChanged(visible);
        }

        public void UpdatePlacement(in HudPlacementContext ctx)
        {
            if (!IsVisible) return;
            if (_anchor == null || ctx.WorldCamera == null) return;

            transform.position = _anchor.position + worldOffset;

            switch (mode)
            {
                case PlacementMode.BillboardYaw:
                {
                    transform.rotation = Quaternion.Euler(0, yawOffsetDeg, 0f);
                    break;
                }
                case PlacementMode.GroundFlat:
                {
                    transform.rotation = Quaternion.Euler(groundTiltDeg, 0, 0f);
                    break;
                }
                case PlacementMode.GroundYawToCamera:
                {
                    var toCam = ctx.WorldCamera.transform.position - transform.position;
                    toCam.y = 0f;

                    var yaw = 0f;
                    if (toCam.sqrMagnitude > 1e-6f)
                        yaw = Quaternion.LookRotation(toCam.normalized, Vector3.up).eulerAngles.y;

                    yaw += yawOffsetDeg;
                    transform.rotation = Quaternion.Euler(groundTiltDeg, yaw, 0f);
                    break;
                }
            }
        }

        public virtual void Dispose()
        {
            // derived items unsubscribe here
        }

        /// <summary>
        /// Derived HUD може зробити "force refresh" коли HUD знову показали.
        /// </summary>
        protected virtual void OnVisibilityChanged(bool visible) { }

        public enum PlacementMode : byte
        {
            BillboardYaw,       // над будівлею
            GroundFlat,         // лежить на землі (X=90)
            GroundYawToCamera   // лежить + yaw до камери (читабельніше)
        }
    }
}

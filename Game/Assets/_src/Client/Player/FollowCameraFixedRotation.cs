using UnityEngine;

namespace Game.Client.Player
{
    /// <summary>
    /// Проста заміна Cinemachine
    /// </summary>
    public class FollowCameraFixedRotation : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -8f);
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private Vector3 eulerRotation = new Vector3(45f, 0f, 0f);

        private Vector3 _vel;

        private void Awake()
        {
            transform.rotation = Quaternion.Euler(eulerRotation);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime);

            // rotation фіксована
            transform.rotation = Quaternion.Euler(eulerRotation);
        }
    }
}
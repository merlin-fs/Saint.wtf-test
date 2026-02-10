using UnityEngine;

namespace Game.Client.Player
{
    /// <summary>
    /// Проста заміна Cinemachine
    /// </summary>
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -8f);
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private bool lookAtTarget = true;

        private Vector3 _vel;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime);

            if (lookAtTarget)
                transform.LookAt(target.position, Vector3.up);
        }
    }
}
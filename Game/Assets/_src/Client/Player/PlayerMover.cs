using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Client.Player
{
    public sealed class PlayerMover : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction; // Gameplay/Move
        [SerializeField] private Rigidbody rb;

        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private bool faceMoveDirection = true;
        [SerializeField] private float turnSpeedDeg = 720f;

        [SerializeField] private Transform cameraTransform; 

        private Vector2 _move;

        private void OnEnable()
        {
            var a = moveAction.action;
            a.Enable();
            a.performed += OnMove;
            a.canceled += OnMove;
        }

        private void OnDisable()
        {
            var a = moveAction.action;
            a.performed -= OnMove;
            a.canceled -= OnMove;
            a.Disable();
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _move = ctx.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            Vector3 dir;

            if (cameraTransform != null)
            {
                var f = cameraTransform.forward;
                f.y = 0;
                f.Normalize();
                var r = cameraTransform.right;
                r.y = 0;
                r.Normalize();
                dir = r * _move.x + f * _move.y;
            }
            else
            {
                dir = new Vector3(_move.x, 0f, _move.y);
            }

            if (dir.sqrMagnitude > 1f) dir.Normalize();

            rb.MovePosition(rb.position + dir * (moveSpeed * Time.fixedDeltaTime));

            if (!faceMoveDirection || !(dir.sqrMagnitude > 0.0001f)) return;
            
            var target = Quaternion.LookRotation(dir, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, target, turnSpeedDeg * Time.fixedDeltaTime));
        }
    }
}
using System.Collections;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    //Components
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private TrailRenderer _trailRenderer;

    //Input
    private float _inputX;
    private float _inputY;
    private bool _jumpInputDown;
    private bool _jumpInputReleased;
    private bool _dashInputDown;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.05f;
    [SerializeField] private LayerMask _collisionMask;

    [Header("Movement Settings")]
    [SerializeField] private bool _active = true;
    [SerializeField] private float _walkVelocity = 2f;
    [SerializeField] private float _jumpVelocity = 10f;

    [Header("Dashing")]
    [SerializeField] private float _dashingVelocity = 14f;
    [SerializeField] private float _dashingTime = 0.5f;
    private Vector2 _dashingDir;
    private bool _isDashing;
    private bool _canDash = true;


    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _trailRenderer = GetComponent<TrailRenderer>();
    }

    private void Update()
    {
        if (!_active) return;

        CaptureInput();
        HandleDashInit();
        ApplyDashing();
        if (_isDashing) return;

        ApplyDashReset();
        ApplyWalking();
        ApplyJump();
        ApplyRotation();
        ApplyAnimations();
    }

    private void CaptureInput()
    {
        _inputX = Input.GetAxisRaw("Horizontal");
        _inputY = Input.GetAxisRaw("Vertical");
        _jumpInputDown = Input.GetButtonDown("Jump");
        _jumpInputReleased = Input.GetButtonUp("Jump");
        _dashInputDown = Input.GetButtonDown("Dash");
    }

    private void HandleDashInit()
    {
        if (_dashInputDown && _canDash)
        {
            _isDashing = true;
            _canDash = false;
            _trailRenderer.emitting = true;
            _dashingDir = new Vector2(_inputX, _inputY);
            if (_dashingDir == Vector2.zero)
            {
                _dashingDir = new Vector2(transform.localScale.x, 0);
            }

            StartCoroutine(StopDashing());
        }
    }

    private void ApplyDashing()
    {
        if (_isDashing)
        {
            _rigidbody.velocity = _dashingDir.normalized * _dashingVelocity;
        }
    }

    private void ApplyDashReset()
    {
        if (IsGrounded())
        {
            _canDash = true;
        }
    }

    private IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(_dashingTime);
        _trailRenderer.emitting = false;
        _isDashing = false;
        _rigidbody.velocity = Vector2.zero;
    }


    private void ApplyWalking()
    {
        _rigidbody.velocity = new Vector2(_inputX * _walkVelocity, _rigidbody.velocity.y);
    }

    private void Jump()
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _jumpVelocity);
    }

    private void ApplyJump()
    {
        if (_jumpInputDown && IsGrounded())
        {
            Jump();
        }

        if (_jumpInputReleased && _rigidbody.velocity.y > 0)
        {
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0);
        }
    }

    private void ApplyRotation()
    {
        if (_inputX != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(_inputX), 1, 1);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            _groundCheck.position,
            _groundCheckRadius,
            _collisionMask);
    }

    private void ApplyAnimations()
    {
        _animator.SetBool("IsDashing", _isDashing);
        _animator.SetBool("IsWalking", _inputX != 0);
        _animator.SetBool("IsGrounded", IsGrounded());
        _animator.SetFloat("VerticalVelocity", _rigidbody.velocity.y);
    }
}

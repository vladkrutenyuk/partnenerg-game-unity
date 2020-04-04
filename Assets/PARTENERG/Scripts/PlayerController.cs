using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	private const float _speedWalk = 4f;
	private const float _speedRun = 8f;
	private const float _speedCrouch = 2f;
    private const float _speedJump = 5f;
    private const float _speedMoveInFlight = 2f;
    private const float _impulseCounterDivider = 10f;
	private const float _gravity = 10f;
	private const float _mouseSensitivity = 3f;
    private const float _minRotationX = -70f;
	private const float _maxRotationX = 80f;

    private Vector3 _motion;
    private Vector3 _savedMotion;
    private Vector3 _movementComponent;
    private Vector3 _impulseComponent;
    private Vector3 _jumpComponent;

    private float _gravityComponent = 0f;
    private float _currentRotationX;
    private bool _isGrounded = true;

	[SerializeField] private CharacterController _controller;
	[SerializeField] private Camera _camera;

	private void Update()
	{
        _isGrounded = IsGrounded();
        _motion = Vector3.zero;
		_movementComponent = GetMovementDirection() * GetMovementSpeed();

        if(_isGrounded)
        {
            _savedMotion = Vector3.zero;
            _gravityComponent = 0;
            _motion = _movementComponent;
            _savedMotion = _motion;

            if(Input.GetKey(KeyCode.Space))
            {
                StartCoroutine(Jump());
            }
        }
        else
        {
            _gravityComponent -= _gravity * Time.deltaTime;
            _movementComponent = Vector3.ClampMagnitude(_movementComponent, _speedMoveInFlight);
            _savedMotion = Vector3.ClampMagnitude(_savedMotion + _movementComponent, _savedMotion.magnitude);
            _motion = _savedMotion + new Vector3(0, _gravityComponent, 0);
        }

        _motion += _impulseComponent + _jumpComponent;

		_controller.Move(_motion * Time.deltaTime);

        Debug.DrawRay(transform.position + (Vector3.up * _controller.radius), _movementComponent, Color.white);
        Debug.DrawRay(transform.position + (Vector3.up * _controller.radius), _motion, Color.blue);
        Debug.DrawRay(transform.position + (Vector3.up * _controller.radius), new Vector3(0, _gravityComponent, 0), Color.green);
        Debug.DrawRay(transform.position + (Vector3.up * _controller.radius), _impulseComponent, Color.yellow);

		ApplyRotation();
	}

    private Vector3 GetMovementDirection()
    {
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        direction = transform.TransformDirection(direction);
        direction.y = ConsiderGroundSlope(direction);

        direction = Vector3.ClampMagnitude(direction, 1);

        return direction;
    }

    private float GetMovementSpeed()
    {
        float speed = _speedWalk;

        if(Input.GetKey(KeyCode.LeftShift))
        {
            speed = _speedRun;
        }
        if(Input.GetKey(KeyCode.LeftControl))
        {
            speed = _speedCrouch;
        }

        return speed;
    }

    private IEnumerator Jump()
    {
        _jumpComponent = new Vector3(0, _speedJump, 0);

        yield return new WaitForSeconds(0.1f);
        while(!_isGrounded)
        {
            if(Physics.Raycast(transform.position + Vector3.up * _controller.height, Vector3.up, 0.1f))
            {
                _jumpComponent = Vector3.zero;
                yield break;
            }
            yield return null;
        }

        _jumpComponent = Vector3.zero;

        yield break;
    }

    private void ApplyRotation()
    {
        Vector3 rotationY = new Vector3(0, Input.GetAxis("Mouse X"), 0) * _mouseSensitivity;
		_controller.transform.Rotate(rotationY);

		float rotationX = Input.GetAxis("Mouse Y") * _mouseSensitivity;
		_currentRotationX -= rotationX;
		_currentRotationX = Mathf.Clamp(_currentRotationX, _minRotationX, _maxRotationX);
		_camera.transform.localEulerAngles = new Vector3(_currentRotationX, 0, 0);
    }

    private bool IsGrounded()
    {
        RaycastHit hit;
		Vector3 origin = transform.position + (Vector3.up * _controller.radius);
		return Physics.SphereCast(origin, _controller.radius, Vector3.down, out hit, 0.1f);
    }

	private float ConsiderGroundSlope(Vector3 velocity)
	{
		RaycastHit hit;
		Vector3 center = transform.position + (Vector3.up * _controller.radius);
		if (Physics.SphereCast(center, _controller.radius, Vector3.down, out hit, 0.1f))
		{
			float angleA = Vector3.Angle(hit.normal, velocity); // Сам угл в градусах
			float angleB;
			Debug.DrawRay(hit.point, hit.normal, Color.red);
			
			if(angleA > 91f & angleA <= 135f) // Подъем по склону
			{
				angleB = angleA - 90f;
				velocity.y = Mathf.Tan(angleB * Mathf.PI / 180f) * velocity.magnitude;
			}
            else if (angleA > 30f & angleA < 89f) // Спуск по склону
			{	
				angleB = 90f - angleA;
				velocity.y = -Mathf.Tan(angleB * Mathf.PI / 180f) * velocity.magnitude * 1.1f;
			}
		}
        else
        {
            velocity.y = 0;
        }

        return velocity.y;
	}

    public void AddImpulse(Vector3 impulse)
    {
        _impulseComponent += impulse;

        StopCoroutine(WillStopImpulseOnGround());
        StartCoroutine(WillStopImpulseOnGround());
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) 
    {
        if(_isGrounded)
        {
            return;
        }

        float counterImpulseMult = Vector3.Dot(_impulseComponent.normalized, hit.normal.normalized);
        counterImpulseMult = 1 - Mathf.Abs(counterImpulseMult) / _impulseCounterDivider;

        _impulseComponent *= counterImpulseMult;
    }

    private IEnumerator WillStopImpulseOnGround()
    {
        yield return new WaitForSeconds(0.1f);

        while(!_isGrounded)
        {
            yield return null;
        }

        _impulseComponent = Vector3.zero;

        yield break;
    }
}

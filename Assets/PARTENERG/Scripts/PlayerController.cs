using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private const float _speedWalk = 4f;
	private const float _speedRun = 8f;
	private const float _speedCrouch = 2f;
    private const float _speedMoveInFlight = 3f;
	private const float _gravity = 10f;
	private const float _mouseSensitivity = 3f;
    private const float _minRotX = -70f;
	private const float _maxRotX = 80f;

	private float _currentRotationX;
	private bool _isJumping = false;
    private bool _isGrounded = true;
    private Vector3 _motion;
    private Vector3 _savedMotion;
    private Vector3 _movementComponent;
    private Vector3 _impulseComponent;
    private float _gravityComponent = 0f;

	[SerializeField] private CharacterController _controller;
	[SerializeField] private Camera _camera;

	private void Update()
	{
        _motion = Vector3.zero;

		_movementComponent = GetMovementDirection() * GetMovementSpeed();

        if(IsGrounded())
        {
            _savedMotion = Vector3.zero;
            _gravityComponent = 0;
            _motion = _movementComponent;
            _savedMotion = _motion;
        }
        else
        {
            _gravityComponent -= _gravity * Time.deltaTime;
            _movementComponent = Vector3.ClampMagnitude(_movementComponent, _speedMoveInFlight);
            _savedMotion = Vector3.ClampMagnitude(_savedMotion + _movementComponent, _savedMotion.magnitude);
            _motion = _savedMotion + new Vector3(0, _gravityComponent, 0);
        }

		_controller.Move(_motion * Time.deltaTime);

        Debug.DrawRay(transform.position + (Vector3.up * _controller.radius), _movementComponent, Color.white);
        Debug.DrawRay(transform.position + (Vector3.up * _controller.radius), _motion, Color.blue);
        Debug.DrawRay(transform.position + (Vector3.up * _controller.radius), new Vector3(0, _gravityComponent, 0), Color.green);

		ApplyRotation();
	}

    private Vector3 GetMovementDirection()
    {
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        direction = transform.TransformDirection(direction);
        direction.y = ConsiderGroundSlope(direction);
        return direction.normalized;
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

    private void ApplyRotation()
    {
        Vector3 rotationY = new Vector3(0, Input.GetAxis("Mouse X"), 0) * _mouseSensitivity;
		_controller.transform.Rotate(rotationY);

		float rotationX = Input.GetAxis("Mouse Y") * _mouseSensitivity;
		_currentRotationX -= rotationX;
		_currentRotationX = Mathf.Clamp(_currentRotationX, _minRotX, _maxRotX);
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
}

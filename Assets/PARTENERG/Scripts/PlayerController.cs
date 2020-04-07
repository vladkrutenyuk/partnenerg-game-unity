using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	private const float SpeedWalk = 4f;
	private const float SpeedRun = 6.5f;
	private const float SpeedCrouch = 2f;
    private const float SpeedJump = 5f;
    private const float VerticalInputFlightFactor = 0.1f;
    private const float HorizontalInputFlightFactor = 0.1f;
    private const float ImpulseCounterDivider = 10f;
	private const float Gravity = 10f;
    private const float CrouchedControllerHeight = 1.3f;
	private const float MouseSensitivity = 3f;
    private const float MinRotationX = -70f;
	private const float MaxRotationX = 80f;

    private Vector3 _motion;
    private Vector3 _savedMotion;
    private Vector3 _movementComponent;
    private Vector3 _impulseComponent;
    private Vector3 _jumpComponent;

    private RaycastHit groundHit;

    private float _currentRotationX;
    private float _controllerHeight;
    private float _controllerCenterY;

    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform mainCameraHolder;

    public float gravityComponent {get; private set;}
    public bool isGrounded {get; private set;}

    public enum MovementType {Walk, Run, Crouch};
    [HideInInspector] public MovementType movementType;
    public Camera mainCamera;

    private void Start() 
    {
        _controllerHeight = controller.height;
        _controllerCenterY = controller.center.y;
    }

	private void Update()
	{
        isGrounded = IsGrounded();

        SetMovementType();
        _motion = Vector3.zero;
		_movementComponent = GetMovementDirection() * GetMovementSpeed();

        if(isGrounded)
        {
            _savedMotion = Vector3.zero;
            gravityComponent = 0;
            _motion = _movementComponent;
            _savedMotion = _motion;

            if(Input.GetKey(KeyCode.Space))
            {
                StartCoroutine(Jump());
            }
        }
        else
        {
            gravityComponent -= Gravity * Time.deltaTime;
            _savedMotion = Vector3.ClampMagnitude(_savedMotion + _movementComponent, _savedMotion.magnitude);
            _motion = _savedMotion + new Vector3(0, gravityComponent, 0);
        }

        _motion += _impulseComponent + _jumpComponent;
		controller.Move(_motion * Time.deltaTime);

		ApplyRotation();

        DrawVectorDebugRays();
	}

    private void DrawVectorDebugRays()
    {
        Debug.DrawRay(groundHit.point, groundHit.normal, Color.red);
        Debug.DrawRay(transform.position + (Vector3.up * controller.radius), _movementComponent, Color.white);
        Debug.DrawRay(transform.position + (Vector3.up * controller.radius), _motion, Color.blue);
        Debug.DrawRay(transform.position + (Vector3.up * controller.radius), new Vector3(0, gravityComponent, 0), Color.green);
        Debug.DrawRay(transform.position + (Vector3.up * controller.radius), _impulseComponent, Color.yellow);
    }

    private void SetMovementType()
    {
        movementType = MovementType.Walk;

        if(Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0)
        {
            movementType = MovementType.Run;
        }

        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            SetCrouchMovementType(true);
        }

        if(Input.GetKey(KeyCode.LeftControl))
        {
            movementType = MovementType.Crouch;
        }

        if(Input.GetKeyUp(KeyCode.LeftControl))
        {
            SetCrouchMovementType(false);
        } 
    }

    private void SetCrouchMovementType(bool isCrouched)
    {
        float duration = 0.4f;
        float toHeight;
        float toCenter;

        if(isCrouched)
        {
            toHeight = CrouchedControllerHeight;
            toCenter = _controllerCenterY - (_controllerHeight - CrouchedControllerHeight) / 2f;
        }
        else
        {
            toHeight = _controllerHeight;
            toCenter = _controllerCenterY;
        }
        
        DOTween.To(() => controller.height, x => {
            controller.height = x;
        }, toHeight, duration);

        DOTween.To(() => controller.center.y, x => {
            controller.center = new Vector3 (0, x, 0);
        }, toCenter, duration);

        mainCameraHolder.DOLocalMoveY(toHeight - 0.1f, duration);
    }

    private Vector3 GetMovementDirection()
    {
        float verticalFactor = 1f;
        float horizontalFactor = 1f;
        if(!isGrounded)
        {
            verticalFactor = VerticalInputFlightFactor;
            horizontalFactor = HorizontalInputFlightFactor;
        }

        Vector3 direction = new Vector3(
            Input.GetAxis("Horizontal") * horizontalFactor, 0, Input.GetAxis("Vertical") * verticalFactor);

        direction = transform.TransformDirection(direction);
        direction.y = ConsiderGroundSlope(direction);

        direction = Vector3.ClampMagnitude(direction, 1);

        return direction;
    }

    private float GetMovementSpeed()
    {
        switch (movementType)
        {
            case MovementType.Walk:
                return SpeedWalk;

            case MovementType.Run:
                return SpeedRun;

            case MovementType.Crouch:
                return SpeedCrouch;

            default :
                return 0;
        }
    }

    private IEnumerator Jump()
    {
        _jumpComponent = new Vector3(0, SpeedJump, 0);

        yield return new WaitForSeconds(0.1f);
        while(!isGrounded)
        {
            if(Physics.Raycast(transform.position + Vector3.up * controller.height, Vector3.up, 0.1f))
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
        Vector3 rotationY = new Vector3(0, Input.GetAxis("Mouse X"), 0) * MouseSensitivity;
		controller.transform.Rotate(rotationY);

		float rotationX = Input.GetAxis("Mouse Y") * MouseSensitivity;
		_currentRotationX -= rotationX;
		_currentRotationX = Mathf.Clamp(_currentRotationX, MinRotationX, MaxRotationX);
		mainCamera.transform.localEulerAngles = new Vector3(_currentRotationX, 0, 0);
    }

    private bool IsGrounded()
    {
		Vector3 origin = transform.position + (Vector3.up * controller.radius);
		return Physics.SphereCast(origin, controller.radius, Vector3.down, out groundHit, 0.1f);
    }

	private float ConsiderGroundSlope(Vector3 velocity)
	{
		if (isGrounded)
		{
			float angleA = Vector3.Angle(groundHit.normal, velocity); // Сам угл в градусах
			float angleB;
			
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
        if(isGrounded)
        {
            return;
        }

        float counterImpulseMult = Vector3.Dot(_impulseComponent.normalized, hit.normal.normalized);
        counterImpulseMult = 1 - Mathf.Abs(counterImpulseMult) / ImpulseCounterDivider;

        _impulseComponent *= counterImpulseMult;
    }

    private IEnumerator WillStopImpulseOnGround()
    {
        yield return new WaitForSeconds(0.1f);

        while(!isGrounded)
        {
            yield return null;
        }

        _impulseComponent = Vector3.zero;

        yield break;
    }
}

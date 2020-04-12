using UnityEngine;

public class FirstPersonSpaceAnimation : MonoBehaviour
{
    // TO-DO Switch weapon animation

    private const float RotationRemapInput = 5f;
    private const float RotationLerpTimeMult = 3f;
    private const string RotationXAnimatorName = "Space_RotationX";
    private const string RotationYAnimatorName = "Space_RotationY";

    private Vector2 _animatorRotation;

    [SerializeField] private Animator animator;

    private void Update() 
    {
        _animatorRotation.x = Mathf.Lerp(
            _animatorRotation.x, GetAnimatorRotation(Input.GetAxis("Mouse X")), Time.deltaTime * RotationLerpTimeMult);

        _animatorRotation.y = Mathf.Lerp(
            _animatorRotation.y, GetAnimatorRotation(Input.GetAxis("Mouse Y")), Time.deltaTime * RotationLerpTimeMult);

        animator.SetFloat(RotationXAnimatorName, _animatorRotation.x);
        animator.SetFloat(RotationYAnimatorName, _animatorRotation.y);
    }

    private float GetAnimatorRotation(float input)
    {
        return Remap(input, -RotationRemapInput, RotationRemapInput, -1, 1);
    }
    
    private float Remap(float value, float from1, float to1, float from2, float to2) 
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}

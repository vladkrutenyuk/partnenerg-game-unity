using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FirstPersonSpaceAnimation : MonoBehaviour
{
    // Holder rotation when mouse rotation

    // Switch weapon animation

    [SerializeField] private Animator animator;

    private float _mouseX;
    private float _mouseY;

    private float _rotationX;
    private float _rotationY;

    private void Update() 
    {
        _mouseX = Input.GetAxis("Mouse X");
        _mouseY = Input.GetAxis("Mouse Y");

        _mouseX = Map(_mouseX, -5, 5, -1, 1);
        _mouseY = Map(_mouseY, -5, 5, -1, 1);

        _rotationX = Mathf.Lerp(_rotationX, _mouseX, Time.deltaTime * 3f);
		_rotationY = Mathf.Lerp(_rotationY, _mouseY, Time.deltaTime * 3f);

        animator.SetFloat("Space_RotationX", _rotationX);
        animator.SetFloat("Space_RotationY", _rotationY);
    }
    
    private float Map (float value, float from1, float to1, float from2, float to2) 
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}

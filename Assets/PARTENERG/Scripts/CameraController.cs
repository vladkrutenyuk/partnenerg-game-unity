using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    
    private float _gravityFactor;
    private bool _wasGrounded;
    
    private void Update()
    {
        if(!_wasGrounded && playerController.isGrounded && _gravityFactor > 5)
        {
            _gravityFactor *= _gravityFactor;
            CallCameraShakes(0.1f, new Vector3(0, 0.003f, 0) * _gravityFactor, 10, 0);
        }

        _wasGrounded = playerController.isGrounded;
        _gravityFactor = -playerController.gravityComponent;
    }

    private void CallCameraShakes(float duration, Vector3 strength, int vibrato, float randomness)
    {
        playerController.mainCamera.DOShakePosition(duration, strength, vibrato, randomness, true);
        print(strength.y);
    }


}

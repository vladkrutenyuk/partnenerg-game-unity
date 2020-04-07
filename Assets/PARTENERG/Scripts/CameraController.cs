using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    private const float FallShakeDuration = 0.1f;
    private const float FallShakeStrength = 0.35f;
    private const int FallShakeVibrato = 10;

    [SerializeField] private PlayerController playerController;

    private float _gravityFactor;
    private bool _wasGrounded;
    
    private void Update()
    {
        if(!_wasGrounded && playerController.isGrounded && _gravityFactor > 5)
        {
            _gravityFactor *= _gravityFactor;
            Vector3 strength = new Vector3(0, FallShakeStrength, 0) * _gravityFactor / 100f;
            CallCameraShakes(FallShakeDuration, strength, FallShakeVibrato, 1);
        }

        _wasGrounded = playerController.isGrounded;
        _gravityFactor = -playerController.gravityComponent;
    }

    public void CallCameraShakes(float duration, Vector3 strength, int vibrato, float randomness)
    {
        playerController.mainCamera.DOShakePosition(duration, strength, vibrato, randomness, true);
        print(strength.y);
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WeaponHandsController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool _isOneHanded = false;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            _isOneHanded = !_isOneHanded;
            SwitchHandMode(_isOneHanded);
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("Reload");
        }
    }

    private void SwitchHandMode(bool isOneHanded)
    {
        DOTween.To(
            () => animator.GetLayerWeight(1), 
            x => {
                animator.SetLayerWeight(1, x);
            }, 
            isOneHanded ? 1 : 0, 
            0.5f);
    }
}

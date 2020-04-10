using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandsController : MonoBehaviour
{
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private List<Weapon> weapons;

    private bool _isOneHanded = false;
    private int _currentWeaponIndex;

    private AnimatorOverrideController animatorOverrideController;
    //private AnimationClipOverrides clipOverrides;

    private void Start() 
    {
        animatorOverrideController = new AnimatorOverrideController(weaponAnimator.runtimeAnimatorController);
        weaponAnimator.runtimeAnimatorController = animatorOverrideController;

        for (int i = 0; i < weapons.Count; i++)
        {
            if(weapons[i].gameObject.activeSelf)
            {
                _currentWeaponIndex = i;
                Debug.Log(weapons[_currentWeaponIndex].name);
            }
        }    
    }
    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            _isOneHanded = !_isOneHanded;
            SwitchHandMode(_isOneHanded);
        }

        if(Input.mouseScrollDelta.y != 0)
        {
            SwitchWeapon((int)Input.mouseScrollDelta.y);
        }
    }
    private void SwitchWeapon(int mouseScroll)
    {
        mouseScroll = Mathf.Clamp(mouseScroll, -1, 1);

        weapons[_currentWeaponIndex].gameObject.SetActive(false);

        _currentWeaponIndex -= mouseScroll;
        _currentWeaponIndex = Mathf.Clamp(_currentWeaponIndex, 0, weapons.Count - 1);

        weapons[_currentWeaponIndex].gameObject.SetActive(true);
        animatorOverrideController["Rifle_Idle"] = weapons[_currentWeaponIndex].idleAnimation;
        // animatorOverrideController.ApplyOverrides()
    }

    private void SwitchHandMode(bool isOneHanded)
    {
        DOTween.To(
            () => weaponAnimator.GetLayerWeight(1), 
            x => {
                weaponAnimator.SetLayerWeight(1, x);
            }, 
            isOneHanded ? 1 : 0, 
            0.5f);
    }
}

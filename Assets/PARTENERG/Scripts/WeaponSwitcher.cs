using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WeaponSwitcher : MonoBehaviour
{
    private const string AnimatorSwitchFloatName = "Space_Switch";
    private int _currentWeaponIndex;
    private bool _isSwitching;

    private AnimatorOverrideController animatorOverrideController;
    private AnimationClipOverrides clipOverrides;

    [SerializeField] private Animator animator;
    [SerializeField] private List<Weapon> weapons;

    private void Start() 
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if(weapons[i].gameObject.activeSelf)
            {
                _currentWeaponIndex = i;
            }
        }

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        SetCurrentWeaponAnimations();
    }

    private void Update() 
    {
        if(Input.mouseScrollDelta.y != 0)
        {
            ApplyMouseScroll((int)Input.mouseScrollDelta.y);
        }
    }

    private void ApplyMouseScroll(int mouseScroll)
    {
        if(_isSwitching)
        {
            return;
        }   

        int newWeaponIndex = Mathf.Clamp(_currentWeaponIndex - mouseScroll, 0, weapons.Count - 1);
        
        if(newWeaponIndex != _currentWeaponIndex)
        {
            StopCoroutine(SwitchWeapon(newWeaponIndex));
            StartCoroutine(SwitchWeapon(newWeaponIndex));
        }
    }

    private IEnumerator SwitchWeapon(int newIndex)
    {
        _isSwitching = true;      

        DOTween.To(() => animator.GetFloat(AnimatorSwitchFloatName), x => {
            animator.SetFloat(AnimatorSwitchFloatName, x);
        }, 1, weapons[_currentWeaponIndex].weaponSwitchTime);

        while(animator.GetFloat(AnimatorSwitchFloatName) < 1)
        {
            yield return null;
        }

        weapons[_currentWeaponIndex].gameObject.SetActive(false);
        _currentWeaponIndex = newIndex;
        weapons[_currentWeaponIndex].gameObject.SetActive(true);  

        SetCurrentWeaponAnimations();
        _isSwitching = false;  

        DOTween.To(() => animator.GetFloat(AnimatorSwitchFloatName), x => {
            animator.SetFloat(AnimatorSwitchFloatName, x);
        }, 0, weapons[_currentWeaponIndex].weaponSwitchTime);

        while(animator.GetFloat(AnimatorSwitchFloatName) > 0)
        {
            yield return null;
        }
        
        yield break;
    }

    private void SetCurrentWeaponAnimations()
    {
        clipOverrides["Idle"] = weapons[_currentWeaponIndex].idleAnimation;
        clipOverrides["Reload"] = weapons[_currentWeaponIndex].reloadAnimation;
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }
}

public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) {}

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    private int _currentWeaponIndex;

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
                Debug.Log(weapons[_currentWeaponIndex].weaponName);
            }
        }

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        SetCurrentWeapon(0);
    }

    private void Update() 
    {
        if(Input.mouseScrollDelta.y != 0)
        {
            SetCurrentWeapon((int)Input.mouseScrollDelta.y);
        }
    }

    private void SetCurrentWeapon(int mouseScroll)
    {
        mouseScroll = Mathf.Clamp(mouseScroll, -1, 1);

        // Change current weapon
        weapons[_currentWeaponIndex].gameObject.SetActive(false);

        _currentWeaponIndex -= mouseScroll;
        _currentWeaponIndex = Mathf.Clamp(_currentWeaponIndex, 0, weapons.Count - 1);

        weapons[_currentWeaponIndex].gameObject.SetActive(true);

        // Override animations for new current weapon
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

using UnityEngine;

public class Weapon : MonoBehaviour
{
    public string weaponName;
    [Range(0f, 1f)] public float weaponSwitchTime;
    public AnimationClip idleAnimation;
    public AnimationClip reloadAnimation;

}

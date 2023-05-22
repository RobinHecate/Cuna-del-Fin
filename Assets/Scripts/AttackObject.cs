using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Normal Attack")]
public class AttackObject : ScriptableObject
{
    public AnimatorOverrideController animOV;
    public float damage;
}

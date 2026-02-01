using UnityEngine;

[DisallowMultipleComponent]
public class CombatSkill : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _animator.SetTrigger(ConstantHelper.Combat.ANIMATOR_PARAM_ATTACK);
        }
    }
}

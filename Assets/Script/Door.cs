using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    Animator animator;
    [SerializeField] float openSpeen = 1f, closeSpeed = 1f;
    [SerializeField] bool _isOpen = false, _isLocked;
    public bool isOpen { get => _isOpen; set => SetState(value); }
    public bool isLocked { get => _isLocked; set => SetLockState(value); }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        SetState(speed: 100);
    }

    void SetState(bool? state = null, float? speed = null)
    {
        if (state != null) _isOpen = (bool)state;
        if (speed != null) animator.speed = (float)speed;
        else animator.speed = _isOpen ? openSpeen : closeSpeed;
        animator.SetBool("isOpen", _isOpen);
    }
    void SetLockState(bool state)
    {
        _isLocked = state;
        if (!_isLocked) Close();
    }

    public void Open()
    {
        if (_isLocked) return;
        animator.speed = openSpeen;
        SetState(true);
    }
    public void Close()
    {
        animator.speed = closeSpeed;
        SetState(false);
    }
}

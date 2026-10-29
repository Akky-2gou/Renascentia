using System.Collections.Generic;
using UnityEngine;

/*
 * モーションテスト用
 */

public class MotionTest : MonoBehaviour
{
    [SerializeField] private List<Animator> _animator;
    [SerializeField] private string _trigger = "";
    [SerializeField] private string _bool = "";
    private bool _isAnimating = false;
    
    void Start()
    {
        _isAnimating = false;
        foreach (Animator anim in _animator) anim.Rebind();
    }

    public void OnClick()
    {
        if (_bool != "") 
        {
            if (_isAnimating)
            {
                foreach (Animator anim in _animator) anim.SetBool(_bool, false);
                _isAnimating = false;
            }
            else
            {
                foreach (Animator anim in _animator) anim.SetBool(_bool, true);
                _isAnimating = true;
            }
        }

        if (_trigger != "") 
        {
            foreach (Animator anim in _animator) anim.SetTrigger(_trigger);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpAnimationTimeEvent : MonoBehaviour {
    [SerializeField] Animator animator;
    [SerializeField] string stateName;
    [SerializeField] int layer;
    [SerializeField] float normalizedTime;

    public void JumpAnimationTime() {
        Destroy(gameObject);
        animator.Play(stateName, layer, normalizedTime);
    }
}

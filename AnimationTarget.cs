using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTarget : MonoBehaviour
{
    public event System.Action<string> OnAnimationEvent;

    public void AnimationEvent(string animEvent)
    {
        OnAnimationEvent?.Invoke(animEvent);
    }
}

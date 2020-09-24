using System.Collections;
using System.Collections.Generic;
using BNJMO;
using UnityEngine;

public class BButtonAnimation : BBehaviour
{
    [SerializeField] private AnimationLerpFloat animLerp_HoverEnter;
    [SerializeField] private AnimationLerpFloat animLerp_HoverExit;

    private float startScale;

    protected override void Awake()
    {
        base.Awake();

        startScale = transform.localScale.x;
    }

    protected override void InitializeEventsCallbacks()
    {
        base.InitializeEventsCallbacks();

        BButton bButton = GetComponent<BButton>();
        if (IS_NOT_NULL(bButton))
        {
            bButton.ButtonHoveredEnter += On_BButton_ButtonHoveredEnter;
            bButton.ButtonHoveredExit += On_BButton_ButtonHoveredExit;
        }

        if (IS_NOT_NULL(animLerp_HoverEnter))
        {
            animLerp_HoverEnter.AnimationUpdated += On_AnimLerp_HoverEnter_AnimationProgressed;
        }

        if (IS_NOT_NULL(animLerp_HoverExit))
        {
            animLerp_HoverExit.AnimationUpdated += On_AnimLerp_HoverExit_AnimationProgressed;
        }
    }

    private void On_AnimLerp_HoverExit_AnimationProgressed(AnimationLerp<float> arg1, float value)
    {
        transform.localScale = Vector3.one * value;
    }

    private void On_AnimLerp_HoverEnter_AnimationProgressed(AnimationLerp<float> arg1, float value)
    {
        transform.localScale = Vector3.one * value;
    }

    private void On_BButton_ButtonHoveredEnter(BButton obj)
    {
        animLerp_HoverExit.StopAnimation();

        animLerp_HoverEnter.StartValue = transform.localScale.x;
        animLerp_HoverEnter.StartAnimation();
    }

    private void On_BButton_ButtonHoveredExit(BButton obj)
    {
        animLerp_HoverEnter.StopAnimation();

        animLerp_HoverExit.StartValue = transform.localScale.x;
        animLerp_HoverExit.EndValue = startScale;
        animLerp_HoverExit.StartAnimation();
    }
}

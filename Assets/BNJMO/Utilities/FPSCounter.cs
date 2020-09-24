using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNJMO;

public class FPSCounter : BBehaviour
{

    private float timer;
    private float refresh;
    private float avgFramerate;
    string display = "{0} FPS";
    private BText myText;

    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        myText = GetComponent<BText>();
    }


    protected override void Update()
    {
        base.Update();

        //Change smoothDeltaTime to deltaTime or fixedDeltaTime to see the difference
        float timelapse = Time.smoothDeltaTime;
        timer = timer <= 0 ? refresh : timer -= timelapse;

        if (timer <= 0) avgFramerate = (int)(1f / timelapse);
        myText.SetText(string.Format(display, avgFramerate.ToString()));
    }
}
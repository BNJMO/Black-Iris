using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNJMO;
using Sirenix.OdinInspector;

public class TrackerStatusTest : BBehaviour
{
    [SerializeField] [ReadOnly] private BImage image;
    private AbstractBTracker tracker;
    

    protected override void OnValidate()
    {
        base.OnValidate();

        image = GetComponentInChildren<BImage>();
    }

    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        if (IS_NOT_NULL(image))
        {
            image.SetColor(Color.red);
        }
    }

    protected override void InitializeEventsCallbacks()
    {
        base.InitializeEventsCallbacks();

        //BEventsCollection.AR_TrackerFound += On_AR_TrackerFound;
    }

    protected override void Update()
    {
        base.Update();

        if ((tracker)
            && (Camera.main))
        {
            BEventsCollection.TEST_TrackerMoved.Invoke(new BEHandle<Vector3>(Camera.main.transform.position - tracker.transform.position), BEventReplicationType.TO_ALL, false);
        }
    }

    private void On_AR_TrackerFound(BEHandle<string, Vector3, Quaternion> handle)
    {
        //if (IS_NOT_NULL(newTracker))
        //{
        //    if (newTracker.TrackerName == "TrackerImage_Test")
        //    {
        //        tracker = newTracker;
        //        image.SetColor(Color.green);
        //    }
        //}
    }
}

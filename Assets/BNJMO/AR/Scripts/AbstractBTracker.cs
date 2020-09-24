using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using easyar;

namespace BNJMO
{
    public abstract class AbstractBTracker : BBehaviour
    {
        public string TrackerName = "Tracker";
        public BTrackerType TrackerType;

        protected override void InitializeEventsCallbacks()
        {
            base.InitializeEventsCallbacks();

            switch (MotherOfManagers.Instance.ARTrackingMode)
            {
                case ARTrackingMode.VUFORIA:
#if VUFORIA
                    DefaultTrackableEventHandler defaultTrackableEventHandler = GetComponent<DefaultTrackableEventHandler>();
                    if (IS_NOT_NULL(defaultTrackableEventHandler))
                    {
                        defaultTrackableEventHandler.OnTargetFound.AddListener(OnTrackerFound);
                        defaultTrackableEventHandler.OnTargetLost.AddListener(OnTrackerLost);
                    }
#endif
                    break;

                case ARTrackingMode.EASY_AR:
                    //TargetController targetController = GetComponent<TargetController>();
                    //if (IS_NOT_NULL(targetController))
                    //{
                    //    targetController.TargetFound += OnTrackerFound;
                    //    targetController.TargetLost += OnTrackerLost;
                    //}
                    break;
            }
        }

        public void OnTrackerFound()
        {
            BEventsCollection.AR_TrackerFound.Invoke(new BEHandle<string, Vector3, Quaternion>(TrackerName, transform.position, transform.rotation), BEventReplicationType.TO_ALL);
        }

        public void OnTrackerLost()
        {
            BEventsCollection.AR_TrackerFound.Invoke(new BEHandle<string, Vector3, Quaternion>(TrackerName, transform.position, transform.rotation), BEventReplicationType.TO_ALL);
        }
    }
}
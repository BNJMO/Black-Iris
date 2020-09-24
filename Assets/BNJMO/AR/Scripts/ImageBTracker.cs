using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class ImageBTracker : AbstractBTracker 
    {
        protected override void OnValidate()
        {
            base.OnValidate();

            if (CanValidate() == false)
            {
                return;
            }

            if (TrackerType == BTrackerType.NONE)
            {
                TrackerType = BTrackerType.IMAGE;
            }
        }
    }
}
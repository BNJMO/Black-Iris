using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BNJMO
{
    public class BContainer : BUIElement
    {
        protected override void OnValidate()
        {
            objectNamePrefix = "C_";

            base.OnValidate();

            if (CanValidate() == false)
            {
                return;
            }
        }

    }
}

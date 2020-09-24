using System;
using UnityEngine;

[Serializable]
public class BAnchorInformation
{
    public BAnchorInformation()
    {
        BAnchorID = "None";
        TransformedPosition = Vector3.zero;
        TransformedRotation = Quaternion.identity;
    }

    public BAnchorInformation(string bAnchorID, Vector3 transformedPosition, Quaternion transformedRotation)
    {
        BAnchorID = bAnchorID;
        TransformedPosition = transformedPosition;
        TransformedRotation = transformedRotation;
    }

    public string BAnchorID { get; set; }
    public Vector3 TransformedPosition { get; set; }
    public Quaternion TransformedRotation { get; set; }
}

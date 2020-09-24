using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class RectanglePlayArea : AbstractPlayArea
    {
        #region Public Events

        #endregion

        #region Public Methods
        public Vector3 UpVector { get; private set; } = Vector3.up;

        public float Width { get; private set; }
        public float Depth { get; private set; }

        public override bool IsInsidePlayArea(Vector3 position)
        {
            // TODO : Implement according to thesis
            throw new System.NotImplementedException();
        }

        public override void SetUpPlayArea()
        {
            if (IS_NOT_NULL(ARCursor.Instance))
            {
                // Reset (existing) BAnchors
                setAnchorsCount = 0;
                PlayAreaBAnchor[] playAreaBAnchorsCopy = PlayAreaBAnchors.ToArray();
                foreach (PlayAreaBAnchor playAreaBAnchor in playAreaBAnchorsCopy)
                {
                    Destroy(playAreaBAnchor.gameObject);
                }
                PlayAreaBAnchors.Clear();
                ExtentPoints.Clear();
                CurrentPlayAreaBAnchorBeingPlaced = null;
                DrawPlayAreaExtensions();

                LogConsole("Started setting up play area. Setting first anchor...");
                PlayAreaState = EPlayAreaState.SETTING_ANCHORS;
                StartSettingFirstAnchor();
            }
        }

        public override void SetUpPlayArea(BAnchorInformation[] bAnchorInformation)
        {
            // TODO: Wait for some frames ?


            if (ARE_EQUAL(bAnchorInformation.Length, 2))
            {
                PlayAreaState = EPlayAreaState.SETTING_ANCHORS;

                // Spawn BAnchors
                for (int i = 0; i < 2; i++)
                {
                    PlayAreaBAnchor playAreaBAnchor = AddNewChildBAnchor(false);
                    if (IS_NOT_NULL(playAreaBAnchor))
                    {
                        playAreaBAnchor.gameObject.name += "_" + (i + 1);
                        playAreaBAnchor.SetTransformedPosition(bAnchorInformation[i].TransformedPosition);
                        playAreaBAnchor.SetTransformedRotation(bAnchorInformation[i].TransformedRotation);
                    }
                }

                // Finalize 
                setAnchorsCount = 2;
                SetExtentPoints();
                DrawPlayAreaExtensions();
                FinalizePlayAreaSetup();
            }
        }
        #endregion

        #region Serialized Fields


        #endregion

        #region Private Variables
        private int setAnchorsCount = 0;


        #endregion

        #region Life Cycle
        protected override void OnValidate()
        {
            base.OnValidate();

            if (CanValidate() == false)
            {
                return;
            }
        }

        #endregion

        #region Events Callbacks
        protected override void On_SpawnedPlayAreaBAnchor_StartedPlacing(PlayAreaBAnchor playAreaBAnchor)
        {
            base.On_SpawnedPlayAreaBAnchor_StartedPlacing(playAreaBAnchor);

        }

        protected override void On_SpawnedPlayAreaBAnchor_EndedPlacing(PlayAreaBAnchor playAreaBAnchor)
        {
            base.On_SpawnedPlayAreaBAnchor_EndedPlacing(playAreaBAnchor);

            switch (setAnchorsCount)
            {
                case 0:
                    setAnchorsCount = 1;
                    LogConsole("First anchor set. Setting second anchor...");
                    StartSettingSecondAnchor();
                    break;

                case 1:
                    setAnchorsCount = 2;
                    LogConsole("Second anchor set. Finalizing setup.");
                    FinalizePlayAreaSetup();
                    break;

                default:
                    LogConsoleWarning("Play Area already set up !");
                    break;
            }
        }


        #endregion

        #region Others


        private void StartSettingFirstAnchor()
        {
            PlayAreaBAnchor playAreaBAnchor = AddNewChildBAnchor();
            if (IS_NOT_NULL(playAreaBAnchor))
            {
                playAreaBAnchor.gameObject.name += "_1";
            }
        }

        private void StartSettingSecondAnchor()
        {
            PlayAreaBAnchor playAreaBAnchor = AddNewChildBAnchor();
            if (IS_NOT_NULL(playAreaBAnchor)
                && PlayAreaBAnchors.Count > 0
                && IS_NOT_NULL(PlayAreaBAnchors[0]))
            {
                playAreaBAnchor.gameObject.name += "_2";
                playAreaBAnchor.DisablePlacementRotation();
                playAreaBAnchor.transform.rotation = PlayAreaBAnchors[0].transform.rotation;
            }
        }

        protected override void SetExtentPoints()
        {
            if (setAnchorsCount > 0
                && PlayAreaState == EPlayAreaState.SETTING_ANCHORS
                && ARE_EQUAL(PlayAreaBAnchors.Count, 2))
            {
                //Vector3 firstAnchorPosition = BUtils.Get3DPlanearVector(playAreaBAnchors[0].GetPosition());
                Vector3 firstAnchorPosition = PlayAreaBAnchors[0].GetPosition();
                Vector3 firstAnchorForward = PlayAreaBAnchors[0].transform.forward;
                Vector3 firstAnchorRight = PlayAreaBAnchors[0].transform.right;

                //Vector3 SecondAnchorPosition = BUtils.Get3DPlanearVector(playAreaBAnchors[1].GetPosition());
                Vector3 SecondAnchorPosition = PlayAreaBAnchors[1].GetPosition();
                SecondAnchorPosition = new Vector3(SecondAnchorPosition.x, firstAnchorPosition.y, SecondAnchorPosition.z);
                Vector3 SecondAnchorForward = PlayAreaBAnchors[1].transform.forward;
                Vector3 SecondAnchorRight = PlayAreaBAnchors[1].transform.right;

                Vector3 firstIntersectionPoint;
                Math3D.LineLineIntersection(out firstIntersectionPoint, firstAnchorPosition, firstAnchorForward, SecondAnchorPosition, SecondAnchorRight);
                //if (Math3D.LineLineIntersection(out firstIntersectionPoint, firstAnchorPosition, firstAnchorForward, SecondAnchorPosition, SecondAnchorRight) == false)
                //{
                //    Vector3 closestPoint1;
                //    Vector3 closestPoint2;
                //    Math3D.ClosestPointsOnTwoLines(out closestPoint1, )
                //}

                Vector3 secondIntersectionPoint;
                Math3D.LineLineIntersection(out secondIntersectionPoint, firstAnchorPosition, firstAnchorRight, SecondAnchorPosition, SecondAnchorForward);

                ExtentPoints.Clear();
                ExtentPoints.Add(firstAnchorPosition);
                ExtentPoints.Add(firstIntersectionPoint);
                ExtentPoints.Add(SecondAnchorPosition);
                ExtentPoints.Add(secondIntersectionPoint);
            }
        }

        protected override void FinalizePlayAreaSetup()
        {
            if (ARE_EQUAL(ExtentPoints.Count, 4))
            {
                // Set UpVector
                UpVector = PlayAreaBAnchors[0].transform.up;

                // Set Width and Depth
                Width = (ExtentPoints[0] - ExtentPoints[3]).magnitude;
                Depth = (ExtentPoints[0] - ExtentPoints[1]).magnitude;
                if (Depth < Width)
                {
                    LogNotification("The set up play area has a width smaller than its depth.");
                }

                LogConsoleRed("Depth : " + Depth + " - Width : " + Width);

                base.FinalizePlayAreaSetup();
            }
        }
        #endregion
    }
}

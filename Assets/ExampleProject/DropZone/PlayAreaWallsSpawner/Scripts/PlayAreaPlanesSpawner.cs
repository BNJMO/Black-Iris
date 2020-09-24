using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class PlayAreaPlanesSpawner : BBehaviour
    {
        #region Public Events


        #endregion

        #region Public Methods


        #endregion

        #region Inspector Variables
        [SerializeField]
        private float wallsHeight = 2.5f;

        [SerializeField]
        private float playAreaSeperatorThickness = 0.3f;

        [SerializeField]
        private float minDropZoneWidth = 1.0f;

        [SerializeField]
        private float minDropZoneDepth = 1.0f;

        #endregion

        #region Private Variables
        private PlayAreaPlane playAreaPlanePrefab;
        private DropZonePlane dropZonePrefab;
        private PlayAreaSeparation playAreaSeparationPrefab;
        private RectanglePlayArea rectanglePlayArea;

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            playAreaPlanePrefab = Resources.Load<PlayAreaPlane>(BConsts.PATH_AR_PlayAreaPlane);
            dropZonePrefab = Resources.Load<DropZonePlane>(BConsts.PATH_DZ_DropZonePlane);
            playAreaSeparationPrefab = Resources.Load<PlayAreaSeparation>(BConsts.PATH_DZ_PlayAreaSeparation);

            BEventsCollection.AR_PlayAreaStateUpdated += On_AR_PlayAreaStateUpdated;
        }

        #endregion

        #region Events Callbacks
        private void On_AR_PlayAreaStateUpdated(BEHandle<EPlayAreaState, AbstractPlayArea> handle)
        {
            rectanglePlayArea = (RectanglePlayArea)handle.Arg2;
            if (handle.Arg1 == EPlayAreaState.READY
                && IS_NOT_NULL(rectanglePlayArea)
                && ARE_EQUAL(rectanglePlayArea.ExtentPoints.Count, 4)
                && ARE_NOT_EQUAL(rectanglePlayArea.PlayAreaBAnchors.Count, 0))
            {
                // Check given width an depth
                if (rectanglePlayArea.Width < minDropZoneWidth * 2.0f)
                {
                    LogConsoleError("The width of the created play area (" + rectanglePlayArea.Width + ") is less than the required minimum : " + minDropZoneWidth * 2.0f);
                    LogNotification("The width of the created play area (" + rectanglePlayArea.Width + ") is less than the required minimum : " + minDropZoneWidth * 2.0f);
                    return;
                }
                if (rectanglePlayArea.Depth < minDropZoneDepth * 2.0f)
                {
                    LogConsoleError("The depth of the created play area (" + rectanglePlayArea.Depth + ") is less than the required minimum : " + minDropZoneDepth * 2.0f);
                    LogNotification("The depth of the created play area (" + rectanglePlayArea.Depth + ") is less than the required minimum : " + minDropZoneDepth * 2.0f);
                    return;
                }

                Vector3[] extentPoints = rectanglePlayArea.ExtentPoints.ToArray();
                Transform parentTransform = rectanglePlayArea.transform;
                Vector3 floorUpwards = rectanglePlayArea.UpVector;

                Vector3 corner0 = extentPoints[0];
                Vector3 corner0_up = extentPoints[0] + floorUpwards * wallsHeight;
                Vector3 corner1 = extentPoints[1];
                Vector3 corner1_up = extentPoints[1] + floorUpwards * wallsHeight;
                Vector3 corner2 = extentPoints[2];
                Vector3 corner2_up = extentPoints[2] + floorUpwards * wallsHeight;
                Vector3 corner3 = extentPoints[3];
                Vector3 corner3_up = extentPoints[3] + floorUpwards * wallsHeight;
                Vector3 middle0_1 = (corner0 + corner1) / 2.0f;
                Vector3 middle2_3 = (corner2 + corner3) / 2.0f;

                // Floor 
                CreatePlaneMesh(corner0, corner1, corner2, corner3, EPlayAreaPlaneType.FLOOR);

                // Ceiling
                CreatePlaneMesh(corner0_up, corner1_up, corner2_up, corner3_up, EPlayAreaPlaneType.CEILING);

                // Wall 0 - 1
                CreatePlaneMesh(corner0, corner0_up, corner1_up, corner1, EPlayAreaPlaneType.WALL);
                
                // Wall 1 - 2
                CreatePlaneMesh(corner1, corner1_up, corner2_up, corner2, EPlayAreaPlaneType.WALL);

                // Wall 2 - 3
                CreatePlaneMesh(corner2, corner2_up, corner3_up, corner3, EPlayAreaPlaneType.WALL);
                
                // Wall 3 - 0
                CreatePlaneMesh(corner3, corner3_up, corner0_up, corner0, EPlayAreaPlaneType.WALL);

                // Play Area Seperation
                CreatePlayAreaSeparation(middle0_1, middle2_3, (corner1 - corner0).normalized, floorUpwards);

                // Drop Zones
                float dZWidthFraction = rectanglePlayArea.Width / minDropZoneWidth;
                int numberDZWidth = Mathf.FloorToInt(dZWidthFraction);
                float dropZoneWidth = minDropZoneWidth + (rectanglePlayArea.Width - (numberDZWidth * minDropZoneWidth)) / numberDZWidth;

                float dZDepthHalfFraction = (rectanglePlayArea.Depth / 2.0f) / minDropZoneDepth;
                int halfNumberDZDepth = Mathf.FloorToInt(dZDepthHalfFraction);
                int numberDZDepth = halfNumberDZDepth * 2;
                float dropZoneDepth = minDropZoneDepth + (rectanglePlayArea.Depth - (numberDZDepth * minDropZoneDepth)) / numberDZDepth;

                Vector3 widthShift = (corner3 - corner0).normalized * dropZoneWidth;
                Vector3 depthShift = (corner1 - corner0).normalized * dropZoneDepth;
                Vector3 cornerShift = widthShift + depthShift;

                LogConsole("numberDZWidth : " + numberDZWidth);
                LogConsole("dropZoneWidth : " + dropZoneWidth);
                LogConsole("numberDZDepth : " + numberDZDepth);
                LogConsole("dropZoneDepth : " + dropZoneDepth);
                LogConsole("cornerShift : " + cornerShift.magnitude);

                for (int j = 0; j < numberDZDepth; j++)
                {
                    for (int i = 0; i < numberDZWidth; i++)
                    {
                        Vector3 origin = corner0 + widthShift * i + depthShift * j + floorUpwards * 0.02f;
                        CreateDropZone(
                            origin,
                            origin + depthShift,
                            origin + cornerShift,
                            origin + widthShift
                            );
                    }
                }
            }
        }

        #endregion

        #region Others
        private void CreatePlaneMesh(Vector3 origin, Vector3 forward, Vector3 corner, Vector3 right, EPlayAreaPlaneType planeType)
        {
            if (IS_NOT_NULL(playAreaPlanePrefab))
            {
                PlayAreaPlane plane = Instantiate(playAreaPlanePrefab, Vector3.zero, Quaternion.identity);
                if (plane)
                {
                    plane.SetVertices(new Vector3[]
                    {
                    origin,
                    forward,
                    corner,
                    right,
                    });

                    plane.transform.position = Vector3.zero;
                    plane.transform.transform.parent = rectanglePlayArea.transform;

                    plane.PlayAreaWallType = planeType;
                }
            }
        }

        //private void Create

        private void CreatePlayAreaSeparation(Vector3 middle1, Vector3 middle2, Vector3 forwardVector, Vector3 upVector)
        {
            if (IS_NOT_NULL(playAreaSeparationPrefab))
            {
                PlayAreaSeparation plane = Instantiate(playAreaSeparationPrefab, Vector3.zero, Quaternion.identity);
                if (plane)
                {
                    plane.SetVertices(new Vector3[]
                    {
                    middle1 - forwardVector * playAreaSeperatorThickness + upVector * 0.04f,
                    middle1 + forwardVector * playAreaSeperatorThickness + upVector * 0.04f,
                    middle2 + forwardVector * playAreaSeperatorThickness + upVector * 0.04f,
                    middle2 - forwardVector * playAreaSeperatorThickness + upVector * 0.04f,
                    });

                    plane.transform.position = Vector3.zero;
                    plane.transform.transform.parent = rectanglePlayArea.transform;

                    plane.PlayAreaWallType = EPlayAreaPlaneType.FLOOR;
                }
            }
        }

        private void CreateDropZone(Vector3 origin, Vector3 forward, Vector3 corner, Vector3 right)
        {
            if (IS_NOT_NULL(dropZonePrefab))
            {
                DropZonePlane plane = Instantiate(dropZonePrefab, Vector3.zero, Quaternion.identity);
                if (plane)
                {
                    plane.SetVertices(new Vector3[]
                    {
                    origin,
                    forward,
                    corner,
                    right,
                    });

                    plane.transform.position = Vector3.zero;
                    plane.transform.transform.parent = rectanglePlayArea.transform;

                    plane.PlayAreaWallType = EPlayAreaPlaneType.FLOOR;
                }
            }
        }



        #endregion
    }
}

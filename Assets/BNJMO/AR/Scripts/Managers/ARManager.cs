using System.Collections;
using System.Collections.Generic;
using BNJMO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;

[Serializable]
public class TrackedImageTupple
{
    public string ImageName = "ObjectName";
    public BAnchor TrackedObject;
}


public class ARManager : AbstractSingletonManager<ARManager>
{
    #region Public Events


    #endregion

    #region Public Methods and Getters
    public WorldRootBAnchor WorldRootBAnchor { get; private set; }

    public BAnchor CurrentBAnchorBeingPlaced { get; private set; } = null;

    public bool IsCurrenltyPlacingBAnchor { get { return CurrentBAnchorBeingPlaced != null; } }

    public bool FixWorldRoot { get; private set; } = false;

    public BAnchor SpawnBAnchorAtCursorPosition(BAnchor bAnchorPrefab, bool replicateSpawnToOthers = false)
    {
        if (IS_NOT_NULL(ARCursor.Instance)
            && IS_NOT_NULL(bAnchorPrefab))
        {
            BAnchor spawnedBAnchor = Instantiate(bAnchorPrefab, ARCursor.Instance.GetCursorPosition(), ARCursor.Instance.GetCursorRotation());
            spawnedBAnchor.Owner = BEventManager.Instance.LocalNetworkID;

            // Trigger event
            BEventReplicationType bEventReplicationType = BEventReplicationType.LOCAL;
            if (replicateSpawnToOthers)
            {
                bEventReplicationType = BEventReplicationType.TO_ALL_OTHERS;
            }
            BEventsCollection.AR_BAnchorSpawned.Invoke(new BEHandle<BAnchorInformation, string>(spawnedBAnchor.GetBAnchorInformation(), spawnedBAnchor.BAnchorID), bEventReplicationType, true);
            
            return spawnedBAnchor;
        }
        return null;
    }

    public bool ToogleFixWorldRoot()
    {
        FixWorldRoot = !FixWorldRoot;
        return FixWorldRoot;
    }

    public void SetWorldRootBAnchor(WorldRootBAnchor worldRootBAnchor)
    {
        if (WorldRootBAnchor != null
            && WorldRootBAnchor != worldRootBAnchor)
        {
            LogConsoleWarning("Overwriting World Root BAnchor!");
        }

        WorldRootBAnchor = worldRootBAnchor;
    }

    public void SpawnWorldRootBAnchor()
    {
        worldRootBAnchor = FindObjectOfType<WorldRootBAnchor>();

        // Spawn if not found
        if (worldRootBAnchor == null)
        {
            WorldRootBAnchor worldRootBAnchorPrefab = Resources.Load<WorldRootBAnchor>(BConsts.PATH_AR_WorldRootBAnchor);
            if (IS_NOT_NULL(worldRootBAnchorPrefab))
            {
                worldRootBAnchor = Instantiate(worldRootBAnchorPrefab, Vector3.zero, Quaternion.identity);
            }
        }

        // Initialize
        if (IS_NOT_NULL(worldRootBAnchor))
        {
            worldRootBAnchor.BAnchorMovedToTracker += On_WorldRootBAnchor_BAnchorMovedToTracker;

            BAnchorCursorPlacer worldRootBAnchorCursorPlacer = worldRootBAnchor.GetComponent<BAnchorCursorPlacer>();
            if (IS_NOT_NULL(worldRootBAnchorCursorPlacer))
            {
                worldRootBAnchorCursorPlacer.EndedPlacing += On_WorldRootBAnchorCursorPlacer_EndedPlacing;
            }
            BEventsCollection.AR_WorldBAnchorSet.Invoke(new BEHandle<BAnchorInformation>(worldRootBAnchor.GetBAnchorInformation()));
        }
    }

    public void SetupPlayArea()
    {
        AbstractPlayArea playArea = SpawnNewPlayArea();

        if (IS_NOT_NULL(playArea))
        {
            playArea.Owner = BEventManager.Instance.LocalNetworkID;
            playArea.SetUpPlayArea();
        }
    }

    public Vector3 GetTransformedPosition(Vector3 position)
    {
        if (IS_NOT_NULL(WorldRootBAnchor))
        {
            return WorldRootBAnchor.transform.InverseTransformPoint(position);
        }

        LogConsoleError("Trying to get the transformed position of this BAnchor but the World Root BAnchor was not set!");
        return Vector3.zero;
    }

    public Quaternion GetTransformedRotation(Quaternion rotation)
    {
        if (IS_NOT_NULL(WorldRootBAnchor))
        {
            return Quaternion.Inverse(WorldRootBAnchor.transform.rotation) * rotation;
        }

        LogConsoleError("Trying to get the transformed rotation of this BAnchor but the World Root BAnchor was not set!");
        return Quaternion.identity;
    }

    public Vector3 GetInverseTransformedPosition(Vector3 transformedPosition)
    {
        if (IS_NOT_NULL(WorldRootBAnchor))
        {
            return WorldRootBAnchor.transform.TransformPoint(transformedPosition);
        }
        LogConsoleError("Trying to get the inverse transformed position of this BAnchor but the World Root BAnchor was not set!");
        return Vector3.zero;
    }

    public Quaternion GetInverseTransformedRotation(Quaternion transformedRotation)
    {
        if (IS_NOT_NULL(WorldRootBAnchor))
        {
            return WorldRootBAnchor.transform.rotation * transformedRotation;
        }
        LogConsoleError("Trying to get the inverse transformed rotation of this BAnchor but the World Root BAnchor was not set!");
        return Quaternion.identity;
    }

    /// <summary>
    /// Called from BAnchorCursorPlacer to start placing a new BAnchor
    /// </summary>
    /// <param name="bAnchorToBePlaced"></param>
    /// <param name="bAnchorCursorPlacer"></param>
    /// <returns></returns>
    public bool OnStartPlacingBAnchor(BAnchorCursorPlacer bAnchorCursorPlacer, BAnchor bAnchorToBePlaced)
    {
        if (IS_NOT_NULL(bAnchorCursorPlacer)
            && IS_NOT_NULL(bAnchorToBePlaced))
        {
            if (IsCurrenltyPlacingBAnchor == false)
            {
                CurrentBAnchorBeingPlaced = bAnchorToBePlaced;
                return true;
            }
            else
            {
                LogConsoleWarning("Trying to select a new BAnchor for placement but one is already slected!");
            }
        }
        return false;
    }

    /// <summary>
    /// Called from BAnchorCursorPlacer to stop placing a new BAnchor
    /// </summary>
    /// <param name="bAnchorToBePlaced"></param>
    /// <param name="bAnchorCursorPlacer"></param>
    /// <returns></returns>
    public bool OnStopPlacingBAnchor(BAnchorCursorPlacer bAnchorCursorPlacer, BAnchor bAnchorToBePlaced)
    {
        if (IS_NOT_NULL(bAnchorCursorPlacer)
            && IS_NOT_NULL(bAnchorToBePlaced))
        {
            if (IsCurrenltyPlacingBAnchor == true)
            {
                if (CurrentBAnchorBeingPlaced == bAnchorToBePlaced)
                {
                    CurrentBAnchorBeingPlaced = null;
                    return true;
                }
                else
                {
                    LogConsoleWarning("Trying to unselected a BAnchor for placement but another one was found already active!");
                }
            }
            else
            {
                LogConsoleWarning("Trying to unselected a BAnchor for placement but no BAnchor placement is currently active!");
            }
        }
        return false;
    }

    public BAnchor GetBAnchorPrefab(string bAnchorID)
    {
        if (IS_NOT_NULL(bAnchorsCollection))
        {
            return bAnchorsCollection.GetBAnchorPrefab(bAnchorID);
        }
        return null;
    }

    #endregion

    #region Serialized Fields
    [SerializeField] private TrackedImageTupple[] trackedImagesTupples = new TrackedImageTupple[0];

    #endregion

    #region Private Variables
    private ARTrackedImageManager trackedImageManager;
    private BAnchorsCollection bAnchorsCollection;
    private Dictionary<string, BAnchor> trackedImagesMap = new Dictionary<string, BAnchor>();
    private WorldRootBAnchor worldRootBAnchor;

    #endregion

    #region Life Cycle
    protected override void Awake()
    {
        base.Awake();

        // Initialize callbacks from ARTrackedImageManager
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        if (trackedImageManager)
        {
            trackedImageManager.trackedImagesChanged += On_TrackedImageManager_trackedImagesChanged;
        }

        // Initialize BAnchorsCollection
        bAnchorsCollection = GetComponentWithCheck<BAnchorsCollection>();

        // Initialize Tracked Images Map
        foreach (TrackedImageTupple trackedImageTupple in trackedImagesTupples)
        {
            if (IS_NOT_NULL(trackedImageTupple.TrackedObject))
            {
                trackedImagesMap.Add(trackedImageTupple.ImageName, trackedImageTupple.TrackedObject);
                trackedImageTupple.TrackedObject.gameObject.SetActive(false);
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        BEventsCollection.AR_NewPlayAreaSet += On_AR_NewPlayAreaSet;
        BEventsCollection.AR_BAnchorSpawned += On_AR_BAnchorSpawned;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        BEventsCollection.AR_NewPlayAreaSet -= On_AR_NewPlayAreaSet;
        BEventsCollection.AR_BAnchorSpawned -= On_AR_BAnchorSpawned;
    }

    protected override void LateStart()
    {
        base.LateStart();

        // Set World Root BAnchor
        if (MotherOfManagers.Instance.ARTrackingMode != ARTrackingMode.NONE
            && WorldRootBAnchor == null)
        {
            WorldRootBAnchor = FindObjectOfType<WorldRootBAnchor>();
            if (WorldRootBAnchor == null)
            {
                WorldRootBAnchor worldRootBAnchorPrefab = Resources.Load<WorldRootBAnchor>(BConsts.PATH_AR_WorldRootBAnchor);
                if (IS_NOT_NULL(worldRootBAnchorPrefab))
                {
                    WorldRootBAnchor = Instantiate(worldRootBAnchorPrefab, Vector3.zero, Quaternion.identity);
                }
            }
        }
    }
    #endregion

    #region Events Callbacks

    private void On_TrackedImageManager_trackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        LogConsole("Added : " + eventArgs.added.Count + " | updated : " + eventArgs.updated.Count + " | removed : " + eventArgs.removed.Count);
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            if (IS_NOT_NULL(trackedImage))
            {
                string trackerName = trackedImage.referenceImage.name;
                BEventsCollection.AR_TrackerFound.Invoke(new BEHandle<string, Vector3, Quaternion>(trackerName, trackedImage.transform.position, trackedImage.transform.rotation));
                //OnTrackedImageAdded(trackedImage);
            }
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            if (IS_NOT_NULL(trackedImage))
            {
                string trackerName = trackedImage.referenceImage.name;
                BEventsCollection.AR_TrackerUpdated.Invoke(new BEHandle<string, Vector3, Quaternion>(trackerName, trackedImage.transform.position, trackedImage.transform.rotation));
                //OnTrackedImageUpdated(trackedImage);
            }
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            if (IS_NOT_NULL(trackedImage))
            {
                string trackerName = trackedImage.referenceImage.name;
                BEventsCollection.AR_TrackerLost.Invoke(new BEHandle<string>(trackerName));
                //OnTrackedImageRemoved(trackedImage);
            }
        }
    }

    private void On_WorldRootBAnchorCursorPlacer_EndedPlacing(BAnchor bAnchor)
    {
        if (ARE_EQUAL(bAnchor, worldRootBAnchor))
        {
            BEventsCollection.AR_WorldBAnchorSet.Invoke(new BEHandle<BAnchorInformation>(bAnchor.GetBAnchorInformation()));
        }
    }

    private void On_WorldRootBAnchor_BAnchorMovedToTracker(BAnchor bAnchor, string trackerName)
    {
        if (ARE_EQUAL(bAnchor, worldRootBAnchor))
        {
            BEventsCollection.AR_WorldBAnchorSet.Invoke(new BEHandle<BAnchorInformation>(bAnchor.GetBAnchorInformation()));
        }
    }

    private void On_AR_NewPlayAreaSet(BEHandle<BAnchorInformation[]> handle)
    {
        if (handle.InvokingNetworkID != BEventManager.Instance.LocalNetworkID)
        {
            AbstractPlayArea playArea = SpawnNewPlayArea();
            playArea.Owner = handle.InvokingNetworkID;

            if (IS_NOT_NULL(playArea))
            {
                playArea.SetUpPlayArea(handle.Arg1);
            }
        }
    }

    private void On_AR_BAnchorSpawned(BEHandle<BAnchorInformation, string> handle)
    {
        BAnchorInformation bAnchorInformation = handle.Arg1;
        string bAnchorID = handle.Arg2;
        BAnchor bAnchorPrefab = GetBAnchorPrefab(bAnchorID);

        if (handle.InvokingNetworkID != BEventManager.Instance.LocalNetworkID
            && IS_NOT_NULL(bAnchorPrefab)
            && IS_NOT_NULL(bAnchorInformation))
        {
            BAnchor spawnedBAnchor = Instantiate(bAnchorPrefab, Vector3.zero, Quaternion.identity);
            spawnedBAnchor.SetTransformedPosition(bAnchorInformation.TransformedPosition);
            spawnedBAnchor.SetTransformedRotation(bAnchorInformation.TransformedRotation);
            spawnedBAnchor.Owner = handle.InvokingNetworkID;
       
            BEventsCollection.AR_BAnchorSpawned.Invoke(new BEHandle<BAnchorInformation, string>(bAnchorInformation, bAnchorID), BEventReplicationType.LOCAL, true);
        }
    }

    #endregion

    #region Others

    private void OnTrackedImageAdded(ARTrackedImage trackedImage)
    {

        string trackedImageName = trackedImage.referenceImage.name;
        if (IS_KEY_CONTAINED(trackedImagesMap, trackedImageName))
        {
            // Check if World Root Anchor should stay fix
            WorldRootBAnchor worldRootBAnchor = (WorldRootBAnchor)trackedImagesMap[trackedImageName];
            if (FixWorldRoot == true
                && worldRootBAnchor != null)
            {
                return;
            }

            trackedImagesMap[trackedImageName].gameObject.SetActive(true);
            trackedImagesMap[trackedImageName].transform.position = trackedImage.transform.position;
            trackedImagesMap[trackedImageName].transform.rotation = trackedImage.transform.rotation;
        }
    }

    private void OnTrackedImageUpdated(ARTrackedImage trackedImage)
    {
        string trackedImageName = trackedImage.referenceImage.name;
        if (IS_KEY_CONTAINED(trackedImagesMap, trackedImageName))
        {
            // Check if World Root Anchor should stay fix
            WorldRootBAnchor worldRootBAnchor = (WorldRootBAnchor)trackedImagesMap[trackedImageName];
            if (FixWorldRoot == true
                && worldRootBAnchor != null)
            {
                return;
            }

            trackedImagesMap[trackedImageName].transform.position = trackedImage.transform.position;
            trackedImagesMap[trackedImageName].transform.rotation = trackedImage.transform.rotation;
        }
    }

    private void OnTrackedImageRemoved(ARTrackedImage trackedImage)
    {
        string trackedImageName = trackedImage.referenceImage.name;
        if (IS_KEY_CONTAINED(trackedImagesMap, trackedImageName))
        {
            // Check if World Root Anchor should stay fix
            WorldRootBAnchor worldRootBAnchor = (WorldRootBAnchor)trackedImagesMap[trackedImageName];
            if (FixWorldRoot == true
                && worldRootBAnchor != null)
            {
                return;
            }

            trackedImagesMap[trackedImageName].gameObject.SetActive(false);
            OnTrackedImageUpdated(trackedImage);
        }
    }

    private AbstractPlayArea SpawnNewPlayArea()
    {
        AbstractPlayArea playArea = FindObjectOfType<AbstractPlayArea>();
        if (playArea != null)
        {
            LogConsole("Destroying existing play area : " + playArea.name);
            Destroy(playArea.gameObject);
        }

        // Spawn new Play Area
        switch (MotherOfManagers.Instance.PlayAreaType)
        {
            case EPlayAreaType.RECTANGLE:
                RectanglePlayArea rectanglePlayAreaPrefab = Resources.Load<RectanglePlayArea>(BConsts.PATH_AR_PlayArea_Rectangle);
                if (IS_NOT_NULL(rectanglePlayAreaPrefab))
                {
                    playArea = Instantiate(rectanglePlayAreaPrefab);
                }
                break;

            case EPlayAreaType.NONE:
                LogConsoleWarning("No Play Area Type selected!");
                break;
        }

        return playArea;
    }
    #endregion
}

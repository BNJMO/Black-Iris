using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Sirenix.OdinInspector;

namespace BNJMO
{
    [RequireComponent(typeof(LineRenderer))]
    public abstract class AbstractPlayArea : BBehaviour
    {
        #region Public Events
        public event Action<PlayAreaBAnchor, int> NewPlayAreaBAnchorSet;

        #endregion

        #region Public Methods
        public EPlayAreaState PlayAreaState { 
            get
            {
                return playAreaState;
            }
            protected set
            {
                playAreaState = value;
                BEventsCollection.AR_PlayAreaStateUpdated.Invoke(new BEHandle<EPlayAreaState, AbstractPlayArea>(value, this));
            }
        }

        public ENetworkID Owner { get { return owner; } set { owner = value; } }

        public List<Vector3> ExtentPoints { get; protected set; } = new List<Vector3>();

        public List<PlayAreaBAnchor> PlayAreaBAnchors { get; protected set; } = new List<PlayAreaBAnchor>();
        
        public PlayAreaBAnchor CurrentPlayAreaBAnchorBeingPlaced { get; protected set; }

        /* abstract methods */
        public abstract bool IsInsidePlayArea(Vector3 position);

        public abstract void SetUpPlayArea();

        public abstract void SetUpPlayArea(BAnchorInformation[] bAnchorInformation);
        #endregion

        #region Inspector Variables
        [SerializeField] 
        protected LineRenderer myLineRenderer;

        [SerializeField]
        private PlayAreaBAnchor playAreaBAnchorPrefab;

        [SerializeField]
        [ReadOnly]
        private ENetworkID owner = ENetworkID.NONE;

        
        #endregion

        #region Private Variables
        private EPlayAreaState playAreaState = EPlayAreaState.NOT_INITIALIZED;



        #endregion

        #region Life Cycle
        protected override void OnValidate()
        {
            base.OnValidate();

            if (CanValidate() == false)
            {
                return;
            }

            if (myLineRenderer == null)
            {
                myLineRenderer = GetComponent<LineRenderer>();
                if (myLineRenderer)
                {
                    myLineRenderer.SetPositions(new Vector3[0]);
                }
            }

            if (playAreaBAnchorPrefab == null)
            {
                playAreaBAnchorPrefab = Resources.Load<PlayAreaBAnchor>(BConsts.PATH_AR_PlayAreaBAnchor);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (CurrentPlayAreaBAnchorBeingPlaced != null)
            {
                SetExtentPoints();
                DrawPlayAreaExtensions();
            }
        }

        #endregion

        #region Events Callbacks
        protected virtual void On_SpawnedPlayAreaBAnchor_StartedPlacing(PlayAreaBAnchor playAreaBAnchor)
        {
            if (IS_NOT_NULL(playAreaBAnchor)
                && IS_VALUE_CONTAINED(PlayAreaBAnchors, playAreaBAnchor)
                && IS_NULL(CurrentPlayAreaBAnchorBeingPlaced))
            {
                CurrentPlayAreaBAnchorBeingPlaced = playAreaBAnchor;
            }
        }

        protected virtual void On_SpawnedPlayAreaBAnchor_EndedPlacing(PlayAreaBAnchor playAreaBAnchor)
        {
            if (IS_NOT_NULL(playAreaBAnchor)
                && IS_VALUE_CONTAINED(PlayAreaBAnchors, playAreaBAnchor)
                && ARE_EQUAL(CurrentPlayAreaBAnchorBeingPlaced, playAreaBAnchor))
            {
                int index = PlayAreaBAnchors.IndexOf(playAreaBAnchor);
                InvokeEventIfBound(NewPlayAreaBAnchorSet, playAreaBAnchor, index);
                CurrentPlayAreaBAnchorBeingPlaced = null;
            }
        }

        #endregion

        #region Others

        protected virtual void FinalizePlayAreaSetup()
        {
            List<BAnchorInformation> bAnchorsInformation = new List<BAnchorInformation>();
            foreach (PlayAreaBAnchor playAreaBAnchor in PlayAreaBAnchors)
            {
                playAreaBAnchor.DisableReplacement();
                bAnchorsInformation.Add(playAreaBAnchor.GetBAnchorInformation());
            }

            PlayAreaState = EPlayAreaState.READY;

            // Only call replication on the side of the client that spawned it
            if (Owner == BEventManager.Instance.LocalNetworkID)
            {
                BEventsCollection.AR_NewPlayAreaSet.Invoke(new BEHandle<BAnchorInformation[]>(bAnchorsInformation.ToArray()), BEventReplicationType.TO_ALL);
            }
        }

        protected abstract void SetExtentPoints();

        protected virtual void DrawPlayAreaExtensions()
        {
            if (IS_NOT_NULL(myLineRenderer))
            {
                Vector3[] positions = ExtentPoints.ToArray();
                myLineRenderer.positionCount = positions.Length;
                myLineRenderer.SetPositions(positions);
            }
        }

        protected virtual PlayAreaBAnchor AddNewChildBAnchor(bool directlyStartPlacing = true)
        {
            if (IS_NOT_NULL(playAreaBAnchorPrefab)
                && IS_NULL(CurrentPlayAreaBAnchorBeingPlaced))
            {
                PlayAreaBAnchor spawnedPlayAreaBAnchor = Instantiate(playAreaBAnchorPrefab);
                if (IS_NOT_NULL(spawnedPlayAreaBAnchor))
                {
                    spawnedPlayAreaBAnchor.transform.parent = transform;
                    PlayAreaBAnchors.Add(spawnedPlayAreaBAnchor);
                    LogConsole("Adding new play area BAnchor : " + PlayAreaBAnchors.Count);

                    spawnedPlayAreaBAnchor.StartedPlacing += On_SpawnedPlayAreaBAnchor_StartedPlacing;
                    spawnedPlayAreaBAnchor.EndedPlacing += On_SpawnedPlayAreaBAnchor_EndedPlacing;

                    if (directlyStartPlacing)
                    {
                        spawnedPlayAreaBAnchor.OnStartPlacing();
                    }

                    return spawnedPlayAreaBAnchor;
                }
            }
            return null;
        }

        protected virtual bool RemoveChildBAnchor(PlayAreaBAnchor playAreaBAnchor)
        {
            if (IS_NOT_NULL(playAreaBAnchor))
            {
                if (IS_VALUE_CONTAINED(PlayAreaBAnchors, playAreaBAnchor))
                {
                    playAreaBAnchor.StartedPlacing -= On_SpawnedPlayAreaBAnchor_StartedPlacing;
                    playAreaBAnchor.EndedPlacing -= On_SpawnedPlayAreaBAnchor_EndedPlacing;

                    PlayAreaBAnchors.Remove(playAreaBAnchor);
                    if (CurrentPlayAreaBAnchorBeingPlaced == playAreaBAnchor)
                    {
                        CurrentPlayAreaBAnchorBeingPlaced = null;
                    }
                    Destroy(playAreaBAnchor.gameObject);
                    return true;
                }
                LogConsoleWarning("Couldn't remove the given PlayAreaBAnchor : " + playAreaBAnchor.gameObject.name);
            }
            return false;
        }


        #endregion
    }
}

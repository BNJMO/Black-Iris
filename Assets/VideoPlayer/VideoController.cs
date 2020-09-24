using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace BNJMO
{
    public struct SPingTupple
    {
        public float HalfPing;
        public ENetworkID NetworkID;
    }
        

    public class VideoController : BBehaviour
    {
        #region Public Events


        #endregion

        #region Public Methods
        public void Play()
        {
            if (IS_NOT_NULL(videoPlayer))
            {
                switch (BEventManager.Instance.NetworkState)
                {
                    case ENetworkState.NOT_CONNECTED:
                        videoPlayer.Stop();
                        videoPlayer.Play();
                        break;

                    case ENetworkState.HOST:
                        videoPlayer.Stop();

                        if (playAfterPingDelay == true)
                        {
                            pingMap.Clear();
                            foreach (ENetworkID connectedNetworkID in BEventManager.Instance.GetConnectedNetworkIDs())
                            {
                                if (connectedNetworkID == ENetworkID.HOST)
                                {
                                    continue;
                                }

                                pingMap.Add(connectedNetworkID, 0);
                                BEventsCollection.NETWORK_RequestPing.Invoke(new BEHandle(), BEventReplicationType.TO_TARGET, true, connectedNetworkID);
                            }

                            StartNewCoroutine(ref delayedPlayEnumerator, DelayedPlayCoroutine());
                        }
                        else
                        {
                            videoPlayer.Play();
                            BEventsCollection.BI_PlayVideo.Invoke(new BEHandle(), BEventReplicationType.TO_ALL, true);
                        }
                        break;
                }

            }
        }

        #endregion

        #region Inspector Variables
        [SerializeField]
        private VideoPlayer videoPlayer;

        [SerializeField]
        private bool playAfterPingDelay = true;

        #endregion

        #region Private Variables
        private Dictionary<ENetworkID, int> pingMap = new Dictionary<ENetworkID, int>();
        private IEnumerator delayedPlayEnumerator;
        private bool isPlayAlgoRunning = false;
        private List<SPingTupple> pingTupples = new List<SPingTupple>();
        private float timeDifference;
        private float lastTime;

        #endregion

        #region Life Cycle
        protected override void OnValidate()
        {
            base.OnValidate();

            if (CanValidate())
            {
                if (videoPlayer == null)
                {
                    videoPlayer = GetComponent<VideoPlayer>();
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            BEventsCollection.NETWORK_RequestPing += On_NETWORK_RequestPing;
            BEventsCollection.NETWORK_SharePing += On_NETWORK_SharePing;
            BEventsCollection.BI_PlayVideo += On_BI_PlayVideo;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            BEventsCollection.NETWORK_RequestPing -= On_NETWORK_RequestPing;
            BEventsCollection.NETWORK_SharePing -= On_NETWORK_SharePing;
            BEventsCollection.BI_PlayVideo += On_BI_PlayVideo;
        }

        protected override void Update()
        {
            base.Update();

            if (isPlayAlgoRunning == true)
            {
                // Delayed play event
                if (pingTupples.Count > 1
                    && Time.time - lastTime >= timeDifference)
                {
                    SPingTupple maxPing = pingTupples[0];
                    SPingTupple nextPing = pingTupples[1];

                    timeDifference = maxPing.HalfPing - nextPing.HalfPing;
                    lastTime = Time.time;

                    LogConsole("Play on " + maxPing.NetworkID + " with delay : " + timeDifference);

                    BEventsCollection.BI_PlayVideo.Invoke(new BEHandle(), BEventReplicationType.TO_TARGET, true, maxPing.NetworkID);

                    pingTupples.RemoveAt(0);
                }
                else if (pingTupples.Count > 0
                    && Time.time - lastTime >= timeDifference)
                {
                    SPingTupple lastPing = pingTupples[0];
                    BEventsCollection.BI_PlayVideo.Invoke(new BEHandle(), BEventReplicationType.TO_TARGET, true, lastPing.NetworkID);

                    LogConsole("Play on " + lastPing.NetworkID + " with delay : " + lastPing.HalfPing);

                    timeDifference = lastPing.HalfPing;
                    lastTime = Time.time;
                }
                else if (Time.time - lastTime >= timeDifference)
                {
                    videoPlayer.Play();
                    isPlayAlgoRunning = false;
                }
            }
        }

        #endregion

        #region Events Callbacks
        private void On_NETWORK_RequestPing(BEHandle handle)
        {
            if (ARE_NOT_EQUAL(BEventManager.Instance.LocalNetworkID, handle.InvokingNetworkID))
            {
                int myPing = BEventManager.Instance.GetPing();
                BEventsCollection.NETWORK_SharePing.Invoke(new BEHandle<int>(myPing), BEventReplicationType.TO_TARGET, true, handle.InvokingNetworkID);
            }
        }

        private void On_NETWORK_SharePing(BEHandle<int> handle)
        {
            ENetworkID invokingNetworkID = handle.InvokingNetworkID;
            if (ARE_NOT_EQUAL(BEventManager.Instance.LocalNetworkID, invokingNetworkID)
                && IS_KEY_CONTAINED(pingMap, invokingNetworkID))
            {
                pingMap[invokingNetworkID] = handle.Arg1;
            }
        }

        private void On_BI_PlayVideo(BEHandle handle)
        {
            if (handle.InvokingNetworkID != BEventManager.Instance.LocalNetworkID 
                && IS_NOT_NULL(videoPlayer))
            {
                videoPlayer.Stop();
                videoPlayer.Play();
            }
        }

        #endregion

        #region Others
        private IEnumerator DelayedPlayCoroutine()
        {
            yield return new WaitForSeconds(1.0f);

            // sorted list with 0 having highest ping
            pingTupples = new List<SPingTupple>();

            // Sort 
            while(pingMap.Count != 0)
            {
                SPingTupple maxPing = new SPingTupple();
                foreach (var tuppleJ in pingMap)
                {
                    if (tuppleJ.Value > maxPing.HalfPing)
                    {
                        maxPing.HalfPing = tuppleJ.Value / 2.0f;
                        maxPing.NetworkID = tuppleJ.Key;
                    }
                }

                pingTupples.Add(maxPing);
                pingMap.Remove(maxPing.NetworkID);
            }

            isPlayAlgoRunning = true;
        }

        #endregion
    }
}

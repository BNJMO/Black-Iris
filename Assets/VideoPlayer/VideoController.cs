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
                        break;
                }

            }
        }

        #endregion

        #region Inspector Variables
        [SerializeField]
        private VideoPlayer videoPlayer;

        #endregion

        #region Private Variables
        private Dictionary<ENetworkID, int> pingMap = new Dictionary<ENetworkID, int>();
        private IEnumerator delayedPlayEnumerator;

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
            if (IS_NOT_NULL(videoPlayer))
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
            List<SPingTupple> pingTupples = new List<SPingTupple>();

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

            // Delayed play event
            while (pingTupples.Count > 1)
            {
                SPingTupple maxPing = pingTupples[0];
                SPingTupple nextPing = pingTupples[1];
                float timeDifference = maxPing.HalfPing - nextPing.HalfPing;

                LogConsole("Play on " + maxPing.NetworkID + " with delay : " + timeDifference);

                BEventsCollection.BI_PlayVideo.Invoke(new BEHandle(), BEventReplicationType.TO_TARGET, true, maxPing.NetworkID);

                pingTupples.RemoveAt(0);

                yield return new WaitForSecondsRealtime(timeDifference / 1000.0f);
            }

            if (pingTupples.Count > 0)
            {
                SPingTupple lastPing = pingTupples[0];
                BEventsCollection.BI_PlayVideo.Invoke(new BEHandle(), BEventReplicationType.TO_TARGET, true, lastPing.NetworkID);

                LogConsole("Play on " + lastPing.NetworkID + " with delay : " + lastPing.HalfPing);

                yield return new WaitForSecondsRealtime(lastPing.HalfPing / 1000.0f);
            }


            videoPlayer.Play();
        }

        #endregion
    }
}

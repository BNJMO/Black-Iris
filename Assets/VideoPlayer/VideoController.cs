using RenderHeads.Media.AVProVideo;
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
        public bool UseSynchronization { get { return useSynchronization; } set { useSynchronization = value; } }
        
        public void Play()
        {
            if (IS_NOT_NULL(videoPlayer))
            {
                switch (BEventManager.Instance.NetworkState)
                {
                    case ENetworkState.NOT_CONNECTED:
                        StopVideo();
                        PlayVideo();
                        break;

                    case ENetworkState.HOST:
                        StopVideo();

                        if (UseSynchronization == true)
                        {
                            LogConsole("Using synchronization ");
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
                            LogConsole("Not using synchronization ");
                            PlayVideo();
                            BEventsCollection.BI_PlayVideo.Invoke(new BEHandle(), BEventReplicationType.TO_ALL, true);
                        }
                        break;
                }

            }
        }

        public void Pause()
        {
            if (IS_NOT_NULL(videoPlayer))
            {
                videoPlayer.Pause();
            }
        }

        public void Sync()
        {
            if (IS_NOT_NULL(videoPlayer)
                && videoPlayer.isPlaying)
            {
                BEventsCollection.BI_SynchFrame.Invoke(new BEHandle<int>((int)videoPlayer.frame), BEventReplicationType.TO_ALL_OTHERS);
            }
        }

        #endregion

        #region Inspector Variables
        [SerializeField]
        private VideoPlayer videoPlayer;

        [SerializeField]
        private MediaPlayer mediaPlayer;

        [SerializeField]
        private bool useMediaPlayer = true;

        [SerializeField]
        private bool useSynchronization = true;

        [SerializeField]
        private int syncThreshold = 1;

        [SerializeField]
        private bool automaticSync = false;

        [SerializeField]
        private float syncRate = 2.0f;


        #endregion

        #region Private Variables
        private Dictionary<ENetworkID, int> pingMap = new Dictionary<ENetworkID, int>();
        private IEnumerator delayedPlayEnumerator;
        private IEnumerator syncVideoEnumerator;
        private bool isPlayAlgoRunning = false;
        private List<SPingTupple> pingTupples = new List<SPingTupple>();
        private float timeDifference;
        private float lastTime;

        private float lastFrame;
        private float lastFrameTime;

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

                if (mediaPlayer == null)
                {
                    mediaPlayer = GetComponent<MediaPlayer>();
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            BEventsCollection.NETWORK_RequestPing += On_NETWORK_RequestPing;
            BEventsCollection.NETWORK_SharePing += On_NETWORK_SharePing;
            BEventsCollection.BI_PlayVideo += On_BI_PlayVideo;
            BEventsCollection.BI_SynchFrame += On_BI_SynchFrame;

            if (videoPlayer)
            {
                videoPlayer.prepareCompleted += On_VideoPlayer_prepareCompleted;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            BEventsCollection.NETWORK_RequestPing -= On_NETWORK_RequestPing;
            BEventsCollection.NETWORK_SharePing -= On_NETWORK_SharePing;
            BEventsCollection.BI_PlayVideo -= On_BI_PlayVideo;
            BEventsCollection.BI_SynchFrame -= On_BI_SynchFrame;
        }

        protected override void LateStart()
        {
            base.LateStart();

            if (videoPlayer
                && useMediaPlayer == false)
            {
                LogConsole("Prepare video player");
                videoPlayer.Prepare();
            }
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

                    timeDifference = (maxPing.HalfPing - nextPing.HalfPing) / 1000.0f;
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


                    timeDifference = lastPing.HalfPing / 1000.0f;
                    lastTime = Time.time;

                    LogConsole("Play on " + lastPing.NetworkID + " with delay : " + timeDifference);

                    pingTupples.RemoveAt(0);
                }
                else if (Time.time - lastTime >= timeDifference)
                {
                    PlayVideo();
                    if (automaticSync == true)
                    {
                        StartNewCoroutine(ref syncVideoEnumerator, SyncVideoCoroutine());
                    }
                    isPlayAlgoRunning = false;
                }
            }


            if (videoPlayer)
            {
                LogCanvas("VideoPlayer", "Frame : " + videoPlayer.frame);

                if (videoPlayer.isPlaying)
                {
                    float elapsedFrames = videoPlayer.frame - lastFrame;
                    float elapsedTime = Time.time - lastFrameTime;

                    //LogConsole("Frames : " + elapsedFrames + " - " + elapsedTime);

                    lastFrame = videoPlayer.frame;
                    lastFrameTime = Time.time;
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
                StopVideo();
                PlayVideo();
            }
        }

        private void On_BI_SynchFrame(BEHandle<int> handle)
        {
            if (BEventManager.Instance.LocalNetworkID != handle.InvokingNetworkID
                && videoPlayer
                && Mathf.Abs(videoPlayer.frame - handle.Arg1) > syncThreshold)
            {
                videoPlayer.frame = handle.Arg1;

                if (videoPlayer.isPlaying == false)
                {
                    videoPlayer.Play();
                }
            }
        }


        private void On_VideoPlayer_prepareCompleted(VideoPlayer source)
        {
            LogConsole("Prepare completed");
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

        private void PlayVideo()
        {
            if (useMediaPlayer == true
                && mediaPlayer)
            {
                mediaPlayer.Play();
            }
            else if (videoPlayer)
            {
                lastFrameTime = Time.time;
                LogConsole("video player prepared : " + videoPlayer.isPrepared);
                videoPlayer.Play();
            }
        }

        private void StopVideo()
        {
            if (useMediaPlayer == true
                && mediaPlayer)
            {
                mediaPlayer.Stop();
                mediaPlayer.Control.Stop();
            }
            else if (videoPlayer)
            {
                videoPlayer.Stop();
            }
        }

        private IEnumerator SyncVideoCoroutine()
        {
            if (videoPlayer)
            {
                while (videoPlayer.isPlaying == true)
                {
                    BEventsCollection.BI_SynchFrame.Invoke(new BEHandle<int>((int)videoPlayer.frame), BEventReplicationType.TO_ALL_OTHERS);

                    yield return new WaitForSeconds(syncRate);
                }
            }
        }

        #endregion
    }
}

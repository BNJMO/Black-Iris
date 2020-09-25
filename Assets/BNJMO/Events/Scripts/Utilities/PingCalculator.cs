using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class PingCalculator : AbstractSingletonManager<PingCalculator>
    {
        #region Public Events


        #endregion

        #region Public Methods
        public int AveragePing { get; private set; }
        public int CurrentPing { get; private set; }

        #endregion

        #region Inspector Variables
        [SerializeField]
        private float pingRefreshRate = 1.0f;

        [SerializeField]
        private int averageListMaxCount = 20;

        [SerializeField]
        private float pingStartCalculationDelay = 1.5f;

        [SerializeField]
        private float pingDropTreshold = 300;
        #endregion

        #region Private Variables
        private IEnumerator pingEnumerator;
        private IEnumerator pingDelayedStartEnumerator;
        private Queue<int> averagePings = new Queue<int>();

        #endregion

        #region Life Cycle
        protected override void OnEnable()
        {
            base.OnEnable();

            BEventsCollection.NETWORK_NetworkStateUpdated += On_NETWORK_NetworkStateUpdated;
            BEventsCollection.NETWORK_CalculateRTT += On_NETWORK_CalculateRTT;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            BEventsCollection.NETWORK_NetworkStateUpdated -= On_NETWORK_NetworkStateUpdated;
            BEventsCollection.NETWORK_CalculateRTT -= On_NETWORK_CalculateRTT;
        }

        protected override void Update()
        {
            base.Update();

            LogCanvas("Ping", "Avg Ping : " + AveragePing);
        }

        //protected override void Start()
        //{
        //    base.Start();

        //    StartNewCoroutine(ref pingEnumerator, PingCoroutine());
        //}

        #endregion

        #region Events Callbacks
        private void On_NETWORK_NetworkStateUpdated(BEHandle<ENetworkState> handle)
        {
            if (handle.Arg1 == ENetworkState.NOT_CONNECTED)
            {
                StopCoroutine(pingEnumerator);
                AveragePing = 0;
            }
            else
            {
                if (pingStartCalculationDelay > 0.0f)
                {
                    StartNewCoroutine(ref pingDelayedStartEnumerator, PingDelayedStartCortouine());
                }
                else
                {
                    StartNewCoroutine(ref pingEnumerator, PingCoroutine());
                }
            }
        }

        private void On_NETWORK_CalculateRTT(BEHandle<ENetworkID, int> handle)
        {
            ENetworkID requestingNetworkID = handle.Arg1;
            int startTime = handle.Arg2;

            // Half-way
            if (BEventManager.Instance.LocalNetworkID != requestingNetworkID)
            {
                BEventsCollection.NETWORK_CalculateRTT.Invoke(new BEHandle<ENetworkID, int>(requestingNetworkID, startTime), BEventReplicationType.TO_TARGET, false, requestingNetworkID);
            }
            // Round Trip
            else
            {
                CurrentPing = BUtils.GetTimeAsInt() - startTime;

                if (CurrentPing < pingDropTreshold)
                {
                    CalculateAvgPing(CurrentPing);
                }
                else
                {
                    LogConsole("Ping dropped : " + CurrentPing);
                }
            }
        }

        #endregion

        #region Others
        private IEnumerator PingCoroutine()
        {
            while (BEventManager.Instance.NetworkState != ENetworkState.NOT_CONNECTED)
            //while (true)
            {
                RequestPing();

                yield return new WaitForSeconds(pingRefreshRate);
            }
        }

        private void RequestPing()
        {
            int currentTime = BUtils.GetTimeAsInt();
            BEventsCollection.NETWORK_CalculateRTT.Invoke(new BEHandle<ENetworkID, int>(BEventManager.Instance.LocalNetworkID, currentTime), BEventReplicationType.TO_TARGET, false, ENetworkID.HOST);
        }

        private void CalculateAvgPing(int newPing)
        {
            averagePings.Enqueue(newPing);

            if (averagePings.Count > averageListMaxCount)
            {
                averagePings.Dequeue();
            }

            float averagePing = 0;
            foreach (float ping in averagePings)
            {
                averagePing += ping;
            }

            averagePing /= averagePings.Count;
            AveragePing = (int) averagePing;
        }

        private IEnumerator PingDelayedStartCortouine()
        {
            yield return new WaitForSeconds(pingStartCalculationDelay);

            StartNewCoroutine(ref pingEnumerator, PingCoroutine());
        }

        #endregion
    }
}

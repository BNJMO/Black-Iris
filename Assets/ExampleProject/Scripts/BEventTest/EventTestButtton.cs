using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNJMO;

public class EventTestButtton : BBehaviour
{

    #region Public Methods
    public void OnIncrementToAll()
    {
        LogConsole("OnIncrementToAll : <color=red>[" + BUtils.GetTimeAsString() + "] </color>");
        BEventsCollection.TEST_CounterIncrement.Invoke(new BEHandle<int>(counter), BEventReplicationType.TO_ALL, true);
    }
    
    public void OnIncrementLocal()
    {
        BEventsCollection.TEST_CounterIncrement.Invoke(new BEHandle<int>(counter), BEventReplicationType.LOCAL, true);
    }
    
    public void OnIncrementToHost()
    {
        BEventsCollection.TEST_CounterIncrement.Invoke(new BEHandle<int>(counter), BEventReplicationType.TO_TARGET, true, ENetworkID.HOST);
    }

    public void OnIncrementToTarget()
    {
        BEventsCollection.TEST_CounterIncrement.Invoke(new BEHandle<int>(counter), BEventReplicationType.TO_TARGET, true, ENetworkID.CLIENT_1);
    }

    /* Benchmark */
    public void OnFloatTest()
    {
        string serialized = BUtils.SerializeObject(new BEHandle<int>(counter));
        LogConsole(serialized);
        //BUtils.DeserializeObject<BAnchorInformation>(serialized);
        
        BEventsCollection.TEST_FloatTest.Invoke(new BEHandle<float>(BUtils.GetTimeAsInt()), BEventReplicationType.TO_ALL, true);
    }

    public void OnVector3Test()
    {
        BEventsCollection.TEST_Vector3Test.Invoke(new BEHandle<Vector3>(new Vector3(5.0450f, -3.24533f, 704.7499f)), BEventReplicationType.TO_ALL, true);
    }

    public void OnObjectTest()
    {
        BallDroneBAnchor ballDroneBAnchor = FindObjectOfType<BallDroneBAnchor>();
        if (IS_NOT_NULL(ballDroneBAnchor))
        {
            BEventsCollection.TEST_ObjectTest.Invoke(new BEHandle<BallDroneBAnchor>(ballDroneBAnchor), BEventReplicationType.TO_ALL, true);
        }
    }

    public void OnNativeIntTest()
    {
        AbstractBEventDispatcher bEventDispatcher = BEventManager.Instance.BEventDispatcher;
        if (IS_NOT_NULL(bEventDispatcher))
        {
            switch (bEventDispatcher.GetBEventDispatcherType())
            {
                case BEventDispatcherType.MIRROR:
                    MirrorBEventDispatcher mirrorBEventDispatcher = (MirrorBEventDispatcher)bEventDispatcher;
                    if (IS_NOT_NULL(mirrorBEventDispatcher)
                        && IS_NOT_NULL(mirrorBEventDispatcher.LocalMirrorPlayerBEventListener))
                    {
                        switch (BEventManager.Instance.NetworkState)
                        {
                            case ENetworkState.HOST:
                                mirrorBEventDispatcher.LocalMirrorPlayerBEventListener.Rpc_TestInteger(counter);
                                break;

                            case ENetworkState.CLIENT:
                                mirrorBEventDispatcher.LocalMirrorPlayerBEventListener.Cmd_TestInteger(counter);
                                break;
                        }
                    }
                    break;


                case BEventDispatcherType.UBI_INTERACT:
                    UbiiBEventDispatcher ubiiBEventDispatcher = (UbiiBEventDispatcher)bEventDispatcher;
                    if (IS_NOT_NULL(ubiiBEventDispatcher))
                    {
                        ubiiBEventDispatcher.Publish_TOPIC_TEST_INT(counter);
                    }
                    break;
            }
        }
    }
    #endregion

    #region Inspector Variables
    [SerializeField] 
    private BText counterBText;

    [SerializeField] 
    private BText networkIDText;
    #endregion

    #region Private Variables
    private int counter = 0;

    #endregion

    #region Life Cycle
    protected override void OnEnable()
    {
        base.OnEnable();

        BEventsCollection.NETWORK_NetworkStateUpdated += On_NETWORK_NetworkStateUpdated;
        BEventsCollection.TEST_CounterIncrement += On_TEST_CounterIncrement;
        BEventsCollection.TEST_FloatTest += On_TEST_FloatTest;
        BEventsCollection.TEST_Vector3Test += On_TEST_Vector3Test;
        BEventsCollection.TEST_ObjectTest += On_TEST_ObjectTest;

    }

    protected override void OnDisable()
    {
        base.OnDisable();

        BEventsCollection.NETWORK_NetworkStateUpdated -= On_NETWORK_NetworkStateUpdated;
        BEventsCollection.TEST_CounterIncrement -= On_TEST_CounterIncrement;
        BEventsCollection.TEST_FloatTest -= On_TEST_FloatTest;
        BEventsCollection.TEST_Vector3Test -= On_TEST_Vector3Test;
        BEventsCollection.TEST_ObjectTest -= On_TEST_ObjectTest;
    }
    #endregion

    #region Events Callbacks
    private void On_NETWORK_NetworkStateUpdated(BEHandle<ENetworkState> handle)
    {
        if (networkIDText)
        {
            networkIDText.SetText("NetworkID : " + BEventManager.Instance.LocalNetworkID);
        }
    }

    private void On_TEST_CounterIncrement(BEHandle<int> handle)
    {
        counter = handle.Arg1 + 1;

        if (IS_NOT_NULL(counterBText))
        {
            counterBText.SetText("Counter : " + counter);
        }
    }


    private void On_TEST_FloatTest(BEHandle<float> handle)
    {
        LogConsole("On_TEST_FloatTest : " + BUtils.GetTimeAsString());
    }

    private void On_TEST_Vector3Test(BEHandle<Vector3> handle)
    {
        LogConsole("On_TEST_Vector3Test : " + BUtils.GetTimeAsString());
    }

    private void On_TEST_ObjectTest(BEHandle<BallDroneBAnchor> handle)
    {
        LogConsole("On_TEST_ObjectTest : " + BUtils.GetTimeAsString());
    }

    #endregion
}

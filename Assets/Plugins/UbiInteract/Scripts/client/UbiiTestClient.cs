using System;
using System.Threading.Tasks;
using UnityEngine;
using Ubii.Services;
using Ubii.TopicData;
using Ubii.UtilityFunctions.Parser;
using UnityEngine.UI;

public class UbiiTestClient : MonoBehaviour, IUbiiClient
{
    NetMQUbiiClient client;

    [Header("Network configuration")]
    [Tooltip("Host ip the client connects to. Default is localhost.")]
    public string ip = "localhost";
    [Tooltip("Port for the client connection to the server. Default is 8101.")]
    public int port = 8101;

    [Header("Test scene settings")]
    [Tooltip("Cube for testing subscribe/publish behavior.")]
    public GameObject cube;

    [SerializeField]
    private Text wazzaText;

    private Vector3 newPos;
    private Vector3 newMoveH;
    private Vector3 newMoveV;
    private Quaternion newRot;
    private string wazzaString = "Wazza";

    // prevents errors
    private bool subscribed;

    private async void Start()
    {
        // Initializing NetMQUbiiClient (sets serverSpecification & clientRegistration, starts sockets)
        client = new NetMQUbiiClient(null, "testClient", ip, port);
        Debug.Log("Initilizing connection to : " + ip + " - " + port);
        await client.Initialize();
        Debug.Log("Connection established");

        Debug.Log("Subscribing to test topics");
        await Subscribe("TestTopicPos", TestTopicDataSubPos);
        await Subscribe("TestTopicRot", TestTopicDataSubRot);
        await Subscribe("MoveCubeH", MoveCubeH);
        await Subscribe("MoveCubeV", MoveCubeV);
        await Subscribe("SayWazza", SayWazza);
        subscribed = true;

        newPos = cube.transform.position;
        newRot = cube.transform.rotation;
        wazzaString = "Wazza";
    }

    private void MoveCubeH(TopicDataRecord obj)
    {
        newMoveH = UbiiParser.ProtoToUnity(obj.Vector3);
    }

    private void MoveCubeV(TopicDataRecord obj)
    {
        newMoveV = UbiiParser.ProtoToUnity(obj.Vector3);
    }
     
    private void SayWazza(TopicDataRecord obj)
    {
        wazzaString += obj.String;
    }

    // SPACE moves Cube, RETURN rotates cube
    private void Update()
    {
        // Only for testing and demonstration purposes
        // Sends a random vector on subscribed topic, that the client also publishes to; vector moves the cube in the "Test" scene
        if (Input.GetKeyDown(KeyCode.Space) && subscribed)
        {

            Debug.Log("Send example topic data for position");
            SendTopicDataTestPosition();
        }
        // Sends a random Quaternion on subscribed topic, that the client also publishes to; Quaternion rotates the cube in the "Test" scene
        if (Input.GetKeyDown(KeyCode.Return) && subscribed)
        {

            Debug.Log("Send example topic data for rotation");
            SendTopicDataTestRotation();
        }
        if (Input.GetAxis("Fire1") != 0 && subscribed)
            Publish(UbiiParser.UnityToProto("MoveCubeH", new Vector3(Input.GetAxis("Fire1"), 0, 0)));
        else
            newMoveH = Vector3.zero;

        if (Input.GetAxis("Fire2") != 0 && subscribed)
            Publish(UbiiParser.UnityToProto("MoveCubeV", new Vector3(0, Input.GetAxis("Fire2"), 0)));
        else
            newMoveV = Vector3.zero;

        if (Input.GetButton("Fire3") && subscribed)
        {
            Publish(UbiiParser.UnityToProto("SayWazza", "a"));
        }

        //cube.transform.position = newPos;
        cube.transform.rotation = newRot;
        cube.transform.position = Vector3.Lerp(cube.transform.position, cube.transform.position + newMoveH + newMoveV, Time.deltaTime * 10f);
        if (wazzaText)
        {
            wazzaText.text = wazzaString;
        }
    }


    #region Test functions
    // Test function subscribing to "TestTopic" to demonstrate how topic subscription works
    public void SendTopicDataTestPosition()
    {
        // set some random coordinates for the cube in the test scene
        float x = UnityEngine.Random.Range(-7f, 7f);
        float y = UnityEngine.Random.Range(-3f, 5f);
        Vector2 v = new Vector2(x, y);
        // Use utility function to publish on topic with data
        Publish(UbiiParser.UnityToProto("TestTopicPos", v));
    }

    private void SendTopicDataTestRotation()
    {
        // set some random coordinates for the cube in the test scene
        float x = UnityEngine.Random.Range(-7f, 7f);
        float y = UnityEngine.Random.Range(-3f, 5f);
        float z = UnityEngine.Random.Range(-7f, 7f);
        float w = UnityEngine.Random.Range(-3f, 5f);
        Quaternion q = new Quaternion(x, y, z, w);

        // publish topic data
        Publish(UbiiParser.UnityToProto("TestTopicRot", q));
    }

    public void TestTopicDataSubPos(TopicDataRecord topicDataRecord)
    {
        // position cannot be directly set as this does not run from main thread, so save position to global variable which will be applied in Update() in main thread
        newPos = UbiiParser.ProtoToUnity(topicDataRecord.Vector3);
    }
    public void TestTopicDataSubRot(TopicDataRecord topicDataRecord)
    {
        // rotation cannot be directly set as this does not run from main thread, so save position to global variable which will be applied in Update() in main thread
        newRot = UbiiParser.ProtoToUnity(topicDataRecord.Quaternion);
    }
    #endregion

    public Task<ServiceReply> CallService(ServiceRequest request)
    {
        return client.CallService(request);
    }

    public void Publish(TopicData topicData)
    {
        client.Publish(topicData);
    }

    public Task<ServiceReply> Subscribe(string topic, Action<TopicDataRecord> callback)
    {
        return client.Subscribe(topic, callback);
    }

    public Task<ServiceReply> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
    {
        return client.SubscribeRegex(regex, callback);
    }

    public Task<ServiceReply> Unsubscribe(string topic)
    {
        return client.Unsubscribe(topic);
    }


    private void OnDisable()
    {
        client.ShutDown();
        Debug.Log("Shutting down UbiiClient");
    }
}


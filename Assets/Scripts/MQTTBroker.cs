using MQTTnet.Server;
using MQTTnet;
using UnityEngine;
using System.Text;
using Mapbox.Utils;

public class MQTTBroker : MonoBehaviour
{
    private MqttServer mqttServer;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        var option = new MqttServerOptionsBuilder().WithDefaultEndpoint().WithDefaultEndpointPort(3030).Build();
        mqttServer = new MqttFactory().CreateMqttServer(option);

        mqttServer.ClientConnectedAsync += async (ClientConnectedEventArgs e) =>
        {
            Debug.Log("Client connected: " + e.ClientId);
        };
        mqttServer.InterceptingPublishAsync += async (InterceptingPublishEventArgs e) =>
        {
            string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            string[] subStr = message.Split(',');
            Vector2d pos = new Vector2d(double.Parse(subStr[1].Substring(0, subStr[1].Length - 1)), double.Parse(subStr[0].Substring(1)));

            gameManager.MoveCar(pos, 0);
        };

        mqttServer.StartAsync().Wait();
    }

    private void OnDestroy()
    {
        mqttServer.StopAsync().Wait();
    }
}

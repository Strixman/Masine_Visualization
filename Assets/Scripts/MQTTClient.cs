using MQTTnet.Client;
using MQTTnet;
using UnityEngine;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mapbox.Utils;
using System.Collections.Generic;

public class MQTTClient : MonoBehaviour
{
    private IMqttClient mqttClient;
    private GameManager gameManager;
    private List<string> clients = new List<string>();
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        var options = new MqttClientOptionsBuilder().WithClientId("test").WithTcpServer("127.0.0.1", 3030).Build();
        mqttClient = new MqttFactory().CreateMqttClient();

        mqttClient.ConnectedAsync += async (MqttClientConnectedEventArgs e) =>
        {
            var subOptions = new MqttClientSubscribeOptionsBuilder().WithTopicFilter("VehicleLocation").Build();
            await mqttClient.SubscribeAsync(subOptions);
        };

        mqttClient.ApplicationMessageReceivedAsync += (MqttApplicationMessageReceivedEventArgs e) =>
        {
            string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            string[] payload = message.Split('$');

            var (isNewClinet, clientIndex) = GetClientIndex(payload[0]);
            if (isNewClinet)
            {
                gameManager.AddVehicle(clientIndex);
            }

            Vector2d pos = new Vector2d(double.Parse(payload[2]), double.Parse(payload[3]));

            //Debug.Log(e.ClientId + " " + pos);
            //gameManager.MoveCar(pos, 0);
            gameManager.UpdateVehicle(clientIndex, pos);

            return Task.CompletedTask;
        };

        mqttClient.ConnectAsync(options, CancellationToken.None).Wait();
    }

    private void OnDestroy()
    {
        mqttClient.DisconnectAsync().Wait();
    }

    private (bool, int) GetClientIndex(string clientName)
    {
        for(int i = 0; i < clients.Count; i++)
        {
            if (clients[i] == clientName)
            {
                return (false, i);
            }
        }
        clients.Add(clientName);
        return (true, clients.Count - 1);
    }
}

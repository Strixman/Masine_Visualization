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
    private Dictionary<string, int> clients = new Dictionary<string, int>();
    private int carID = 0;
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        var options = new MqttClientOptionsBuilder().WithClientId(System.Environment.MachineName).WithTcpServer("127.0.0.1", 3030).Build();
        mqttClient = new MqttFactory().CreateMqttClient();

        mqttClient.ConnectedAsync += async (MqttClientConnectedEventArgs e) =>
        {
            var subOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter("VehicleConnected")
                .WithTopicFilter("VehicleDisconnected")
                .WithTopicFilter("VehicleLocation")
                .WithTopicFilter("VehicleSpeed")
                .WithTopicFilter("VehicleTemperature")
                .Build();
            await mqttClient.SubscribeAsync(subOptions);
        };

        mqttClient.ApplicationMessageReceivedAsync += (MqttApplicationMessageReceivedEventArgs e) =>
        {
            string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            string[] payload = message.Split('$');

            switch (e.ApplicationMessage.Topic)
            {
                case "VehicleConnected":
                    AddClient(payload[0]);
                    break;
                case "VehicleDisconnected":
                    RemoveClient(payload[0]);
                    break;
                case "VehicleLocation":
                    {
                        int index = GetClientIndex(payload[0]);
                        Vector2d pos = new Vector2d(double.Parse(payload[2]), double.Parse(payload[3]));
                        gameManager.UpdateLocation(index, pos);
                        break;
                    }
                case "VehicleSpeed":
                    {
                        int index = GetClientIndex(payload[0]);
                        float speed = float.Parse(payload[2]);
                        gameManager.UpdateSpeed(index, speed);
                        break;
                    }
                case "VehicleTemperature":
                    {
                        int index = GetClientIndex(payload[0]);
                        float temperature = float.Parse(payload[2]);
                        gameManager.UpdateTemperature(index, temperature);
                        break;
                    }
                default:
                    break;
            }

            return Task.CompletedTask;
        };

        mqttClient.ConnectAsync(options, CancellationToken.None).Wait();
    }

    private void OnDestroy()
    {
        mqttClient.DisconnectAsync().Wait();
    }

    private void RemoveClient(string clientName)
    {
        foreach(var (name, index) in clients)
        {
            if(name == clientName)
            {
                gameManager.RemoveVehicle(index);
                clients.Remove(name);
            }
        }
    }

    private void AddClient(string clientName)
    {
        if (clients.ContainsKey(clientName)) return;
        clients.Add(clientName, carID);
        gameManager.AddVehicle(carID);
        carID++;
    }

    private int GetClientIndex(string clientName)
    {
        return clients[clientName];
    }
}

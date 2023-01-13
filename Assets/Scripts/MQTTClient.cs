using MQTTnet.Client;
using MQTTnet;
using UnityEngine;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mapbox.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using System;

public class MQTTClient : MonoBehaviour
{
    private IMqttClient mqttClient;
    private GameManager gameManager;
    private Dictionary<string, Dictionary<string, int>> clients = new Dictionary<string, Dictionary<string, int>>();
    private int carID = 0;

    private void Start()
    {
        string address = PlayerPrefs.GetString("address", "tcp://127.0.0.1:3030");
        string[] addressPort = address.Remove(0, 6).Split(':');
        gameManager = FindObjectOfType<GameManager>();

        var options = new MqttClientOptionsBuilder().WithClientId(System.Environment.MachineName).WithTcpServer(addressPort[0], int.Parse(addressPort[1])).Build();
        mqttClient = new MqttFactory().CreateMqttClient();

        mqttClient.ConnectedAsync += async (MqttClientConnectedEventArgs e) =>
        {
            var subOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter("CONNECTED")
                .WithTopicFilter("DISCONNECTED")
                .WithTopicFilter("VehicleConnected")
                .WithTopicFilter("VehicleDisconnected")
                .WithTopicFilter("VehicleLocation")
                .WithTopicFilter("VehicleSpeed")
                .WithTopicFilter("VehicleTemperature")
                .WithTopicFilter("VehicleRPM")
                .Build();
            await mqttClient.SubscribeAsync(subOptions);
        };

        mqttClient.ApplicationMessageReceivedAsync += (MqttApplicationMessageReceivedEventArgs e) =>
        {
            string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            string[] payload = message.Split('$');

            switch (e.ApplicationMessage.Topic)
            {
                case "CONNECTED":
                    AddClient(e.ClientId);
                    break;
                case "DISCONNECTED":
                    RemoveClient(e.ClientId);
                    break;
                case "VehicleConnected":
                    AddCar(e.ClientId, payload[0]);
                    break;
                case "VehicleDisconnected":
                    RemoveClientCar(e.ClientId, payload[0]);
                    break;
                case "VehicleLocation":
                    {
                        int index = GetCarIndex(e.ClientId, payload[0]);
                        Vector2d pos = new Vector2d(double.Parse(payload[2]), double.Parse(payload[3]));
                        gameManager.UpdateLocation(index, pos);
                        break;
                    }
                case "VehicleSpeed":
                    {
                        int index = GetCarIndex(e.ClientId, payload[0]);
                        float speed = float.Parse(payload[2]);
                        gameManager.UpdateSpeed(index, speed);
                        break;
                    }
                case "VehicleTemperature":
                    {
                        int index = GetCarIndex(e.ClientId, payload[0]);
                        float temperature = float.Parse(payload[2]);
                        gameManager.UpdateTemperature(index, temperature);
                        break;
                    }
                case "VehicleRPM":
                    {
                        int index = GetCarIndex(e.ClientId, payload[0]);
                        float rpm = float.Parse(payload[2]);
                        gameManager.UpdateRPM(index, rpm);
                        break;
                    }
                default:
                    break;
            }

            return Task.CompletedTask;
        };
        try
        {
            mqttClient.ConnectAsync(options, CancellationToken.None).Wait();
        }
        catch (Exception)
        {
            SceneManager.LoadScene(0);
        }
    }

    private void OnDestroy()
    {
        if(mqttClient != null) mqttClient.DisconnectAsync().Wait();
    }

    private void RemoveClient(string clientName)
    {
        foreach(var (name, index) in clients.Select(x => (x.Key, x.Value)))
        {
            if(name == clientName)
            {
                foreach(int id in clients[clientName].Values)
                {
                    gameManager.RemoveVehicle(id);
                }
                clients.Remove(name);
            }
        }
    }
    private void RemoveClientCar(string clientName, string vehicleName)
    {
        foreach (var (name, index) in clients.Select(x => (x.Key, x.Value)))
        {
            if (name == clientName)
            {
                foreach (var(vehicle, id) in clients[clientName].Select(x => (x.Key, x.Value)))
                {
                    if(vehicle == vehicleName)
                    {
                        gameManager.RemoveVehicle(id);
                    }
                }
            }
        }
    }

    private void AddClient(string clientName)
    {
        if (!clients.ContainsKey(clientName))
        {
            clients.Add(clientName, new Dictionary<string, int>());
        }
    }

    private void AddCar(string clientName, string vehicleName)
    {
        Dictionary<string, int> cars;
        if (!clients.ContainsKey(clientName))
        {
            cars = new Dictionary<string, int>();
            clients.Add(clientName, cars);
        }
        else cars = clients[clientName];
        if (cars.ContainsKey(vehicleName)) return;
        cars.Add(vehicleName, carID);
        gameManager.AddVehicle(carID, vehicleName);
        carID++;
    }

    private int GetCarIndex(string clientName, string vehicleName)
    {
        return clients[clientName][vehicleName];
    }
}

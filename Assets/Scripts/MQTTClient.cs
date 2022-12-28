using MQTTnet.Client;
using MQTTnet;
using UnityEngine;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mapbox.Utils;

public class MQTTClient : MonoBehaviour
{
    private IMqttClient mqttClient;
    private GameManager gameManager;
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
            Vector2d pos = new Vector2d(double.Parse(payload[1]), double.Parse(payload[2]));

            gameManager.MoveCar(pos, 0);

            return Task.CompletedTask;
        };

        mqttClient.ConnectAsync(options, CancellationToken.None).Wait();
    }

    private void OnDestroy()
    {
        mqttClient.DisconnectAsync().Wait();
    }
}

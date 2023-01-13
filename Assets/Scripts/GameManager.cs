using Mapbox.Json;
using Mapbox.Json.Linq;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.VectorTile.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameManager;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class Car
    {
        public int ID;
        public string name;
        public Vector2d location = new Vector2d();
        public float speed = 0.0f;
        public float temperature = 0.0f;
        public float rpm = 0.0f;
        public GameObject obj;
    }

    public Button exitButton;

    public AbstractMap map;
    public InfoCanvasManager infoCanvas;
    public Transform mainCamera;

    private List<Car> cars = new List<Car>();
    public Transform carsTransform;

    public GameObject itemPrefeb;
    public GameObject carPrefab;

    private Car focusedCar;

    private void Awake()
    {
        exitButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(0);
        });
    }
    void Start()
    {
        infoCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        lock (newCarsLock)
        {
            while(newCars.Count > 0)
            {
                var (id, name) = newCars.Dequeue();
                Car car = new Car
                {
                    ID = id,
                    name = name,
                    obj = Instantiate(carPrefab, carsTransform)
                };

                car.obj.GetComponent<CarManager>().ID = car.ID;
                car.obj.GetComponent<CarManager>().name = car.name;
                cars.Add(car);
                positions.Add(car.location);
                speeds.Add(car.speed);
                temperatures.Add(car.temperature);
                rpms.Add(car.rpm);
            }
        }

        lock (removeCarsLock)
        {
            while (removeCars.Count > 0)
            {
                int ID = removeCars.Dequeue();
                if (focusedCar != null && ID == focusedCar.ID)
                {
                    StopAllCoroutines();
                    focusedCar = null;
                    infoCanvas.gameObject.SetActive(false);
                }
                for (int i =0; i < cars.Count; i++)
                {
                    if (cars[i].ID == ID)
                    {
                        cars[i].obj.Destroy();
                        cars.RemoveAt(i);
                        positions.RemoveAt(i);
                        speeds.RemoveAt(i);
                        temperatures.RemoveAt(i);
                    }
                }
            }
        }

        for (int i = 0; i < cars.Count; i++)
        {
            cars[i].location = positions[i];
            cars[i].speed = speeds[i];
            cars[i].temperature = temperatures[i];
            cars[i].rpm = rpms[i];
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopAllCoroutines();
            focusedCar = null;
            infoCanvas.gameObject.SetActive(false);
        }

        float zoom = Mathf.Pow(map.Zoom / 15f, 10);
        Vector3 scale = new Vector3(zoom, zoom, zoom);

        Vector3 nextCarPosition;
        foreach(Car car in cars)
        {
            nextCarPosition = map.GeoToWorldPosition(car.location);

            car.obj.transform.LookAt(nextCarPosition, Vector3.up);
            car.obj.transform.position = nextCarPosition;

            car.obj.transform.localScale = scale;

            car.obj.GetComponent<MeshRenderer>().material.color = new Color(car.temperature / 255f, 1, 1); //TODO CHANGE
        }

        if (focusedCar != null && infoCanvas.isActiveAndEnabled)
        {
            infoCanvas.speedText.text = $"{focusedCar.speed} km/h";
            infoCanvas.temperatureText.text = $"{focusedCar.temperature} °C";
            infoCanvas.rpmText.text = $"{focusedCar.rpm} rpm";

            mainCamera.transform.rotation = Quaternion.Euler(new Vector3(45, 0, 0));
            mainCamera.transform.position = new Vector3(focusedCar.obj.transform.position.x, mainCamera.transform.position.y, focusedCar.obj.transform.position.z - 100f);
        }
    }
    IEnumerator GetLastData(string vehicleName)
    {
        while (true)
        {
            UnityWebRequest uwr = UnityWebRequest.Get($"http://localhost:5000/api/vehicle/speed/{vehicleName}");
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.ConnectionError)
            {
                foreach (Transform o in infoCanvas.scrollView.transform) {
                    GameObject.Destroy(o.gameObject);
                }

                try
                {
                    int i = 0;
                    JObject parsedJson = JObject.Parse(uwr.downloadHandler.text);
                    foreach (var key in parsedJson[vehicleName]["VehicleSpeed"])
                    {
                        float speed = float.Parse((string)key.First.First);
                        DateTime time = DateTime.Parse(key.ToString().Substring(6, 24));
                        GameObject obj = Instantiate(itemPrefeb, infoCanvas.scrollView.transform);
                        var itemManager = obj.GetComponent<ItemManager>();
                        itemManager.valueText.text = "Speed: " + speed.ToString("0.00") + " km/h";
                        itemManager.timeText.text = "Time: " + time.ToShortTimeString();
                        obj.transform.Translate(new Vector3(0, -i * 40, 0));
                        i++;
                    }
                }
                catch (Exception) {}
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void SetCarFocus(int id, string name)
    {
        if(focusedCar != null && focusedCar.ID == id)
        {
            focusedCar = null;
            infoCanvas.gameObject.SetActive(false);
            return;
        }

        foreach(Car car in cars){
            if(car.ID == id)
            {
                focusedCar = car;
            }
        }

        foreach (Transform o in infoCanvas.scrollView.transform)
        {
            GameObject.Destroy(o.gameObject);
        }

        infoCanvas.vehicleNameText.text = name;
        infoCanvas.gameObject.SetActive(true);

        StartCoroutine(GetLastData(name));
    }

    public void UpdateLocation(int ID, Vector2d location)
    {
        for(int i = 0; i < cars.Count; i++)
        {
            if (cars[i].ID == ID) positions[i] = location;
        }
    }

    public void UpdateSpeed(int ID, float speed)
    {
        for (int i = 0; i < cars.Count; i++)
        {
            if (cars[i].ID == ID) speeds[i] = speed;
        }
    }

    public void UpdateTemperature(int ID, float temperature)
    {
        for (int i = 0; i < cars.Count; i++)
        {
            if (cars[i].ID == ID) temperatures[i] = temperature;
        }
    }

    public void UpdateRPM(int ID, float rpm)
    {
        for (int i = 0; i < cars.Count; i++)
        {
            if (cars[i].ID == ID) rpms[i] = rpm;
        }
    }

    public void AddVehicle(int ID, string name)
    {
        lock (newCars)
        {
            newCars.Enqueue((ID, name));
        }
    }

    public void RemoveVehicle(int ID)
    {
        lock (removeCarsLock)
        {
            removeCars.Enqueue(ID);
        }
    }

    private List<Vector2d> positions = new List<Vector2d>(200);
    private List<float> speeds = new List<float>(200);
    private List<float> temperatures = new List<float>(200);
    private List<float> rpms = new List<float>(200);
    private Queue<(int, string)> newCars = new Queue<(int,string)>();
    private readonly object newCarsLock = new object();
    private Queue<int> removeCars = new Queue<int>();
    private readonly object removeCarsLock = new object();
}
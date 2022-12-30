using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.VectorTile.Geometry;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class Car
    {
        public int ID;
        public Vector2d location = new Vector2d();
        public float speed = 0.0f;
        public float motorTemperature = 0.0f;
        public GameObject obj;
    }

    public AbstractMap map;
    public InfoCanvasManager infoCanvas;
    public Transform mainCamera;

    private List<Car> cars = new List<Car>();
    public Transform carsTransform;
    public GameObject carPrefab;

    private Car focusedCar;

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
                Car car = new Car
                {
                    ID = newCars.Dequeue(),
                    obj = Instantiate(carPrefab, carsTransform)
                };

                cars.Add(car);
                positions.Add(car.location);
            }
        }

        for (int i = 0; i < cars.Count; i++)
        {
            cars[i].location = positions[i];
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            focusedCar = null;
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

            car.obj.GetComponent<MeshRenderer>().material.color = new Color(car.motorTemperature / 255f, 1, 1); //TODO CHANGE
        }

        if (focusedCar != null && infoCanvas.isActiveAndEnabled)
        {
            infoCanvas.speedText.text = "Speed: " + focusedCar.speed.ToString();
            infoCanvas.motorTemperatureText.text = "Motor Temperature: " + focusedCar.motorTemperature.ToString();

            mainCamera.transform.rotation = Quaternion.Euler(new Vector3(45, 0, 0));
            mainCamera.transform.position = new Vector3(focusedCar.obj.transform.position.x, mainCamera.transform.position.y, focusedCar.obj.transform.position.z - 100f);
        }

    }

    public void SetCarFocus(int id)
    {
        if(focusedCar != null && focusedCar.ID == id)
        {
            focusedCar = null;
            infoCanvas.gameObject.SetActive(false);
            return;
        }

        focusedCar = cars[id];

        infoCanvas.speedText.text = "Speed: " + focusedCar.speed.ToString();
        infoCanvas.motorTemperatureText.text = "Motor Temperature: " + focusedCar.motorTemperature.ToString();

        mainCamera.transform.rotation = Quaternion.Euler(new Vector3(45,0,0));
        mainCamera.transform.position = new Vector3(focusedCar.obj.transform.position.x, mainCamera.transform.position.y, focusedCar.obj.transform.position.z - 100f);

        infoCanvas.gameObject.SetActive(true);
    }

    public void UpdateVehicle(int ID, Vector2d location)
    {
        positions[ID] = location;
    }

    public void AddVehicle(int ID)
    {
        lock (newCars)
        {
            newCars.Enqueue(ID);
        }
    }

    private List<Vector2d> positions = new List<Vector2d>(200);
    private Queue<int> newCars = new Queue<int>();
    private readonly object newCarsLock = new object();
}
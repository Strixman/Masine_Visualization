using Mapbox.Unity.Map;
using Mapbox.Utils;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class Car
    {
        public int ID;
        public Vector2d location;
        public float speed;
        public float motorTemperature;
        public GameObject obj;
    }

    public AbstractMap map;
    public InfoCanvasManager infoCanvas;
    public Transform mainCamera;

    public List<Car> cars = new List<Car>();
    public Transform carsTransform;
    public GameObject carPrefab;

    private Car focusedCar;
    void Start()
    {
        infoCanvas.gameObject.SetActive(false);

        foreach (Car car in cars)
        {
            car.obj = Instantiate(carPrefab, carsTransform);
            car.obj.GetComponent<CarManager>().ID = car.ID;
        }
    }

    void Update()
    {
        lock (positionLock)
        {
            while(positions.Count > 0)
            {
                cars[0].location = positions.Dequeue();
                break;
            }
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

    public void MoveCar(Vector2d newPos, int id)
    {
        lock (positionLock)
        {
            positions.Enqueue(newPos);
        }
        //Debug.Log(newPos);
        //cars[id].location = newPos;
        //Debug.Log(map.GeoToWorldPosition(cars[id].location));

    }

    private Queue<Vector2d> positions = new Queue<Vector2d>();
    private readonly object positionLock = new object();
}

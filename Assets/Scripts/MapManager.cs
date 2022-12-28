using Mapbox.Unity.Map;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public AbstractMap map;

    void Update()
    {
        if (Input.GetAxisRaw("Mouse ScrollWheel") != 0)
        {
            if(map.Zoom + Input.GetAxis("Mouse ScrollWheel") < 15f && map.Zoom + Input.GetAxis("Mouse ScrollWheel") > 12f)
            {
                map.SetZoom(map.Zoom + Input.GetAxis("Mouse ScrollWheel"));
                map.UpdateMap();
            }
        }
    }
}

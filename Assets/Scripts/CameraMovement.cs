using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed = 10f;
    public float rotationSpeed = 1f;

    private float y;
    void Start()
    {
        y = transform.position.y;
    }

    [System.Obsolete]
    void Update()
    {
        Vector3 pos = Vector3.zero;
        bool moved = false;
        if (Input.GetKey(KeyCode.A))
        {
            pos.x -= 1;
            moved = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            pos.x += 1;
            moved = true;
        }
        if (Input.GetKey(KeyCode.W))
        {
            pos.y += 1;
            moved = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            pos.y -= 1;
            moved = true;
        }

        if (Input.GetMouseButton(0))
        {
            transform.RotateAroundLocal(Vector3.up, Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed);
        }

        if (moved)
        {
            transform.Translate(pos * speed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }
    }
}

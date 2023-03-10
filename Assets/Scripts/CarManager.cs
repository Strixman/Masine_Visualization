using UnityEngine;

public class CarManager : MonoBehaviour
{
    public string name;
    public int ID;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gameManager.SetCarFocus(ID, name);
        }
    }
}

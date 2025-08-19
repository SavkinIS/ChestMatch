using UnityEngine;

public class Bootstrap : MonoBehaviour
{
 
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            Debug.Log(hit.collider != null
                ? $"Попал по: {hit.collider.gameObject.name}"
                : "Raycast не попал");
        }
    }
}

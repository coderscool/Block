using UnityEngine;

public class ShapeDrag : MonoBehaviour
{
    Vector3 offset;
    Collider2D myCollider;
    Rigidbody2D rb;
    public string destinationTag = "Square";

    void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    Vector3 MouseWorldPosition()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouse);
    }

    void OnMouseDown()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        offset = transform.position - MouseWorldPosition();
    }

    void OnMouseDrag()
    {
        Vector2 target = MouseWorldPosition() + offset;
        rb.MovePosition(target); 
    }

    void OnMouseUp()
    {
        myCollider.enabled = false;
        Vector2 mousePos = MouseWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if (hit != null && hit.CompareTag(destinationTag))
            transform.position = hit.transform.position + new Vector3(0, 0, -0.01f);
        myCollider.enabled = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}
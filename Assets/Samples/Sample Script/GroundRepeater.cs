using UnityEngine;

public class GroundRepeater : MonoBehaviour
{
    public float checkOffset = 20f; // distance from camera to reset
    private float groundWidth;

    void Start()
    {
        groundWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        if (transform.position.x + groundWidth < Camera.main.transform.position.x - checkOffset)
        {
            transform.position += new Vector3(groundWidth * 2, 0, 0);
        }
    }
}

using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private float startPos;
    public GameObject cam;
    public float parallaxFactor = 0.5f;

    void Start()
    {
        startPos = transform.position.x;
    }

    void LateUpdate()
    {
        float distance = cam.transform.position.x * parallaxFactor;
        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);
    }
}

using UnityEngine;

public class FreeRoamCam : MonoBehaviour
{
    public float rotationMultiplier = 0.25f;

    // Speed in m/s
    public float movementMultiplier = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0.0f, rotationMultiplier * Input.GetAxis("Mouse X"), 0.0f, Space.World);
        transform.Rotate(-rotationMultiplier * Input.GetAxis("Mouse Y"), 0.0f, 0.0f, Space.Self);
        transform.Translate(0.0f, 0.0f, movementMultiplier* Input.GetAxis("Vertical"), Space.Self);
        transform.Translate(movementMultiplier * Input.GetAxis("Horizontal"), 0.0f, 0.0f, Space.Self);
    }
}

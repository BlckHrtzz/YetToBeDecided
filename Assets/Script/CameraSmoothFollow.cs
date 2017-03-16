using UnityEngine;
using System.Collections;

public class CameraSmoothFollow : MonoBehaviour
{
    public Transform lookAt;
    public bool smoothCameraEnable = true;
    public float smoothSpeed = 0.0125f;
    Vector3 offset = new Vector3(0, 0, -13);

    void LateUpdate()
    {
        Vector3 desiredPosition = lookAt.transform.position + offset;
        if (smoothCameraEnable)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        }
        else
        {
            transform.position = lookAt.transform.position + offset;
        }
    }
}

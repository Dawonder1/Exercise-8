using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
    public Transform lookat; //out pengu, the object we are looking at
    public Vector3 offset = new Vector3 (0f, 1.5f, -1f);
    public Vector3 rotation = new Vector3(35, 0, 0);
    public bool IsMoving { set; get; }
    // Start is called before the first frame update
    void Start()
    {
        //transform.position = lookat.position + offset;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!IsMoving)
            return;

        Vector3 desiredPosition = lookat.position + offset;
        desiredPosition.x = 0;
        transform.position = Vector3.Lerp(transform.position, desiredPosition,0.1f);
        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.Euler(rotation), 0.1f);
    }
}

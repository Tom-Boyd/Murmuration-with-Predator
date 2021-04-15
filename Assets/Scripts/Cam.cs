using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    public static bool isEnabled = true;

    private Transform starling;
    private Transform predator;
    private Transform target;
    private Vector3 speed = new Vector3(-2.4f, 5.0f, 2.0f);
    private Vector3 rotation;
    private float distance = 2.0f;
    private readonly float keyFactor = 1.0f;
    private readonly float minDistance = 0.2f;
    private readonly float xMinLimit = -90;
    private readonly float xMaxLimit = 90;
    private bool rotateWithTarget = false;
    private bool isAttached = true;
    private Vector3 position = Vector3.zero;

    private Flock flock;

    public void Initialise(Flock flock, Transform starling, Transform target, Transform predator)
    {
        this.flock = flock;
        this.starling = starling;
        this.target = target;
        this.predator = predator;
    }

    void Start()
    {
        rotation = transform.eulerAngles;

        if (target)
        {
            distance = (transform.position - target.transform.position).magnitude;
            position = target.transform.position;
            isAttached = true;
        }

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            isAttached = false;

        if (target && isAttached)
            position = target.transform.position;

        if (isEnabled)
        {
            //Rotate Camera
            if (!Input.GetMouseButton(1))
            {
                rotation.x += Input.GetAxis("Mouse Y") * speed.x;
                rotation.y += Input.GetAxis("Mouse X") * speed.y;
                if (rotation.x < -180)
                    rotation.x += 360;
                if (rotation.x > 180)
                    rotation.x -= 360;
                rotation.x = Mathf.Clamp(rotation.x, xMinLimit, xMaxLimit);
            }

            //Zoom in and out with scrollwheel or arrow keys
            var distRaw = -Input.GetAxis("Mouse ScrollWheel") * speed.z;
            if (Input.GetKey(KeyCode.UpArrow))
                distRaw -= speed.z * keyFactor * Time.deltaTime;
            if (Input.GetKey(KeyCode.DownArrow))
                distRaw += speed.z * keyFactor * Time.deltaTime;
            distance = Mathf.Max(distance + distRaw, minDistance);
        }

        //Rotate with target
        if (target && isAttached && rotateWithTarget)
        {
            rotation = target.rotation.eulerAngles;
            rotation.z = 0;
            rotation.x += 5;
        }

        var quatRot = Quaternion.Euler(rotation);

        if (isEnabled)
        {
            var shift = Vector3.zero;

            //Attach to target
            if (Input.GetKeyDown("space"))
                isAttached = true;

            //Toggle targets between predator and starling
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (target == predator)
                {
                    target = starling;
                }
                else
                {
                    target = predator;
                }
            }

            //Follow new random starling
            if (Input.GetKey(KeyCode.G))
            {
                starling = flock.GetRandomStarling();
                target = starling;
            }

            //Toggle to rotate with target
            if (Input.GetKeyDown(KeyCode.T))
                rotateWithTarget = !rotateWithTarget;

            //Movement Keys
            if (Input.GetKey(KeyCode.W))
                shift.z += speed.z * keyFactor * Time.deltaTime;
            if (Input.GetKey(KeyCode.S))
                shift.z -= speed.z * keyFactor * Time.deltaTime;
            if (Input.GetKey(KeyCode.A))
                shift.x += speed.x * keyFactor * Time.deltaTime;
            if (Input.GetKey(KeyCode.D))
                shift.x -= speed.x * keyFactor * Time.deltaTime;
            if (Input.GetKey(KeyCode.Q))
                shift.y -= speed.y * keyFactor * Time.deltaTime;
            if (Input.GetKey(KeyCode.E))
                shift.y += speed.y * keyFactor * Time.deltaTime;
            if (shift != Vector3.zero)
            {
                position += quatRot * shift;
                isAttached = false;
            }
        }

        transform.rotation = quatRot;
        transform.position = quatRot * new Vector3(0.0f, 0.0f, -distance) + position;
    }

    private bool IsAttached()
    {
        return target && isAttached;
    }
}

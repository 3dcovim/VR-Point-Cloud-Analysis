using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonMovement : MonoBehaviour
{
    private GameObject Target;

    void Awake()
    {
        Target = GameObject.FindGameObjectWithTag("MainCamera");
    }

    private void Update()
    {
        transform.LookAt(transform.position + Target.transform.rotation * Vector3.back, Target.transform.rotation * Vector3.up);
        transform.Rotate(0, 180, 0);
    }
}

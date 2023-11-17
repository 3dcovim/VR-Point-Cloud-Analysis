using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasMovement : MonoBehaviour
{
    private GameObject Target;
    private Canvas Canvas;

    void Awake()
    {
        Target = GameObject.FindGameObjectWithTag("MainCamera");
        Canvas = this.GetComponent<Canvas>();

        Canvas.worldCamera = Target.GetComponent<Camera>();
    }

    private void Update()
    {
        transform.LookAt(transform.position + Target.transform.rotation * Vector3.back, Target.transform.rotation * Vector3.up);
        transform.Rotate(0, 180, 0);
    }
}

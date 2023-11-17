using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudSlice : MonoBehaviour
{
    public int[] PointsPerSlice;
    public Vector3 Centroid = new Vector3(0, 0, 0);
    public Vector3 Size = new Vector3(0, 0, 0);
    public Vector3[] PCSliceBoundingBox = new Vector3[8];    //Column1: Max, Column2: Min // File1: x, File2: y, File3: z

}

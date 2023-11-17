using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudSliceClass
{
    public int[] PointsPerSlice;
    public Vector3 Centroid = new Vector3(0, 0, 0);
    public Vector3 Size = new Vector3(0, 0, 0);
    public Vector3[] PCSliceBoundingBox = new Vector3[8];    //Column1: Max, Column2: Min // File1: x, File2: y, File3: z
    public bool isBeingSeen = false;
    //[SerializeField] GameObject SliceGO;

    public PointCloudSliceClass(Vector3 centroid, Vector3 size, Vector3[] pCSliceBoundingBox)
    {
        PointsPerSlice = null;
        Centroid = centroid;
        Size = size;
        PCSliceBoundingBox = pCSliceBoundingBox;    //Column1: Max, Column2: Min // File1: x, File2: y, File3: z

        /*SliceGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        SliceGO.transform.position = Centroid;
        SliceGO.transform.localScale = new Vector3(2, 2, 2);*/
    }
    
    public PointCloudSliceClass(int[] pointsPerSlice, Vector3 centroid, Vector3 size, Vector3[] pCSliceBoundingBox)
    {
        PointsPerSlice = pointsPerSlice;
        Centroid = centroid;
        Size = size;
        PCSliceBoundingBox = pCSliceBoundingBox;    //Column1: Max, Column2: Min // File1: x, File2: y, File3: z

        /*SliceGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        SliceGO.transform.position = Centroid;
        SliceGO.transform.localScale = new Vector3(10, 10, 10);*/
    }

    public void Unseen()
    {
        //this.SliceGO.GetComponent<MeshRenderer>().enabled = false;
    }

    public void Seen()
    {
        //this.SliceGO.GetComponent<MeshRenderer>().enabled = true;
    }
}

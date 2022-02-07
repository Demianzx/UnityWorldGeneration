using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PerlinGrapher : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float heightScale = 2f; 
    [Range(0, 1)]
    public float scale = 0.5f;
    public int octaves = 1;
    public float heightOffset = 1f;
    [Range(0, 1)]
    public float probability = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
        Graph();
    }
    void Graph()
    {
        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 100;
        int z = 11;
        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        for (int x = 0; x < lineRenderer.positionCount; x++)
        {
            float y = MeshUtils.fBM(x,z, octaves, scale, heightScale, heightOffset) + heightOffset;
           positions[x] = new Vector3(x, y , z);
        }
        lineRenderer.SetPositions(positions);
    }

    void OnValidate() {
        Graph();   
    }
}

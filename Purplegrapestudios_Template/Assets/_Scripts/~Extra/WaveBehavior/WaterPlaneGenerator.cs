using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPlaneGenerator : MonoBehaviour {

    public float size = 1;
    public int gridSize = 16;

    private MeshFilter filter;
    private MeshCollider meshcollider;

	// Use this for initialization
	void Start () {
        filter = GetComponent<MeshFilter>();
        filter.mesh = GenerateMesh();
        //meshcollider = GetComponent<MeshCollider>();
        //meshcollider.sharedMesh = filter.mesh;

    }

    private Mesh GenerateMesh()
    {
        Mesh m = new Mesh();

        var vertices = new List<Vector3>();//Stores vert x,y,z
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();//Just stores 2 values (x,z)

        for (int x = 0; x < gridSize + 1; x++)
        {
            for (int y = 0; y < gridSize + 1; y++)
            {
                vertices.Add(new Vector3(-size * 0.5f + size * (x / ((float)gridSize)), 0, -size * 0.5f + size * (y / ((float)gridSize))));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(x / (float)gridSize, y / (float)gridSize));
            }
        }

        var triangles = new List<int>();
        var vertCount = gridSize + 1;
        for (int i = 0; i < vertCount * vertCount - vertCount; i++)
        {
            if ((i + 1) % vertCount == 0)
            {
                continue;
            }
            triangles.AddRange(new List<int>()
            {
                //Points of the vertices of a Square (2 triangles)
                //4 vertices (0,1,2,3) [going counterclockwise for example] make a plane.
                //So vertices 0,1,3 = triangle1 and vertices 1,2,3 = triangle2 of the plane.
                i + 1 + vertCount, i + vertCount, i, i, i + 1, i + vertCount + 1
            });
        }

        m.SetVertices(vertices);
        m.SetNormals(normals);
        m.SetUVs(0, uvs);
        m.SetTriangles(triangles, 0);

        return m;
    }
    
}

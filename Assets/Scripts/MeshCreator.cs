using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshCreator : MonoBehaviour
{
    private Vector3 _slicePlaneA = new Vector3(0.5f, 1f, 0f);
    private Vector3 _slicePlaneB = new Vector3(0.5f, 1f, 1f);
    private Vector3 _slicePlaneC = new Vector3(1.5f, 0f, 0f);

    private Vector3[] _slicePlane2 = new Vector3[]
    {
        new Vector3(2f, 0.9f, 0.5f),
        new Vector3(2.6f, 0.2f, 0.1f),
        new Vector3(2.9f, 0.1f, 0.9f)
    };

    private Vector3[] _slicePlane3 = new Vector3[]
    {
        new Vector3(1f, 1f, 0f),
        new Vector3(2f, 1f, 0f),
        new Vector3(0.5f, 0f, 1f)
    };

    void Start()
    {
        // Create box
        Mesh mesh = CreateBoxMesh(3, 1, 1);

        // Cut box
        Plane cutPlane = new Plane(_slicePlaneA, _slicePlaneB, _slicePlaneC);
        mesh = MeshCutter.CutMesh(mesh, cutPlane);

        // cut again!
        Plane cutPlane2 = new Plane(_slicePlane2[0], _slicePlane2[1], _slicePlane2[2]);
        mesh = MeshCutter.CutMesh(mesh, cutPlane2);

        // cut again!!
        Plane cutPlane3 = new Plane(_slicePlane3[0], _slicePlane3[1], _slicePlane3[2]);
        //mesh = MeshCutter.CutMesh(mesh, cutPlane3);

        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void Update()
    {
        Debug.DrawLine(_slicePlaneA, _slicePlaneB, Color.red);
        Debug.DrawLine(_slicePlaneB, _slicePlaneC, Color.red);
        Debug.DrawLine(_slicePlaneC, _slicePlaneA, Color.red);

        Debug.DrawLine(_slicePlane2[0], _slicePlane2[1], Color.red);
        Debug.DrawLine(_slicePlane2[1], _slicePlane2[2], Color.red);
        Debug.DrawLine(_slicePlane2[2], _slicePlane2[0], Color.red);

        Debug.DrawLine(_slicePlane3[0], _slicePlane3[1], Color.red);
        Debug.DrawLine(_slicePlane3[1], _slicePlane3[2], Color.red);
        Debug.DrawLine(_slicePlane3[2], _slicePlane3[0], Color.red);
    }

    private static Mesh CreateBoxMesh(int width, int height, int depth)
    {
        Mesh mesh = new Mesh();

        KeyValuePair<int, Vector3> a = new KeyValuePair<int, Vector3>(0, new Vector3(0, 0, 0));
        KeyValuePair<int, Vector3> b = new KeyValuePair<int, Vector3>(1, new Vector3(0, 0, depth));
        KeyValuePair<int, Vector3> c = new KeyValuePair<int, Vector3>(2, new Vector3(0, height, 0));
        KeyValuePair<int, Vector3> d = new KeyValuePair<int, Vector3>(3, new Vector3(0, height, depth));
        KeyValuePair<int, Vector3> e = new KeyValuePair<int, Vector3>(4, new Vector3(width, 0, 0));
        KeyValuePair<int, Vector3> f = new KeyValuePair<int, Vector3>(5, new Vector3(width, 0, depth));
        KeyValuePair<int, Vector3> g = new KeyValuePair<int, Vector3>(6, new Vector3(width, height, 0));
        KeyValuePair<int, Vector3> h = new KeyValuePair<int, Vector3>(7, new Vector3(width, height, depth));

        Vector3[] newVertices = new Vector3[]
                { a.Value, b.Value, c.Value, d.Value, e.Value, f.Value, g.Value, h.Value };

        // List of faces, order of vertices in each face must form a simple polygon.
        KeyValuePair<int, Vector3>[][] faces = new KeyValuePair<int, Vector3>[][]
            {
                new KeyValuePair<int, Vector3>[] { a, b, d, c },
                new KeyValuePair<int, Vector3>[] { a, c, g, e },
                new KeyValuePair<int, Vector3>[] { c, d, h, g },
                new KeyValuePair<int, Vector3>[] { a, e, f, b },
                new KeyValuePair<int, Vector3>[] { b, f, h, d },
                new KeyValuePair<int, Vector3>[] { e, g, h, f }
            };

        IEnumerable<int> newTriangles = faces.SelectMany<KeyValuePair <int, Vector3>[], int>(GetFaceTriangles);

        Vector2[] newUV = null;

        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = newTriangles.ToArray();

        return mesh;
    }

    /// <summary>
    /// Gets triangles for mesh face. Face must be a plane!!!
    /// </summary>
    /// <param name="face">Vertices of face of mesh.</param>
    /// <returns></returns>
    private static IEnumerable<int> GetFaceTriangles(IList<KeyValuePair<int, Vector3>> face)
    {
        if (face.Count < 3)
        {
            throw new ArgumentException("face");
        }

        Transformation transformation = new Transformation(face[0].Value, face[1].Value, face[2].Value);

        // Get triangles from input indices.
        IEnumerable<int> triangles = Triangulator.Triangulate(face.Select(p => transformation.From3DTo2D(p.Value)).ToList());

        // Return triangles from mesh indices.
        return triangles.Select(i => face[i].Key);
    }
}

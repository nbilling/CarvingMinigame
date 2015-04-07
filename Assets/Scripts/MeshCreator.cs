using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshCreator : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = CreateBoxMesh(3, 1, 1);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public static Vector3[] newVertices;
    public static int[] foo;

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

        newVertices = new Vector3[]
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

        IEnumerable<int> triangles = faces.SelectMany<KeyValuePair <int, Vector3>[], int>(GetFaceTriangles);

        Vector2[] newUV = null;

        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = triangles.ToArray();

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

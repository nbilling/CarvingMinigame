using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MeshCutter
{
    public static Mesh CutMesh(Mesh mesh, Plane cutPlane)
    {
        IList<int> oldTriangles = mesh.triangles;
        if (oldTriangles.Count % 3 != 0)
        {
            throw new ArgumentException("mesh triangles");
        }

        IList<Vector3> oldVertices = mesh.vertices;

        IList<CutTriangle> cutTriangles = new List<CutTriangle>();
        for (int i = 0; i < oldTriangles.Count; i += 3)
        {
            cutTriangles.Add(new CutTriangle(
                oldVertices[oldTriangles[i]],
                oldVertices[oldTriangles[i + 1]],
                oldVertices[oldTriangles[i + 2]],
                cutPlane));
        }

        // Get reconstructed triangles and add to new triangles
        List<Vector3> newTrianglePoints = new List<Vector3>();

        newTrianglePoints.AddRange(cutTriangles.SelectMany(ct => ct.TrianglesAfterCut));

        // Triangulate cut plane and add to new triangles
        PolygonCreator3D cutPlaneCreator = new PolygonCreator3D(cutPlane);
        foreach (CutTriangle ct in cutTriangles.Where(ct => ct.WasCut && !ct.WasRemoved))
        {
            cutPlaneCreator.AddSide(ct.CutEdgePointA, ct.CutEdgePointB);
        }
        newTrianglePoints.AddRange(cutPlaneCreator.GetTriangles());

        // Copy new mesh data back into mesh object
        Dictionary<Vector3, int> vertexIndices = new Dictionary<Vector3, int>();
        int[] newTriangles = new int[newTrianglePoints.Count];

        for (int i = 0; i < newTrianglePoints.Count; i++)
        {
            int j;
            if (!vertexIndices.TryGetValue(newTrianglePoints[i], out j))
            {
                j = vertexIndices.Count;
                vertexIndices.Add(newTrianglePoints[i], j);
            }

            newTriangles[i] = j;
        }

        mesh.vertices = vertexIndices.Keys.ToArray();
        mesh.triangles = newTriangles;

        return mesh;
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PolygonCreator3D
{
    private PolygonCreator _polygonCreator = new PolygonCreator();
    private Transformation _transformation;

    public PolygonCreator3D(Plane plane)
    {
        _transformation = new Transformation(plane);
    }

    public void AddSide(Vector3 a, Vector3 b)
    {
        _polygonCreator.AddSide(_transformation.From3DTo2D(a), _transformation.From3DTo2D(b));
    }

    public IEnumerable<Vector3> GetTriangles()
    {
        List<Vector2> polygon = _polygonCreator.GetClockwisePolygon().ToList();
        return Triangulator.Triangulate(polygon.ToList())
            .Select(i => _transformation.From2DTo3D(polygon[i]));
    }
}

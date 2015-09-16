using System.Collections.Generic;
using UnityEngine;

public class PolygonCreator
{
    private IDictionary<Vector2, List<Vector2>> _vertices = new Dictionary<Vector2, List<Vector2>>();
    private Vector2? _leftmost = null;

    public void AddSide(Vector2 a, Vector2 b)
    {
        UpdateLeftmost(a);
        UpdateLeftmost(b);

        AddAdjacent(a, b);
        AddAdjacent(b, a);
    }

    public IEnumerable<Vector2> GetClockwisePolygon()
    {
        List<Vector2> polygon = new List<Vector2>();

        polygon.Add(_leftmost.Value);

        List<Vector2> adjacentVertices = _vertices[_leftmost.Value];
        Vector2 current = GetRatioBetween(_leftmost.Value, adjacentVertices[0]) > GetRatioBetween(_leftmost.Value, adjacentVertices[1]) ?
            adjacentVertices[0] :
            adjacentVertices[1];
        Vector2 last = _leftmost.Value;

        // While we haven't looped back to the start
        while (current != _leftmost)
        {
            // Add to polygon
            polygon.Add(current);

            // Pick not-visited vertex
            adjacentVertices = _vertices[current];
            Vector2 next = last == adjacentVertices[0] ? adjacentVertices[1] : adjacentVertices[0];

            // Move to next vertex
            last = current;
            current = next;
        }

        return polygon;
    }

    private void AddAdjacent(Vector2 vertex, Vector2 adjacentVertex)
    {
        if (!_vertices.ContainsKey(vertex))
        {
            _vertices[vertex] = new List<Vector2>();
        }

        _vertices[vertex].Add(adjacentVertex);
    }

    private void UpdateLeftmost(Vector2 vertex)
    {
        if (!_leftmost.HasValue ||
            vertex.x < _leftmost.Value.x)
        {
            _leftmost = vertex;
        }
    }

    private static float GetRatioBetween(Vector2 a, Vector2 b)
    {
        return (b.y - a.y) / (b.x - a.x);
    }
}

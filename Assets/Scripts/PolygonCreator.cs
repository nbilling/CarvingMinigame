using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PolygonCreator
{
    private class Vector2EqualityComparer : IEqualityComparer<Vector2>
    {
        public static Vector2EqualityComparer Instance = new Vector2EqualityComparer();

        private Vector2EqualityComparer()
        {
        }

        private static int WholeTenPowNths(float f, uint n)
        {
            return Mathf.RoundToInt(f * Mathf.Pow(10, n));
        }

        private static bool Similar(float x, float y, uint n)
        {
            float delta = Mathf.Abs(x - y);
            return delta < (Mathf.Pow(10, -n));
        }

        public bool Equals(Vector2 x, Vector2 y)
        {
            bool retval = Similar(x.x, y.x, 4) && Similar(x.y, y.y, 4);
            return retval;
        }

        public int GetHashCode(Vector2 obj)
        {
            // Hash is just sum of tenths in x component and tenths in y component.
            // TODO: better hash
            int retVal = WholeTenPowNths(obj.x, 2) + WholeTenPowNths(obj.y, 2);
            return retVal;
        }
    }

    private IDictionary<Vector2, List<Vector2>> _vertices = new Dictionary<Vector2, List<Vector2>>(Vector2EqualityComparer.Instance);
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

        //EnsureClosed();

        polygon.Add(_leftmost.Value);

        Debug.Assert(_vertices.ContainsKey(_leftmost.Value));
        List<Vector2> adjacentVertices = _vertices[_leftmost.Value];

        Debug.Assert(adjacentVertices.Count == 2);
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

    private void EnsureClosed()
    {
        // For each vertex, check if it has 2 adjacent vertices.
        // If not, then look for the closest other vertex and make them adjacent.
        // TODO: what to do if closest already has 2 adjacent vertices?
        for (int i = 0; i < _vertices.Count - 1; i++)
        {
            KeyValuePair<Vector2, List<Vector2>> currentVertex = _vertices.ElementAt(i);

            if (currentVertex.Value.Count < 2)
            {
                KeyValuePair<Vector2, List<Vector2>> closestVertex = _vertices.ElementAt(i + 1);

                for (int j = i + 2; j < _vertices.Count; j++)
                {
                    KeyValuePair<Vector2, List<Vector2>> nextVertex = _vertices.ElementAt(j);

                    if (Mathf.Abs(Vector2.Distance(currentVertex.Key, nextVertex.Key))
                        < Mathf.Abs(Vector2.Distance(currentVertex.Key, closestVertex.Key)))
                    {
                        closestVertex = nextVertex;
                    }
                }

                Debug.Assert(closestVertex.Value.Count < 2, "vertex on cut plane is becoming adjacent to more than 2 other vertices...");

                currentVertex.Value.Add(closestVertex.Key);
                closestVertex.Value.Add(currentVertex.Key);
            }
        }
    }
}

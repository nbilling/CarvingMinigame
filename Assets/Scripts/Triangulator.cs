using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Triangulator
{
    private class PolygonVertex
    {
        public PolygonVertex Prev { get; set; }
        public PolygonVertex Next { get; set; }
        public int Index { get; set; }
        public float WindingValue { get; set; }
        public bool IsReflex { get; set; }
    };

    /// <summary>
    /// Splits a polygon into triangles.
    /// </summary>
    /// <param name="polygon">
    /// Polygon to be split.
    /// </param>
    /// <returns>
    /// List of vertices in input polygon that form triangles (will either
    /// be empty or have count that is a factor of 3). Triangles will be clockwise if
    /// polygon is clockwise and counter-clockwise if polygon is counter-clockwise.
    /// </returns>
    public static IEnumerable<int> Triangulate(IList<Vector2> polygon)
    {
        int N = polygon.Count;

        // Not a polygon.
        if (N <= 2)
        {
            return new int[] { };
        }

        IList<int> triangles = new List<int>();

        // Initialize leftmost and vertices for polygon
        IList<PolygonVertex> vertices = polygon.Select(v => new PolygonVertex()).ToList();
        int iLeftMost = 0;
        for (int i = 0; i < N; i++)
        {
            int iPrev = MathUtil.Mod(i - 1, N);
            int iNext = MathUtil.Mod(i + 1, N);

            // Init polygon vertex
            vertices[i].Index = i;
            vertices[i].Prev = vertices[iPrev];
            vertices[i].Next = vertices[iNext];
            vertices[i].Prev.Index = iPrev;
            vertices[i].Next.Index = iNext;
            vertices[i].WindingValue = WindingValue(polygon, vertices[i]);
            vertices[i].IsReflex = false;

            // Update leftmost for polygon
            Vector2 p = polygon[i];
            Vector2 lm = polygon[iLeftMost];

            if (p.x < lm.x || (p.x == lm.x && p.y < lm.y))
            {
                iLeftMost = i;
            }
        }

        // Check if polygon is counter-clockwise
        bool isCcw = vertices[iLeftMost].WindingValue > 0.0f;

        // Initialize list of reflex vertices
        IList<PolygonVertex> reflexVertices = new List<PolygonVertex>();

        foreach (var vertex in vertices)
        {
            if (IsReflex(isCcw, vertex))
            {
                vertex.IsReflex = true;
                reflexVertices.Add(vertex);
            }
        }

        // Perform triangulation
        int skipped = 0; // Number of consecutive vertices skipped
        int nVertices = vertices.Count; // Number of vertices left in polygon

        PolygonVertex current = vertices[0];

        // While polygon not a triangle
        while (nVertices > 3)
        {
            PolygonVertex prev = current.Prev;
            PolygonVertex next = current.Next;

            if (IsEarTip(polygon, current, reflexVertices))
            {
                // Add this ear to list of triangles
                triangles.Add(prev.Index);
                triangles.Add(current.Index);
                triangles.Add(next.Index);

                // Remove this ear from polygon
                prev.Next = next;
                next.Prev = prev;

                // Re-calculate reflexivity of adjacent vertices
                PolygonVertex[] adjacent = { prev, next };
                foreach (var vertex in adjacent)
                {
                    if (vertex.IsReflex)
                    {
                        vertex.WindingValue = WindingValue(polygon, vertex);
                        vertex.IsReflex = IsReflex(isCcw, vertex);

                        if (!vertex.IsReflex)
                        {
                            reflexVertices.Remove(vertex);
                        }
                    }
                }

                nVertices--;
                skipped = 0;
            }
            else if (++skipped > nVertices)
            {
                // If we have gone through all remaining vertices and not found ear, then fail.
                return new int[] { };
            }

            current = next;
        }

        // Remaining polygon _is_ a triangle.
        triangles.Add(current.Prev.Index);
        triangles.Add(current.Index);
        triangles.Add(current.Next.Index);

        return triangles;
    }

    private static bool TriangleContains(Vector2 a, Vector2 b, Vector2 c, Vector2 point)
    {

        if ((point.x == a.x && point.y == a.y) ||
            (point.x == b.x && point.y == b.y) ||
            (point.x == c.x && point.y == c.y))
        {
            return false;
        }

        float A = 0.5f * (float)(-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
        float sign = A < 0.0f ? -1.0f : 1.0f;

        float s = (float)(a.y * c.x - a.x * c.y + (c.y - a.y) * point.x + (a.x - c.x) * point.y) * sign;
        float t = (float)(a.x * b.y - a.y * b.x + (a.y - b.y) * point.x + (b.x - a.x) * point.y) * sign;

        return s >= 0.0f && t >= 0.0f && (s + t) <= (2.0f * A * sign);
    }

    private static float WindingValue(IList<Vector2> polygon, PolygonVertex vertex)
    {
        Vector2 a = polygon[vertex.Prev.Index];
        Vector2 b = polygon[vertex.Index];
        Vector2 c = polygon[vertex.Next.Index];

        return (float)((b.x - a.x) * (c.y - b.y) - (c.x - b.x) * (b.y - a.y));
    }

    private static bool IsEarTip(IList<Vector2> polygon, PolygonVertex vertex, IList<PolygonVertex> reflexVertices)
    {
        if (vertex.IsReflex)
        {
            return false;
        }

        Vector2 a = polygon[vertex.Prev.Index];
        Vector2 b = polygon[vertex.Index];
        Vector2 c = polygon[vertex.Next.Index];

        foreach (var reflexVertex in reflexVertices)
        {
            int index = reflexVertex.Index;

            if (index == vertex.Prev.Index || index == vertex.Next.Index)
            {
                continue;
            }

            if (TriangleContains(a, b, c, polygon[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsReflex(bool isCcw, PolygonVertex v)
    {
        return isCcw ? v.WindingValue <= 0.0f : v.WindingValue >= 0.0f;
    }
}


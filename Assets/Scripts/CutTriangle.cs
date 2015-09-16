using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    internal class CutTrianglePoint
    {
        public Vector3 P { get; private set; }
        public bool IsRemoved { get; private set; }
        public int Order { get; set; }

        public CutTrianglePoint(Vector3 p, int order, Plane cutPlane)
        {
            P = p;
            Order = order;
            IsRemoved = !cutPlane.GetSide(p);
        }
    }

internal class CutTriangle
{
    public IEnumerable<Vector3> TrianglesAfterCut
    {
        get;
        private set;
    }

    public bool WasCut
    {
        get;
        private set;
    }

    public bool WasRemoved
    {
        get;
        private set;
    }

    private Vector3 _cutEdgePointA;
    public Vector3 CutEdgePointA
    {
        get
        {
            ThrowIfNotPartiallyRemoved();

            return _cutEdgePointA;
        }
    }

    private Vector3 _cutEdgePointB;
    public Vector3 CutEdgePointB
    {
        get
        {
            ThrowIfNotPartiallyRemoved();

            return _cutEdgePointB;
        }
    }

    private void ThrowIfNotPartiallyRemoved()
    {
        if (WasRemoved)
        {
            throw new InvalidOperationException("Triangle was entirely removed.");
        }
        if (!WasCut)
        {
            throw new InvalidOperationException("Triangle wasn't cut.");
        }
    }

    public CutTriangle(Vector3 a, Vector3 b, Vector3 c, Plane cutPlane)
    {
        IList<CutTrianglePoint> originalPoints = new List<CutTrianglePoint>
            {
                new CutTrianglePoint(a, 0, cutPlane),
                new CutTrianglePoint(b, 1, cutPlane),
                new CutTrianglePoint(c, 2, cutPlane)
            };

        // Logic is:
        // - If entirely on good side of plane then save it.
        // - If entirely on bad side of plane then save nothing.
        // - If partially inside on good side of plane then we have to break it:
        //   + If 1 vertex of triangle on bad side of plane then 2 remain. Find a vertex on each cut
        //     edge of the original triangle where the plane cut it. Add the new edges to the
        //     cut plane polygon, and then find the resulting 'broken' triangles and add them to
        //     the new triangles.
        //   + If 2 vertices of triangle on bad side of plane then 1 remains. Find a vertex on each
        //     cut edge of the original where the plane cut it. Add the new edges to the cut
        //     plane polygon, and then find the resulting 'broken' triangle and add it to the new
        //     triangles.

        int removedCount = originalPoints.Count(ctp => ctp.IsRemoved);

        if (removedCount == 0)
        {
            // Triangle intact
            TrianglesAfterCut = new Vector3[]
                {
                    // Original triangle
                    a, b, c
                };
        }
        else if (removedCount == 1)
        {
            // One vertex of triangle was cut off, two remaining
            CutTrianglePoint removedPoint = originalPoints.Single(ctp => ctp.IsRemoved);

            // Remaining point that was predecessor of removed point in original triangle
            CutTrianglePoint predecessorRemainingPoint = originalPoints.Single(ctp => ctp.Order == MathUtil.Mod(removedPoint.Order - 1, 3));

            // Remaining point that was successor of removed point in original triangle
            CutTrianglePoint successorRemainingPoint = originalPoints.Single(ctp => ctp.Order == MathUtil.Mod(removedPoint.Order + 1, 3));

            // Find where plane cut triangle
            Vector3 predecessorAdjacentNewPoint = GetPlaneLineIntersection(predecessorRemainingPoint.P, removedPoint.P, cutPlane);
            Vector3 successorAdjacentNewPoint = GetPlaneLineIntersection(successorRemainingPoint.P, removedPoint.P, cutPlane);

            // Now, resulting polygon looks like (in 2D, and the rotation is not meaningful):
            //         RP
            //         /\
            //  PANP  /__\  SANP
            //       /    \
            //      /      \
            // PRP  --------  SRP
            // Where the top has been 'cut' off, and the bottom quadrilateral is the result.

            // Cut quadrilateral into two triangles: (PANP, SANP, PRP) and (SANP, SRP, PRP).
            // Importantly, these triangles are guaranteed to have the same direction (CW vs. CCW) as the original triangle.

            TrianglesAfterCut = new Vector3[]
                {
                    // Top-left half of quad from diagram
                    predecessorAdjacentNewPoint, successorAdjacentNewPoint, predecessorRemainingPoint.P,
                    // Bottom-right half of quad from diagram
                    successorAdjacentNewPoint, successorRemainingPoint.P, predecessorRemainingPoint.P
                };

            // TODO: Is there any way to pick a 'correct' order from here? Probably, given that we know the plane normal.
            _cutEdgePointA = predecessorAdjacentNewPoint;
            _cutEdgePointB = successorAdjacentNewPoint;

            WasCut = true;
        }
        else if (removedCount == 2)
        {
            // Two vertices of triangle were cut off, one remaining
            CutTrianglePoint remainingPoint = originalPoints.Single(ctp => !ctp.IsRemoved);

            // Removed point that was predecessor to remaining point
            CutTrianglePoint predecessorRemovedPoint = originalPoints.Single(ctp => ctp.Order == (MathUtil.Mod(remainingPoint.Order - 1, 3)));

            // Removed point that was successor to remaining point
            CutTrianglePoint sucessorRemovedPoint = originalPoints.Single(ctp => ctp.Order == (MathUtil.Mod(remainingPoint.Order + 1, 3)));

            // Find where plane cut triangle
            Vector3 predecessorNewPoint = GetPlaneLineIntersection(remainingPoint.P, predecessorRemovedPoint.P, cutPlane);
            Vector3 sucessorNewPoint = GetPlaneLineIntersection(remainingPoint.P, sucessorRemovedPoint.P, cutPlane);

            // Now, the resulting polygon looks like (in 2D, and the rotation here is not meaningful):
            // SRP  --------  PRP
            // SNP  \______/  PNP
            //       \    /
            //        \  /
            //         \/
            //         RP
            // Where the top has been 'cut' off, and the bottom triangle is the result.

            // New triangle is (PNP, RP, SNP).
            // Importantly, this triangle is guaranteed to have the same direction (CW vs. CCW) as the original triangle.

            TrianglesAfterCut = new Vector3[]
                {
                    // Bottom tip of triangle in diagram.
                    predecessorNewPoint, remainingPoint.P, sucessorNewPoint
                };
            // TODO: Is there any way to pick a 'correct' order from here? Probably, given that we know the plane normal.
            _cutEdgePointA = predecessorNewPoint;
            _cutEdgePointB = sucessorNewPoint;

            WasCut = true;
        }
        else
        {
            // All vertices were removed.
            TrianglesAfterCut = new Vector3[0];
            WasRemoved = true;

            // Nothing to do.
        }

    }

    private static Vector3 GetPlaneLineIntersection(Vector3 lineStart, Vector3 lineEnd, Plane plane)
    {
        float distance;

        Vector3 lineDirection = (lineEnd - lineStart).normalized;

        plane.Raycast(new Ray(lineStart, lineDirection), out distance);

        return lineStart + (lineDirection * distance);
    }
}


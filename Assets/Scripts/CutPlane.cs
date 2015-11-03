using UnityEngine;
using System.Collections;

public class CutPlane : MonoBehaviour
{
    // 3d points for plane
    public Vector3 triA;// = new Vector3(0.5f, 1f, 0f);//(1, 2, -1);
    public Vector3 triB;// = new Vector3(0.5f, 1f, 1f);//(3, -4, 4);
    public Vector3 triC;// = new Vector3(1.5f, 0f, 0f);//(-2, -1, -3);

    public Vector2 recA2D = new Vector2(1, 1);
    public Vector2 recB2D = new Vector2(1, -1);
    public Vector2 recC2D = new Vector2(-1, -1);
    public Vector2 recD2D = new Vector2(-1, 1);

    private Plane _plane;
    private Vector3 _planeOrigin;

    private Vector3 _triMidAB;
    private Vector3 _triMidBC;
    private Vector3 _triMidCA;

    private Vector3 _recA;
    private Vector3 _recB;
    private Vector3 _recC;
    private Vector3 _recD;

    void Start()
    {
        _plane = new Plane(triA, triB, triC);
        _planeOrigin = _plane.distance * (-_plane.normal);

        Transformation transformation = new Transformation(triA, triB, triC);

        // Project triangle points to 2d
        Vector2 triA2D = transformation.From3DTo2D(triA);
        Vector2 triB2D = transformation.From3DTo2D(triB);
        Vector2 triC2D = transformation.From3DTo2D(triC);

        // Find midpoints of sides of triangle in 2d
        Vector2 triMidAB2D = (triA2D + triB2D) / 2;
        Vector2 triMidBC2D = (triB2D + triC2D) / 2;
        Vector2 triMidCA2D = (triC2D + triA2D) / 2;

        // Translate midpoints back to 3d
        _triMidAB = transformation.From2DTo3D(triMidAB2D);
        _triMidBC = transformation.From2DTo3D(triMidBC2D);
        _triMidCA = transformation.From2DTo3D(triMidCA2D);

        // Translate rectangle to 3d
        _recA = transformation.From2DTo3D(recA2D);
        _recB = transformation.From2DTo3D(recB2D);
        _recC = transformation.From2DTo3D(recC2D);
        _recD = transformation.From2DTo3D(recD2D);
    }

    void Update()
    {
        // Draw triangle for plane.
        Debug.DrawLine(triA, triB, Color.blue);
        Debug.DrawLine(triB, triC, Color.green);
        Debug.DrawLine(triC, triA, Color.yellow);

        // Draw line from 3d origin to origin of plane projection.
        Debug.DrawLine(Vector2.zero, _planeOrigin, Color.red);

        // Draw 2d points translated back to 3d.
        Debug.DrawLine(_triMidAB, _triMidBC, Color.magenta);
        Debug.DrawLine(_triMidBC, _triMidCA, Color.magenta);
        Debug.DrawLine(_triMidCA, _triMidAB, Color.magenta);

        Debug.DrawLine(_recA, _recB, Color.gray);
        Debug.DrawLine(_recB, _recC, Color.gray);
        Debug.DrawLine(_recC, _recD, Color.gray);
        Debug.DrawLine(_recD, _recA, Color.gray);
    }
}

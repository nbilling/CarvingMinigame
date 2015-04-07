﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Transformation
{
    private Vector3 _planeOrigin;
    private Vector3 _xAxis;
    private Vector3 _yAxis;

    public Transformation(Vector3 a, Vector3 b, Vector3 c)
    {
        Plane plane = new Plane(a, b, c);

        // Normal faces towards you when you can read vertices clockwise!!!
        _planeOrigin = Mathf.Abs(plane.distance) * plane.normal;

        _xAxis = GetOrthogonal(plane.normal).normalized;
        //_xAxis = (a - _planeOrigin).normalized; // TODO: use a more robust algorithm to pick this vector. This will break if triA is also the plane origin.
        _yAxis = Vector3.Cross(plane.normal, _xAxis).normalized;
    }

    public Vector2 From3DTo2D(Vector3 p)
    {
        return new Vector2(Vector3.Dot(_xAxis, p - _planeOrigin), Vector3.Dot(_yAxis, p - _planeOrigin));
    }

    public Vector3 From2DTo3D(Vector2 p)
    {
        return _planeOrigin + (p.x * _xAxis) + (p.y * _yAxis);
    }

    private Vector3 GetOrthogonal(Vector3 v)
    {
        // Find coordinate with least magnitude, set that one to zero, switch other two and negate one of them
        if (Math.Abs(v.x) <= Math.Abs(v.y) && Math.Abs(v.x) <= Math.Abs(v.z))
        {
            return new Vector3(0, -v.z, v.y);
        }
        else if (Math.Abs(v.y) <= Math.Abs(v.x) && Math.Abs(v.y) <= Math.Abs(v.z))
        {
            return new Vector3(-v.z, 0, v.x);
        }
        else
        {
            return new Vector3(-v.y, v.x, 0);
        }
    }
}
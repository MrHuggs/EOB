using System;
using System.Collections;
using System.Collections.Generic;
using Codice.CM.Common;
using UnityEngine;

public class CourtTriangulaion
{
    // Tries to find the minimum-weight triangulation via a greedy method.
    // https://en.wikipedia.org/wiki/Minimum-weight_triangulation
    //
    // This might or might not be the best method for setting up links. But
    // it's the easiest to implement.
    

    class LineRecord
    {
        public int idx0;    // Indices of the source points.
        public int idx1;

        //public Vector2 pt0;
        //public Vector2 pt1;

        public Vector2 center;  // For early out testing
        public float radius;

        public Vector2 dir;     // Normalized direction from 0-->1
        
        public LineRecord(int i0, int i1, Vector2[] points)
        {
            idx0 = i0;
            idx1 = i1;

            var delta = points[i1] - points[i0];
            radius = delta.magnitude * .5f;

            center = points[i0] + delta * .5f;
            dir = delta.normalized;

            //pt0 = points[i0];
            //pt1 = points[i1];
        }

        public bool IsSame(in LineRecord other)
        {
            if ((center - other.center).sqrMagnitude > .001)
                return false;
            if (Math.Abs(radius - other.radius) > .01)
                return false;
            if ((dir - other.dir).sqrMagnitude > .001)
            {
                if ((dir + other.dir).sqrMagnitude > .001)
                    return false;
            }

            return true;
        }

    }
    
    static bool is_it(in LineRecord t0, in LineRecord t1)
    {
        Vector2[] l0 = new Vector2[] { new Vector2(5.5f, 0), new Vector2(4, 10) };
        Vector2[] l1 = new Vector2[] { new Vector2(0, 0), new Vector2(16, 22) };
        float xoff = 41.75f;
        l0[0].x = xoff - l0[0].x;
        l0[1].x = xoff - l0[1].x;
        l1[0].x = xoff - l1[0].x;
        l1[1].x = xoff - l1[1].x;

        LineRecord local0 = new LineRecord(0, 1, l0);
        LineRecord local1 = new LineRecord(0, 1, l1);

        if ((local0.IsSame(t0) && local1.IsSame(t1)) ||
            (local0.IsSame(t1) && local1.IsSame(t0)))
        {
            return true;
        }

        return false;
    }

    static bool LineLineTest(in LineRecord l0, in LineRecord l1)
    {
        var diff = l1.center - l0.center;
        var rsum = l0.radius + l1.radius;

        if (is_it(l0, l1))
        {
            Debug.Log("Targets");
        }

        if (rsum < .001f)
        {
            Debug.LogError("Testing intersection for segments too short");
            return false;
        }
        
        // Early out for segments far away:
        float d2 = diff.sqrMagnitude;
        if (diff.sqrMagnitude > rsum * rsum)
            return false;

        if (d2 < .001f)
            return true;

        var v = l1.dir;
        var w = l0.dir;
        
        float det = (v.y * w.x - v.x * w.y);
        if (Math.Abs(det) < .01f)
        {
            return false;   // Almost parallel
        }
        
        float ood = 1.0f/det;
        float t0 = (w.y * diff.x - w.x * diff.y) * ood;
        float t1 = (v.y * diff.x - v.x * diff.y) * ood;

        var cp0 = l0.center + l0.dir * t1;
        var cp1 = l1.center + l1.dir * t0;
        var errv = cp1 - cp0;
        var err = errv.magnitude;
        if (err > .01f)
        {
            Debug.Log("Intersection test failed.");
        }
        Debug.Assert(err < .01f);

        t0 = Math.Abs(t0);
        t1 = Math.Abs(t1);
        float end_tolerance = .1f;
        if (t1 <= l0.radius - end_tolerance && t0 <= l1.radius - end_tolerance)
        {
            return true;
        }

        return false;
    }

    static LineRecord[] Analyze(in Vector2[] points)
    {
        LineRecord[] results = new LineRecord[(points.Length * (points.Length - 1)) / 2];
        int count = 0;
        for (int i = 0; i < points.Length - 1; i++)
        {
            for (int j = i + 1; j < points.Length; j++)
            {
                results[count++] = new (i, j, points);   
            }
        }
        Debug.Assert(count == results.Length);
        
        Array.Sort(results, (x, y) => x.radius.CompareTo(y.radius));
        return results;
    }
    
    static public (int, int)[] FindEdges(in Vector2[] points)
    {
        var edges = new List<(int, int)>();
        var keeps = new List<int>();

        LineRecord[] records = Analyze(points);

        for (int i = 0; i < records.Length; i++)
        {
            bool success = true;
            foreach (int lineid in keeps)
            {
                if (LineLineTest(records[lineid], records[i]))
                {
                    success = false;
                    break;
                }
            }

            if (success)
            {
                edges.Add((records[i].idx0, records[i].idx1));
                keeps.Add(i);
            }
        }

        return edges.ToArray();
    }
}

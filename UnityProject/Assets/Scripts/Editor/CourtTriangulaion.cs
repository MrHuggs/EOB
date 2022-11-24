using System;
using System.Collections;
using System.Collections.Generic;
using Codice.CM.Common;
using UnityEngine;

public class CourtTriangulaion
{
    // Construct a pleasing triangulation of the player spots.
    // The idea is to use the minimum-weight triangulation with
    // edges that are too close in direction removed.
    
    // https://en.wikipedia.org/wiki/Minimum-weight_triangulation
    //
    

    class LineRecord
    {
        public int idx0;    // Indices of the source points.
        public int idx1;

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
        }

    }
    
    static float EndTolerance = .1f;    
    static bool LineLineTest(in LineRecord l0, in LineRecord l1)
    {
        var diff = l1.center - l0.center;
        var rsum = l0.radius + l1.radius;

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
        
        if (t1 <= l0.radius - EndTolerance && t0 <= l1.radius - EndTolerance)
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

                float direction;
                if (records[lineid].idx0 == records[i].idx0 || records[lineid].idx1 == records[i].idx1)
                {
                    direction = 1;
                }
                else if (records[lineid].idx0 == records[i].idx1 || records[lineid].idx1 == records[i].idx0)
                {
                    direction = -1;
                }
                else
                {
                    continue;
                }
                
                var dp = Vector2.Dot(records[lineid].dir, records[i].dir);
                dp *= direction;
                    
                float angle_cuttoff = (float) Math.Cos(5 * Math.PI / 180);
                
                if (dp > angle_cuttoff)
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

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

class SpotUtilsWindow : EditorWindow
{
    [MenuItem("Window/Spot Utils")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SpotUtilsWindow));
    }

    string RootNode = "Hoop Center";

    void OnGUI()
    {
        // The actual window code goes here
        RootNode = EditorGUILayout.TextField("Root Node", RootNode);

        if (GUILayout.Button("Mirror"))
        {
            Mirror();
        }
        if (GUILayout.Button("Cleanup"))
        {
            Cleanup();
        }
        if (GUILayout.Button("Make Connections"))
        {
            Connections();
        }        
    }

    void Cleanup()
    {
        Debug.Log("Cleanup...");
        
        var root = GameObject.Find(RootNode);

        if (!root)
        {
            Debug.LogError($"Root node {RootNode} node found.");
            return;
        }

        for (;;)
        {
            // When we destroy a transform, the list iteration stops. So keep iterating
            // until there are no nodes destroyed.
            int clean_count = 0;
            foreach (Transform transform in root.transform)
            {
                var spot = transform.gameObject;
                if (spot.name[0] == 'R')
                {
                    DestroyImmediate(spot);
                    clean_count++;
                    continue;
                }
            
                var existing_renderer = transform.gameObject.GetComponent<LineRenderer>();
                if (existing_renderer)
                    DestroyImmediate(existing_renderer);
            }

            if (clean_count == 0)
                break;
        }
    }


    void Mirror()
    {
        Debug.Log("Mirroring...");
        var root = GameObject.Find(RootNode);

        if (!root)
        {
            Debug.LogError($"Root node {RootNode} node found.");
            return;
        }

        List<GameObject> adds = new List<GameObject>();
        foreach (Transform transform in root.transform)
        {
            var spot = transform.gameObject;

            if (spot.name[0] == 'C')
            {
                // Don't mirror nodes on the centerline
                continue;
            }
            else if (spot.name[0] == 'R')
            {
                DestroyImmediate(spot);
                continue;
            }
            else if (spot.name[0] != 'L')
            {
                Debug.Log($"Misnamed object {spot.name} found during mirror.");
            }

            adds.Add(spot);
        }
        
        foreach (var spot in adds)
        {
            GameObject mirrored = GameObject.Instantiate(spot);

            string newname = "R" + spot.name.Substring(1);
            Debug.Log(newname);
            mirrored.name = newname;
            mirrored.transform.parent = root.transform;

            var pos = spot.transform.localPosition;
            pos.y = -pos.y;
            mirrored.transform.SetLocalPositionAndRotation(pos, spot.transform.localRotation);
        }        
    }

    void Connections()
    {
        var root = GameObject.Find(RootNode);

        if (!root)
        {
            Debug.LogError($"Root node {RootNode} node found.");
            return;
        }

        List<GameObject> objects = new List<GameObject>();
        foreach (Transform transform in root.transform)
        {
            var existing_renderer = transform.gameObject.GetComponent<LineRenderer>();
            if (existing_renderer)
            {
                DestroyImmediate(existing_renderer);
            }

            objects.Add(transform.gameObject);
        }
     
        Debug.Assert(objects.Count > 0);

        GameObject[] objects_array = new GameObject[objects.Count];
        Vector2[] positions = new Vector2[objects.Count];
        int pos = 0;
        foreach (var go in objects)
        {
            objects_array[pos] = go;
            positions[pos++] = go.transform.position;
        }

        var edges = CourtTriangulaion.FindEdges(positions);


        // We can only have one 1 LineRenderer/spot. Hence we collect lines
        // that come from the came spot and create a single line rendered with multiple
        // lines.
        Array.Sort(edges, (x, y) => x.Item1.CompareTo(y.Item1));
        int cur_start = 0;
        for (int i = 1; ; i++)
        {
            if (i < edges.Length && edges[i].Item1 == edges[cur_start].Item1)
            {
                continue;
            }

            int line_count = i - cur_start;
            
            GameObject o0 = objects_array[edges[cur_start].Item1];

            LineRenderer lineRenderer = o0.AddComponent<LineRenderer>();
            
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.widthMultiplier = 0.2f;
        
            lineRenderer.positionCount = line_count * 2;
            var points = new Vector3[line_count * 2];

            for (int idx_line = 0; idx_line < line_count; idx_line++)
            {
                GameObject o1 = objects_array[edges[cur_start + idx_line].Item2];

                points[idx_line * 2] = o0.transform.position;
                points[idx_line * 2 + 1] = o1.transform.position;
            }

            lineRenderer.SetPositions(points);
            lineRenderer.startColor = Color.blue;
            lineRenderer.endColor = Color.blue;

            if (i == edges.Length)
            {
                break;
            }
            else
            {
                cur_start = i;
            }
        }
    }
    
}

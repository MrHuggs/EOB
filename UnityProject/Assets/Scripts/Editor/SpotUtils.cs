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
    }

    void Cleanup()
    {
        Debug.Log("Mirroring...");
        
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
                }
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
}

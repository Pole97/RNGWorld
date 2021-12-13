using System.Collections;
using System.Runtime;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {
    MapGenerator mapGen;

    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        if (DrawDefaultInspector()) {
            if (mapGen.autoUpdate) {
                mapGen.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate")) {
            mapGen.GenerateMap();
        }
    }



    private void OnEnable() {
        mapGen = (MapGenerator)target;
    }
}

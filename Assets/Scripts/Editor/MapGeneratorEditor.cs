using System.Collections;
using System.Runtime;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {
    MapGenerator mapGen;
    Editor noiseEditor;

    public override void OnInspectorGUI() {

        using (var check = new EditorGUI.ChangeCheckScope()) {
            base.OnInspectorGUI();
            if (check.changed) {
                mapGen.GenerateMap();
            }
        }

        DrawSettingsEditor(mapGen.noiseSettings, mapGen.OnNoiseSettingsUpdated, ref mapGen.noiseSettingsFaldout, ref noiseEditor);

        if (GUILayout.Button("Generate")) {
            mapGen.GenerateMap();
        }
    }

    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool faldout, ref Editor editor) {
        if (settings != null) {
            faldout = EditorGUILayout.InspectorTitlebar(faldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope()) {
                if (faldout) {
                    CreateCachedEditor(settings, null, ref editor);
                    editor = CreateEditor(settings);
                    editor.OnInspectorGUI();

                    if (check.changed) {
                        if (onSettingsUpdated != null) {
                            onSettingsUpdated();
                        }
                    }
                }
            }
        }

    }



    private void OnEnable() {
        mapGen = (MapGenerator)target;
    }
}

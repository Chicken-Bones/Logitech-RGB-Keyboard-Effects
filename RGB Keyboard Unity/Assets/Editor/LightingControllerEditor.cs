using CodeChicken.RGBKeyboard;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightingController))]
public class LightingControllerEditor : Editor
{
	public override void OnInspectorGUI() {
		var cont = (LightingController)target;

		cont.show_texture = EditorGUILayout.Toggle("Show Texture", cont.show_texture);
		if (cont.show_texture) {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("res"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("texture"));
		}

		EditorGUILayout.PropertyField(serializedObject.FindProperty("showKeys"));
		if (EditorApplication.isPlaying) {
			if (EditorGUILayout.Toggle("Live", cont.live) != cont.live)
				cont.ToggleLive();

			if (!cont.live) {
				EditorGUILayout.PropertyField(serializedObject.FindProperty("seed"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("simTime"));
			}

			if (GUILayout.Button("Save Effect"))
				cont.Effect.Save();
		}

		serializedObject.ApplyModifiedProperties();
	}
}

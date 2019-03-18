using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(NvrInteractiveObject)), CanEditMultipleObjects]
public class NvrInteractiveObjectEditor : Editor {

    private SerializedProperty interactionEnabled;
    private SerializedProperty distance;
    private SerializedProperty clickWithGaze;
    private SerializedProperty gazeClickTime;
    private SerializedProperty useOutline;
    private SerializedProperty outlineWidth;
    private SerializedProperty outlineGrowSpeed;
    private SerializedProperty outlineDepthOffset;
    private SerializedProperty outlineColor;
    private static GUIStyle mainTitle = null;
    //private static VRInteractiveObject t;

    private string[] tooltips = new string[17]{
        "",
        "Is it possible to interact with this object or not. May be changed at runtime.",
        "This is maximum interaction distance between the camera and this object.",
        "Enabling this will allow the gaze to perform an actual 'click' in the Event System by gazing at the object for X seconds.",
        "How long to gaze at the object before a click happens.",
        "Enable/disable an outline for the object. Will only work for gameobjects with a Mesh Renderer.",
        "The width of the outline. Too small and the outline is not visible. Too large and the outline separates from the object.",
        "How fast the outline will grow to its full width.",
        "The Depth at which the outline is drawn.",
        "Select a color for the outline.",
        "",
        "",
        "",
        "",
        "",
        "",
        ""
    };

    void OnEnable() {
        //t = target as VRInteractiveObject;
        interactionEnabled = serializedObject.FindProperty("interactionEnabled");
        distance = serializedObject.FindProperty("distance");
        clickWithGaze = serializedObject.FindProperty("clickWithGaze");
        gazeClickTime = serializedObject.FindProperty("gazeClickTime");
        useOutline = serializedObject.FindProperty("useOutline");
        outlineWidth = serializedObject.FindProperty("outlineWidth");
        outlineGrowSpeed = serializedObject.FindProperty("outlineGrowSpeed");
        outlineDepthOffset = serializedObject.FindProperty("outlineDepthOffset");
        outlineColor = serializedObject.FindProperty("outlineColor");
    }

    public override void OnInspectorGUI() {
        if (mainTitle == null) {
            mainTitle = new GUIStyle();
            mainTitle.alignment = TextAnchor.UpperCenter;
            mainTitle.fontSize = 22;
            if (EditorGUIUtility.isProSkin) {
                mainTitle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            }
            else {
                mainTitle.normal.textColor = Color.black;
            }
        }
        serializedObject.Update();
        RenderSectionTop("VR Interactive Object", tooltips[0]);
        RenderSeparator();
        EditorGUILayout.PropertyField(interactionEnabled, new GUIContent("Interaction Enabled:", tooltips[1]));
        EditorGUILayout.PropertyField(distance, new GUIContent("Interaction Distance:", tooltips[2]));
        EditorGUILayout.PropertyField(clickWithGaze, new GUIContent("Click with Gaze?", tooltips[3]));
        if (clickWithGaze.boolValue) {
            EditorGUILayout.PropertyField(gazeClickTime, new GUIContent("  Gaze Click Time:", tooltips[4]));
        }
        EditorGUILayout.PropertyField(useOutline, new GUIContent("Show Outline?", tooltips[5]));
        if (useOutline.boolValue) {
            EditorGUILayout.PropertyField(outlineWidth, new GUIContent("  Outline Width:", tooltips[6]));
            EditorGUILayout.PropertyField(outlineGrowSpeed, new GUIContent("  Outline Grow Speed:", tooltips[7]));
            EditorGUILayout.PropertyField(outlineDepthOffset, new GUIContent("  Outline Depth Offset:", tooltips[8]));
            EditorGUILayout.PropertyField(outlineColor, new GUIContent("  Outline Color:", tooltips[9]));
        }
        serializedObject.ApplyModifiedProperties();
    }

    void RenderSectionHeader(string header, string tooltip = "") {
        EditorGUILayout.LabelField(new GUIContent(header, tooltip), EditorStyles.largeLabel, GUILayout.Height(20f));
    }
    void RenderSectionTop(string header, string tooltip = "") {
        EditorGUILayout.LabelField(new GUIContent(header, tooltip), mainTitle, GUILayout.Height(20f));
    }
    void RenderSeparator() {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }
}

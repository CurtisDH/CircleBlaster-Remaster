using System;
using System.Reflection;
using Managers;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class WaveCreation : EditorWindow
    {
        public XmlManager.FullWaveInformation wave = new();
        public XmlManager.Enemy enemy = new();
        private SerializedObject SO;
        private Vector2 scrollPos;

        [MenuItem("Custom Editors/Wave Creator")]
        public static void ShowWindow()
        {
            WaveCreation.GetWindow(typeof(WaveCreation));
        }

        private void OnEnable()
        {
            SO = new(this);
        }

        private void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width),
                GUILayout.Height(position.height));
            EditorGUILayout.PropertyField(SO.FindProperty("wave"));
            SO.ApplyModifiedProperties();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset"))
            {
                wave = new();
            }

            if (GUILayout.Button("Save"))
            {
                XmlManager.SerializeWaveData(wave);
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(SO.FindProperty("enemy")); //TODO

            GUILayout.EndScrollView();
        }

        //TODO create a load button - Load existing xml wave data so we can easily modify it.
    }
}
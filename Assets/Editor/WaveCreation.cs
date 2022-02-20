using System.Collections.Generic;
using Managers;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;
using Application = UnityEngine.Application;

namespace Editor
{
    public class WaveCreation : EditorWindow
    {
        public List<XmlManager.FullWaveInformation> wave = new();
        public List<XmlManager.Enemy> enemy = new();
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

            if (GUILayout.Button("Save"))
            {
                XmlManager.SerializeData(wave, XmlManager.ConfigName.WaveDataConfig);
            }

            if (GUILayout.Button("Reset"))
            {
                wave = new List<XmlManager.FullWaveInformation>();
                SO.Update();
            }


            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(SO.FindProperty("enemy"));
            SO.ApplyModifiedProperties();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                XmlManager.SerializeData(enemy, XmlManager.ConfigName.EnemyConfig);
            }

            if (GUILayout.Button("Reset"))
            {
                enemy = new List<XmlManager.Enemy>();
                SO.Update();
            }

            GUILayout.EndHorizontal();
            if (GUILayout.Button("Open Module Folder"))
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath + "/Modules");
            }

            if (GUILayout.Button("deserialize"))
            {
                XmlManager.DeserializeAllData();
            }

            if (GUILayout.Button("Find and load all configs"))
            {
                XmlManager.DeserializeData(wave, XmlManager.ConfigName.WaveDataConfig);
                XmlManager.DeserializeData(enemy, XmlManager.ConfigName.EnemyConfig);
                SO.Update();
            }

            GUILayout.EndScrollView();
        }
    }
}
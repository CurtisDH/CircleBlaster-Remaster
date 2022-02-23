using System.Collections.Generic;
using Managers;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Application;

namespace Editor
{
    public class XmlEditor : EditorWindow
    {
        public List<XmlManager.FullWaveInformation> wave = new();
        public List<XmlManager.Enemy> enemy = new();
        public List<XmlManager.Projectile> projectile = new();
        private SerializedObject SO;
        private Vector2 scrollPos;


        [MenuItem("Custom Editors/Wave Creator")]
        public static void ShowWindow()
        {
            XmlEditor.GetWindow(typeof(XmlEditor));
        }

        private void OnEnable()
        {
            SO = new(this);
        }

        private void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width),
                GUILayout.Height(position.height));
            WaveFieldLayout();
            EnemyFieldLayout();
            ProjectileFieldLayout();
            Buttons();

            GUILayout.EndScrollView();
        }



        private void Buttons()
        {
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
                wave = XmlManager.DeserializeWaveData();
                enemy = XmlManager.DeserializeEnemyData();
                projectile = XmlManager.DeserializeProjectileData();
                SO.Update();
            }
        }

        private void WaveFieldLayout()
        {
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
        }

        private void EnemyFieldLayout()
        {
            EditorGUILayout.PropertyField(SO.FindProperty("enemy"));
            SO.ApplyModifiedProperties();
            GUILayout.BeginHorizontal();

            //TODO can i make these buttons generic?
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
        }
        
        private void ProjectileFieldLayout()
        {
            EditorGUILayout.PropertyField(SO.FindProperty("projectile"));
            SO.ApplyModifiedProperties();
            GUILayout.BeginHorizontal();

            //TODO can i make these buttons generic?
            if (GUILayout.Button("Save"))
            {
                XmlManager.SerializeData(projectile, XmlManager.ConfigName.PlayerWeaponProjectileConfig);
            }

            if (GUILayout.Button("Reset"))
            {
                projectile = new List<XmlManager.Projectile>();
                SO.Update();
            }

            GUILayout.EndHorizontal();
        }
    }
}
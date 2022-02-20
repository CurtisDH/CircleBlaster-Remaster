using System.Collections.Generic;
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

        // Used to select prefab
        private int SelectedIndex = 0;
        public List<XmlManager.Enemy> test = new();

        [MenuItem("Custom Editors/Wave Creator")]
        public static void ShowWindow()
        {
            WaveCreation.GetWindow(typeof(WaveCreation));
        }

        private void OnEnable()
        {
            SO = new(this);
            for (int i = 0; i < 10; i++)
            {
                test.Add(new XmlManager.Enemy()
                {
                    displayName = $"Enemy: {i.ToString()}",
                });
            }
        }

        private void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width),
                GUILayout.Height(position.height));
            EditorGUILayout.PropertyField(SO.FindProperty("wave"));
            SO.ApplyModifiedProperties();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset")) //TODO reset doesnt work.
            {
                wave = new XmlManager.FullWaveInformation();
                SO.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Save"))
            {
                XmlManager.SerializeData(wave, XmlManager.ConfigName.WaveDataConfig);
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(SO.FindProperty("enemy"));
            SO.ApplyModifiedProperties();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                XmlManager.SerializeData(enemy, XmlManager.ConfigName.EnemyConfig);
            }

            if (GUILayout.Button("Reset")) //TODO reset doesnt work.
            {
                enemy = new XmlManager.Enemy();
                SO.ApplyModifiedProperties();
            }

            GUILayout.EndHorizontal();
            //DrawPointSelectorInspector(test.ToArray());
            GUILayout.EndScrollView();
        }


        // Pretty close to what I was aiming for thanks to:
        // https://forum.unity.com/threads/any-way-to-create-popup-or-list-box-that-allows-multiple-selections.333392/
        // Not sure how I can implement this for a specific value -- //TODO look into attributes
        // This will remain here until committed as I may improve this editor later
        void OnPointSelected(object index)
        {
            SelectedIndex = (int)index;
        }


        private void DrawPointSelectorInspector(XmlManager.Enemy[] datas)
        {
            List<string> data = new();
            foreach (var d in datas)
            {
                data.Add(d.displayName);
            }

            var selectedPointButtonSb = new System.Text.StringBuilder();


            selectedPointButtonSb.Append($"{data[SelectedIndex]}, ");


            if (!GUILayout.Button(selectedPointButtonSb.ToString())) return;

            var selectedMenu = new GenericMenu();

            for (var i = 0; i < data.Count; ++i)
            {
                var menuString = $"{data[i]}";
                var displaySelectedStatusTick = data[i] == data[SelectedIndex];
                selectedMenu.AddItem(new GUIContent(menuString), displaySelectedStatusTick, OnPointSelected, i);
            }

            selectedMenu.ShowAsContext();


            //TODO create a load button - Load existing xml wave data so we can easily modify it.
        }
    }
}
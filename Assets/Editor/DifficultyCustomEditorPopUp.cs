using _2Scripts.Manager;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class DifficultyCustomEditorPopUp : EditorWindow
    {
        private bool _toggleTest;
        private int _intTest;
        private void OnEnable()
        {
            GetWindow((typeof(DifficultyCustomEditorPopUp)));
        }
        
        private void OnGUI()
        {
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ceci est un label field.", GUILayout.Width(100));
            _toggleTest = EditorGUILayout.Toggle("Ceci est un toggle test.",_toggleTest);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.IntField("int text", _intTest);
            EditorGUILayout.SelectableLabel("Ceci est un label selectionable", GUILayout.Height(GUI.skin.textField.lineHeight));
            
            
            if(GUILayout.Button("Close")) this.Close();
        }

        [MenuItem("Window/Difficulty Manager/Difficulty Editor", false, -1)]
        private static void ShowDifficultyEditor()
        {
            EditorWindow editorPopUp =  GetWindow(typeof(DifficultyCustomEditorPopUp));
            editorPopUp.minSize = new Vector2(600, 400);
            editorPopUp.titleContent.text = "Difficulty Editor";
        }
    }
}

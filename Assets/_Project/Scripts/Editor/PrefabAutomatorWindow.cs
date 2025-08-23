using UnityEngine;
using UnityEditor;

public class PrefabAutomatorWindow : EditorWindow
{
    private GameObject modelAsset;

    [MenuItem("Tools/Prefab Automator")]
    public static void ShowWindow()
    {
        GetWindow<PrefabAutomatorWindow>("Prefab Automator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Arraste seu modelo 3D abaixo:", EditorStyles.boldLabel);
                
        modelAsset = (GameObject)EditorGUILayout.ObjectField("Modelo 3D (.fbx)", modelAsset, typeof(GameObject), false);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Criar Prefab"))
        {
            Debug.Log("Botão clicado! O modelo selecionado é: " + modelAsset.name);
        }
    }
}
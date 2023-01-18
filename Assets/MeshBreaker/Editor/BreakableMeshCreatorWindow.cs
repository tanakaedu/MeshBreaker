using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class BreakableMeshCreatorWindow : EditorWindow
{
    [MenuItem("Tools/DAT/BreakableMeshCreatorWindow")]
    public static void ShowExample()
    {
        BreakableMeshCreatorWindow wnd = GetWindow<BreakableMeshCreatorWindow>();
        wnd.titleContent = new GUIContent("BreakableMeshCreatorWindow");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/MeshBreaker/Editor/BreakableMeshCreatorWindow.uxml");
        VisualElement labelFromUXML = visualTree.CloneTree();
        root.Add(labelFromUXML);

        var updateButton = root.Query<Button>("updateButton");
        updateButton.First().RegisterCallback<MouseDownEvent>(ListupMeshFaces);
    }

    /// <summary>
    /// 選択中のオブジェクトのMeshRendererの面数をスクロールビューに表示
    /// </summary>
    /// <param name="evt"></param>
    private void ListupMeshFaces(MouseDownEvent evt)
    {
        var selection = Selection.activeObject as GameObject;
        if (selection == null) return;

        var scrollView = rootVisualElement.Query<ScrollView>("facesView").First();
        scrollView.Clear();

        int faceCount = 0;

        var meshRenderes = selection.GetComponentsInChildren<MeshRenderer>();
        foreach (var render in meshRenderes)
        {
            var mesh = render.GetComponent<MeshFilter>();
            if ((mesh == null) || (mesh.sharedMesh == null)) continue;
            var label = new Label($"{render.name} {mesh.sharedMesh.triangles.Length / 3}");
            faceCount += (mesh.sharedMesh.triangles.Length / 3);
            scrollView.Add(label);
        }

        var labelAll = new Label($"総ポリゴン数 {faceCount}");
        scrollView.Add(labelAll);
    }
}
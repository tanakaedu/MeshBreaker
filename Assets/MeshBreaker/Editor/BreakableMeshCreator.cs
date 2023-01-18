using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class BreakableMeshCreator : EditorWindow
{
    [MenuItem("Window/UIElements/BreakableMeshCreator")]
    public static void ShowExample()
    {
        BreakableMeshCreator wnd = GetWindow<BreakableMeshCreator>();
        wnd.titleContent = new GUIContent("BreakableMeshCreator");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/BreakableMeshCreator.uxml");
        VisualElement labelFromUXML = visualTree.CloneTree();
        root.Add(labelFromUXML);
    }
}
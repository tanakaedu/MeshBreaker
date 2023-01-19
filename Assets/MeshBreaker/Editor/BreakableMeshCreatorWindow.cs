using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using DAT.MeshExplosion;

public class BreakableMeshCreatorWindow : EditorWindow
{
    [MenuItem("Tools/DAT/BreakableMeshCreatorWindow")]
    public static void ShowEditorWindow()
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
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        var updateButton = root.Query<Button>("updateButton");
        updateButton.First().RegisterCallback<ClickEvent>(ListupMeshFaces);

        var procButton = root.Query<Button>("procButton");
        procButton.First().RegisterCallback<ClickEvent>(CreateBreakableObjects);
    }

    /// <summary>
    /// 選択中のオブジェクトのMeshRendererの面数をスクロールビューに表示
    /// </summary>
    /// <param name="evt"></param>
    private void ListupMeshFaces(ClickEvent evt)
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

    /// <summary>
    /// 生成ボタンが押されたときの処理
    /// </summary>
    void CreateBreakableObjects(ClickEvent evt)
    {
        var selection = Selection.activeObject as GameObject;
        if (selection == null)
        {
            Debug.LogWarning($"分割したいオブジェクトを選択してから実行してください。");
            return;
        }

        var meshRenderers = selection.GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers.Length == 0) return;

        // 破壊ブロックを配下におくための親オブジェクトの作成
        var breakableParent = MakeBreakableParentObject(selection);

        var merge = rootVisualElement.Query<SliderInt>("mergeSlider");

        foreach (var render in meshRenderers)
        {
            var mesh = render.GetComponent<MeshFilter>();
            if ((mesh != null) && (mesh.sharedMesh != null))
            {
                // 配列からリストを作成
                DivideTriangles(breakableParent, mesh, merge.First().value);
            }
        }

        Debug.Log($"生成完了");
    }

    /// <summary>
    /// 指定の選択オブジェクトをもとに破壊オブジェクトの親を作成
    /// </summary>
    /// <param name="selection">制作もとのオブジェクト</param>
    GameObject MakeBreakableParentObject(GameObject selection)
    {
        var breakableParent = new GameObject($"Breakable_{selection.name}");
        breakableParent.transform.position = selection.transform.position;
        breakableParent.transform.rotation = Quaternion.identity;
        breakableParent.transform.localScale = Vector3.one;
        breakableParent.AddComponent<ExplodeParent>();

        // BoxCollider
        breakableParent.AddComponent<BoxCollider>();
        var sourceCollider = selection.GetComponent<BoxCollider>();
        if (sourceCollider == null)
        {
            Debug.LogWarning("親オブジェクトにBoxColliderがないので、当たり判定を手動で調整してください。");
        }
        else
        {
            var destCollider = breakableParent.GetComponent<BoxCollider>();
            destCollider.center = new Vector3(
                 sourceCollider.center.x * selection.transform.lossyScale.x,
                 sourceCollider.center.y * selection.transform.lossyScale.y,
                 sourceCollider.center.z * selection.transform.lossyScale.z
                 );
            destCollider.size = new Vector3(
                 sourceCollider.size.x * selection.transform.lossyScale.x,
                 sourceCollider.size.y * selection.transform.lossyScale.y,
                 sourceCollider.size.z * selection.transform.lossyScale.z
                 );
        }

        // Effectコンポーネントを追加
        breakableParent.AddComponent<ExplodeEffect>();
        return breakableParent;
    }

    /// <summary>
    /// 指定のオブジェクトを親にして、指定のメッシュを規定のポリゴン数ごとに
    /// まとめたオブジェクトを生成する。
    /// </summary>
    /// <param name="parent">まとめるための親オブジェクト</param>
    /// <param name="mesh">メッシュ</param>
    /// <param name="mergeCount">統合するポリゴン数</param>
    void DivideTriangles(GameObject parent, MeshFilter mesh, int mergeCount)
    {
        List<int> triangles = null;
        triangles = new List<int>(mesh.sharedMesh.triangles);

        // ポリゴン統合ループ
        while (triangles.Count > 0)
        {
            if (triangles.Count < mergeCount * 3 * 2)
            {
                // 残りのポリゴン数が２つ以上統合できない時、残りのポリゴン数の半分ずつの２つのグループで統合を試すようにメソッドを呼び出す
                int count = triangles.Count / 3 / 2;
                CreateBrokeObject(parent, mesh, triangles, count);
                CreateBrokeObject(parent, mesh, triangles, triangles.Count / 3);
            }
            else
            {
                // 残りのポリゴン数が２つ以上統合できるなら、統合数を指定してメソッドを呼び出す
                CreateBrokeObject(parent, mesh, triangles, mergeCount);
            }
        }
    }

    /// <summary>
    /// 指定のオブジェクトの子供に、
    /// 指定の頂点リストからmergeCount個のポリゴンを統合した
    /// オブジェクトを作成。
    /// </summary>
    /// <param name="parent">作成したオブジェクトの親</param>
    /// <param name="mesh">元のメッシュ</param>
    /// <param name="tris">残りの頂点リスト</param>
    /// <param name="mergeCount">結合数</param>
    void CreateBrokeObject(GameObject parent, MeshFilter mesh, List<int> tris, int mergeCount)
    {
        int verCount = mergeCount * 3;

        // TODO: 実際に作成するポリゴン数を最終調整
        //// 引数で渡された統合数に離れているポリゴンがないかをチェック
        //// 離れているポリゴンがあったらその前までに範囲を狭める

        // TODO: 厚さがあるかを確認
        //// 厚さがないなら、法線逆向きに頂点を１つ追加
        //// 穴をふさぐ頂点インデックスを生成

        // 元オブジェクトの頂点と破片用の頂点を対応付け
        var verticesMap = MakeSourceVerticesToDebrisVerticesMap(tris, verCount);

        // 元データを取得
        var sourceRenderer = mesh.GetComponent<MeshRenderer>();
        var sourceVertices = mesh.sharedMesh.vertices;
        var sourceTris = mesh.sharedMesh.triangles;
        var sourceNormals = mesh.sharedMesh.normals;

        // 破片オブジェクトへデータをコピー
        Vector3[] vertices = new Vector3[verticesMap.Count];
        Vector3[] customNormals = new Vector3[verticesMap.Count];
        foreach (var data in verticesMap)
        {
            vertices[data.Value] = mesh.transform.TransformPoint(sourceVertices[data.Key]);
            customNormals[data.Value] = mesh.transform.TransformDirection(sourceNormals[data.Key]);
        }
        Vector3 barySum = Vector3.zero;
        int[] setTris = new int[verCount];
        for (int i = 0; i < verCount; i++)
        {
            setTris[i] = verticesMap[tris[i]];
            barySum += vertices[setTris[i]];
        }
        Vector3 baryCenter = barySum / verCount;

        // 確定した統合数でオブジェクトを生成
        var debris = new GameObject();
        debris.transform.parent = parent.transform;
        debris.transform.position = baryCenter;
        debris.name = "Debris";
        debris.layer = LayerMask.NameToLayer("Debris");
        debris.AddComponent<MeshRenderer>();
        debris.AddComponent<MeshFilter>();
        debris.AddComponent<ExplodeDebris>();

        // マテリアル設定
        var customRenderer = debris.GetComponent<MeshRenderer>();
        customRenderer.material = sourceRenderer.material;
        var customMesh = debris.GetComponent<MeshFilter>();

        // メッシュデータを設定
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= baryCenter;
        }

        customMesh.sharedMesh = new Mesh();
        customMesh.sharedMesh.vertices = vertices;
        customMesh.sharedMesh.triangles = setTris;
        customMesh.sharedMesh.normals = customNormals;
        customMesh.sharedMesh.name = "debris_mesh";

        // 速度設定
        debris.AddComponent<Rigidbody>();
        var rb = debris.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        // 取り出した頂点はリストから削除
        for (int i = 0; i < verCount; i++)
        {
            tris.RemoveAt(0);
        }

        // 当たり判定
        debris.AddComponent<MeshCollider>();
        var meshCol = debris.GetComponent<MeshCollider>();
        meshCol.sharedMesh = customMesh.sharedMesh;
        meshCol.convex = true;
        meshCol.enabled = false;
    }

    /// <summary>
    /// 受け取った頂点インデックスの先頭からmergeCountポリゴン数分の
    /// 頂点のインデックスを通し番号と対応づけるDictionaryを作成して返す。
    /// </summary>
    /// <param name="tris">頂点インデックス</param>
    /// <param name="verticesCount">頂点数</param>
    /// <returns>頂点インデックスを通し番号と対応づけるmapデータ</returns>
    Dictionary<int, int> MakeSourceVerticesToDebrisVerticesMap(List<int> tris, int verticesCount)
    {
        var dict = new Dictionary<int, int>();
        int counter = 0;

        for (int i = 0; i < verticesCount; i++)
        {
            if (!dict.ContainsKey(tris[i]))
            {
                dict.Add(tris[i], counter);
                counter++;
            }
        }

        return dict;
    }
}
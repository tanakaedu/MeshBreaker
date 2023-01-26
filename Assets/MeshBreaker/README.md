# MeshBreaker

破壊可能オブジェクトを作成するエディター拡張です。

## 動作環境
- Unity2021.3.X以降

## 動作デモ

[動作デモ](https://tanakaedu.github.io/MeshBreaker/WebGL/index.html)

WASDキーか矢印キーで赤い球体を操作します。
赤い球体で白い球体に触れると破壊します。

## エディター拡張の利用準備

### パッケージをプロジェクトへ組み込む
以下の手順でパッケージをダウンロードしてプロジェクトに組み込みます。

1. [Releases](https://github.com/tanakaedu/MeshBreaker/releases)を開きます
2. 最新のバージョンの `MeshBreaker???.unitypackage` をダウンロードします
1. ダウンロードしたMeshBreaker???.unitypackageファイルをドラッグして、組み込みたいUnityプロジェクトのProjectウィンドウにドロップしてインポートします

### レイヤーとタグの設定
Unityプロジェクトに必要なレイヤーとタグの作成と設定をします。

1. Playerレイヤーを作成して、プレイヤーなどの破壊する側のキャラに設定
1. Debrisレイヤーを作成
1. Project SettingsのPhysics設定を開いて、PlayerとDebrisレイヤーが接触しないようにチェックを外す
1. プレイヤーキャラにPlayerタグを設定

Debrisレイヤーはエディターが作成する破片オブジェクトに設定するので作成だけしておけばOKです。

## 破壊オブジェクトの作り方

レイヤーが準備できたら以下の手順で破壊される側のオブジェクトを作成します。

1. ToolsメニューからDAT > BreakableMeshCreatorWindow を選択
1. Hierarchyウィンドウから破壊したいオブジェクトを選択します。選択するのは、接触を検出するBoxColliderがアタッチされているオブジェクトだけを選んでください。Is Triggerはオフにします
1. BreakableMeshCreatorWindowの情報更新ボタンを押します
1. 総ポリゴン数を確認して、500オブジェクト以下になるぐらいにポリゴンの統合数を決めます。例えば総ポリゴン数が6000ポリゴンなら統合するポリゴンの数を**12**にします。総ポリゴン数が2000ポリゴン以下なら統合するポリゴン数は4ポリゴンにします
1. 統合するポリゴンの数を設定したら、破壊オブジェクトの生成開始ボタンを押します。Hierarchyウィンドウに`Breakable_元のオブジェクト名`という名前で破壊オブジェクトが生成されます

以上で生成完了です。生成ができたら生成元のオブジェクトは削除します。

破壊したいオブジェクトが複数配置されているなら、プレハブを作成して元のオブジェクトに置き換えます。

## 破壊時のエフェクト

生成した破壊オブジェクトのExplode EffectコンポーネントのParticle欄にパーティクルのプレハブをアタッチすると、接触時に設定したプレハブを生成します。

パーティクルには以下を設定します。

- Loopingのチェックを外す
- Play On Awakeにチェック
- Stop ActionをDestroyに設定

効果音を鳴らす場合、プレハブにAudio Sourceをアタッチして、鳴らしたいAudio Clipを設定しておきます。
Play On Awakeにチェックすればパーティクル発生時に自動的に鳴ります。
また、Spatial Blendを1にすれば3Dサウンドで鳴ります。

## パフォーマンス向上
オブジェクト数が大幅に増えるため、パフォーマンスが落ちる可能性があります。その場合はLODを設定して、近距離のオブジェクトに破壊できるオブジェクト、遠距離を元のオブジェクトに設定することでパフォーマンスアップが期待できます。

参考 [Unityマニュアル. メッシュのLOD](https://docs.unity3d.com/ja/2021.3/Manual/LevelOfDetail.html)

## デモシーン
`Assets/MeshBreaker/Demo/Scenes/MeshBreakerDemo`シーンを開くと、動作確認ができます。

## 関連URL
- [Unity2019でのUI Elements](https://docs.unity3d.com/2019.3/Documentation/Manual/UIE-Controls.html)
- [UI Elements Developer Guide](https://docs.unity3d.com/2019.3/Documentation/Manual/UIElements.html)
- [UI Elements Reference](https://docs.unity3d.com/2019.3/Documentation/Manual/UIE-ElementRef.html)
- [Unityマニュアル. メッシュのLOD](https://docs.unity3d.com/ja/2021.3/Manual/LevelOfDetail.html)

## ライセンス
[MIT License](./LICENSE)

Copyright (c) 2023 Tanaka Yu

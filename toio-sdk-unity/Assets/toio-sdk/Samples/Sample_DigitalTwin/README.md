## Sample_DigitalTwin

このサンプルはリアルキューブの動きをシミュレーターのキューブにリアルタイムに反映する、いわゆる「ディジタルツイン」を実現したものです。

<div align="center">
<img src="../../../../../docs/res/samples/digitaltwin_demo.gif">
</div>
<br>

スクリプトが2つあります。
- `DigitalTwinBinder.cs`：接続するリアルキューブのローカルネームと対応シミュレーターの指定と、動きの反映を実装したスクリプト
- `Sample_DigitalTwin.cs`：メインスクリプト、接続したキューブを制御するスクリプト

### サンプルの使い方

<div align="center">
<img src="../../../../../docs/res/samples/digitaltwin_prop.png">
</div>
<br>

ゲームオブジェクト`Binder`のインスペクターで、`Binding Table` に接続したいリアルキューブのローカルネームと、動きを再現させたいシミュレーターキューブを設定します。

この図の場合、`toio-e4h` というローカルネームのリアルキューブと接続できれば、その動きを `Cube` という名前のシミュレーターキューブに再現させます。
`toio-n0Q`と接続できれば、 `Cube2` に再現させます。また、接続できなければ、対応シミュレーターは動きません。リストに設定していないローカルネームのキューブとは接続しません。

他のパラメータ（次節を参照）を任意に設定して実行・ビルドします。

### `DigitalTwinBinder.cs` のパラメータ

- `Binding Table`：接続するリアルキューブのローカルネームと、動きを再現するシミュレータキューブのテーブル
- `Mat`：シミュレータキューブが置かれるマット
- `Mapping Method`：座標と角度をマッピングする方法
  - `Direct`：リアルキューブの座標と角度をそのままシミュレータキューブに設定する
    - リアルタイム性が高い
    - 情報がノイジーでシミュレータキューブが振動しているように見える
  - `AddForce`：リアルキューブの座標と角度に向けて、シミュレータキューブに力を加える
    - 少し遅延が感じられる
    - 振動が抑えられる
    - シミュレータ上のオブジェクトと衝突した場合、より安定した動きが期待される


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Linq;

public class Sample_ConnectName : MonoBehaviour
{
    public GameObject cubeItemPrefab;
    public ConnectType connectType = ConnectType.Real;
    public Button scanButton;
    public RectTransform listContent;

    private CubeScanner scanner;
    private CubeConnecter connecter;
    private Dictionary<string, GameObject> cubeItems = new Dictionary<string, GameObject>();
    private List<string> connectedAddrs = new List<string>();
    private List<BLEPeripheralInterface> connectedPeripherals = new List<BLEPeripheralInterface>();
    private List<BLEPeripheralInterface> scannedPeripherals = new List<BLEPeripheralInterface>();

    void Start()
    {
        this.scanner = new CubeScanner(this.connectType);
        this.connecter = new CubeConnecter(this.connectType);
    }

    void Update ()
    {
        int idx = 0;

        var addrsToRemove = this.cubeItems.Keys.ToList();

        // Display connected items
        foreach (var peripheral in this.connectedPeripherals) {
            var item = TryGetCubeItem(peripheral);
            item.transform.SetSiblingIndex(idx++);
            addrsToRemove.Remove(peripheral.device_address);
        }

        // Display scanned items
        foreach (var peripheral in this.scannedPeripherals) {
            if (peripheral == null) continue;
            var item = TryGetCubeItem(peripheral);
            item.transform.SetSiblingIndex(idx++);
            addrsToRemove.Remove(peripheral.device_address);
        }

        // Remove disappeared items
        foreach (var addr in addrsToRemove)
        {
            Destroy(this.cubeItems[addr]);
            this.cubeItems.Remove(addr);
        }

    }

    GameObject TryGetCubeItem (BLEPeripheralInterface peripheral)
    {
        if (!this.cubeItems.ContainsKey(peripheral.device_address))
        {
            var item = Instantiate(this.cubeItemPrefab, this.listContent);
            item.GetComponent<Button>().onClick.AddListener(async () => await OnItemClick(item, peripheral));
            item.GetComponentInChildren<Text>().text = peripheral.device_name + (peripheral.isConnected? ": connected" : ": paired");
            this.cubeItems.Add(peripheral.device_address, item);
            return item;
        }
        return this.cubeItems[peripheral.device_address];
    }

    public void StartScan()
    {
        // Clear list (except connected items)
        foreach (var addr in this.cubeItems.Keys.ToArray())
        {
            if (this.connectedAddrs.Contains(addr))
                continue;
            Destroy(this.cubeItems[addr]);
            this.cubeItems.Remove(addr);
        }

        // Start scan
        this.scanButton.interactable = false;
        this.scanButton.GetComponentInChildren<Text>().text = "Scanning...";
        this.scanner.StartScan(OnScanUpdate, OnScanEnd, 20).Forget();
    }

    void OnScanEnd()
    {
        this.scanButton.interactable = true;
        this.scanButton.GetComponentInChildren<Text>().text = "Scan";
    }

    void OnScanUpdate(BLEPeripheralInterface[] peripherals)
    {
        this.scannedPeripherals = peripherals.ToList();

        // Add connection listener
        foreach (var peripheral in peripherals)
        {
            if (peripheral == null) continue;
            peripheral.AddConnectionListener("Sample_ConnectName", this.OnConnection);
        }
    }

    async UniTask OnItemClick(GameObject item, BLEPeripheralInterface peripheral)
    {
        if (peripheral.isConnected) {
#if !(UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
            // NOTE: On Windows, disconnecting a device causes crash
            this.connecter.Disconnect(peripheral);
            await UniTask.Delay(200);
            if (item)
                item.GetComponentInChildren<Text>().text = peripheral.device_name + ": paired";
#endif
        }
        else{
            item.GetComponentInChildren<Button>().interactable = false;
            item.GetComponentInChildren<Text>().text = peripheral.device_name + ": connecting...";
            try {
                var cube = await this.connecter.Connect(peripheral);
                if (cube == null) {
                    item.GetComponentInChildren<Text>().text = peripheral.device_name + ": failed";
                }
            }
            catch (System.Exception e) {
                if (item)
                    item.GetComponentInChildren<Text>().text = peripheral.device_name + ": failed";
#if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                // Connectition fail twice causes BLE host device lost issue on iOS/macOS
                if (this.connectedPeripherals.Contains(peripheral))
                    this.connectedPeripherals.Remove(peripheral);
#endif
                Debug.LogError(e);
                return;
            }
            if (item)
                item.GetComponentInChildren<Button>().interactable = true;
        }
    }

    void OnConnection(BLEPeripheralInterface peripheral)
    {
        if (peripheral.isConnected)
        {
            if (!this.connectedPeripherals.Contains(peripheral))
                this.connectedPeripherals.Add(peripheral);

            if (this.cubeItems.ContainsKey(peripheral.device_address))
                this.cubeItems[peripheral.device_address].GetComponentInChildren<Text>().text = peripheral.device_name + ": connected";
        }
        else
        {
            if (this.connectedPeripherals.Contains(peripheral))
                this.connectedPeripherals.Remove(peripheral);

            if (this.cubeItems.ContainsKey(peripheral.device_address))
                this.cubeItems[peripheral.device_address].GetComponentInChildren<Text>().text = peripheral.device_name + ": paired";
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using VRTK;
using IATK;

public class DataSelector : MonoBehaviour
{
    public VRTK_ControllerEvents controllerEvents;
    public GameObject menu;
    public SelectionMatrix selectionMatrixPrefab;
    public MatrixVisualizer matrixVisualizerPrefab;
    bool menuState = false;

    public TextAsset dataFile;
    public CSVDataSource dataSource;

    // Start is called before the first frame update
    void Start()
    {
        FileBrowser.HideDialog();
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Data Files", ".csv"));
        FileBrowser.SetDefaultFilter(".csv");
    }

    void OnEnable() {
        controllerEvents.ButtonTwoReleased += toggleMenu;
    }
    void OnDisable() {
        controllerEvents.ButtonTwoReleased -= toggleMenu;
    }

    private void showMenu() {
        gameObject.GetComponent<VRTK_TransformFollow>().enabled = false;
        FileBrowser.ShowLoadDialog(
            loadFile,
            hideMenu,
            FileBrowser.PickMode.Files,
            false,
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\IATK\\Datasets",
            null,
            "Select CSV Datafile",
            "Select");
    }

    private void hideMenu() {
        gameObject.GetComponent<VRTK_TransformFollow>().enabled = true;
        FileBrowser.HideDialog();
    }

    private void loadFile(string[] paths) {
        string path = paths[0];
        print("Loading File");
        print(path);
        Application.OpenURL(path);
        dataFile = new TextAsset(FileBrowserHelpers.ReadTextFromFile(path));
        print(dataFile);
        dataSource = createCSVDataSource(dataFile.text);

        var visualizer = Instantiate(matrixVisualizerPrefab);
        var selectionMatrix = Instantiate(selectionMatrixPrefab);

        GameObject visualization = new GameObject("Visualization");

        selectionMatrix.visualizer = visualizer;
        visualizer.selectionMatrix = selectionMatrix;

        selectionMatrix.transform.SetParent(visualization.transform);
        visualizer.transform.SetParent(visualization.transform);

        visualizer.setDataSource(dataSource);
        
    }

    CSVDataSource createCSVDataSource(string data) {
        CSVDataSource source;
        source = gameObject.AddComponent<CSVDataSource>();
        source.load(data, null);
        return source;
    }

    private void toggleMenu(object sender, ControllerInteractionEventArgs eventArgs) {
        print("Toggling file dialog");
        menuState = !menuState;
        print(menuState);
        if (menuState) {
            showMenu();
        } else {
            hideMenu();
        }
    }
}

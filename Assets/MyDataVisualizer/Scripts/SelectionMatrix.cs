using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IATK;
using VRTK;
using MyDataVisualizer;

namespace MyDataVisualizer
{

    public class SelectionMatrix : MonoBehaviour {
        public GameObject canvas;
        public Button buttonPrefab;
        public Text labelPrefab;
        public MatrixVisualizer visualizer;
        List<string> dataColumns;
        List<List<Button>> buttons;

        // Start is called before the first frame update
        void Start() {
            if (buttons == null) {
                buttons = new List<List<Button>>();
            }
            visualizer.selectionMatrix = this;

            var controllers = GameObject.FindGameObjectsWithTag("GameController");
            foreach (var controller in controllers) {
            }
        }

        public void CloseVisualization() {
            Destroy(transform.parent.gameObject);
        }

        public void clickButton(int row, MatrixVisualizer.VIEW_DIMENSION dimension) {
            List<Button> column = buttons[(int)dimension];
            Button button = column[row];
            Image image = button.GetComponent<Image>();
            if (image.color == Color.red) {
                image.color = Color.white;
                visualizer.resetViewDimension(dimension);
            }
            else {
                foreach(var _button in column) {
                    _button.GetComponent<Image>().color = Color.white;
                }
                image.color = Color.red;
                visualizer.setViewDimension(dataColumns[row], dimension);
            }
        }

        public void initDataSource() {
            dataColumns = new List<string>();
            foreach (var dim in visualizer.dataSource) 
            {
                dataColumns.Add(dim.Identifier); 
            } 
            generateMatrixButtons();
            clickButton(0, MatrixVisualizer.VIEW_DIMENSION.X);
            clickButton(1, MatrixVisualizer.VIEW_DIMENSION.Y);
            clickButton(2, MatrixVisualizer.VIEW_DIMENSION.Z);
        }

        // Update is called once per frame
        void Update() {
            
        }

        void generateMatrixButtons() {

            int i = 0;
            foreach (var dim in visualizer.dataSource) {
                var label = Instantiate(labelPrefab);
                label.transform.SetParent(canvas.transform, false);
                label.text = dim.Identifier;
                label.GetComponent<RectTransform>().localPosition = topLeft + new Vector3(
                    -label.GetComponent<RectTransform>().sizeDelta.x,
                    -i * buttonHeight);
                i++;
            }

            i = 0;
            foreach(var dimension in 
                    EnumUtil.GetValues<MatrixVisualizer.VIEW_DIMENSION>()) {
                var column = generateColumn(dimension);
                column.GetComponent<RectTransform>().localPosition = new Vector3(
                    buttonWidth * i, 0,  0);
                i++;
            }

            buttonPrefab.GetComponent<RectTransform>().sizeDelta = 
                new Vector2(
                    buttonWidth - buttonWidth/4,
                    buttonHeight - buttonHeight/4);

            var close_button = Instantiate(buttonPrefab);
            close_button.transform.SetParent(canvas.transform, false);
            close_button.GetComponentInChildren<Text>().text = "Remove visualization";
            close_button.GetComponent<RectTransform>().localPosition = topRight;

            close_button.onClick.AddListener(CloseVisualization);

            close_button.gameObject.SetActive(true);

        }

        public Vector3 topLeft {
            get {
                return new Vector3(
                -width / 2 + buttonWidth / 2,
                height / 2 - buttonHeight / 2);
            }
        }

        public Vector3 topRight {
            get {
                return topLeft + new Vector3(width, 0f);
            }
        }

        public int numberOfViewDimensions {
            get {
                return EnumUtil.Count<MatrixVisualizer.VIEW_DIMENSION>();
            }
        }

        public float width {
            get {
                return canvas.GetComponent<RectTransform>().rect.width;
            }
        }
        public float buttonWidth {
            get {
                return width / (numberOfViewDimensions + 1);
            }
        }

        public float height {
            get {
                return canvas.GetComponent<RectTransform>().rect.height;
            }
        }

        public float buttonHeight {
            get {
                return height / dataColumns.Count;
            }
        }

        GameObject generateColumn(MatrixVisualizer.VIEW_DIMENSION dimension) {
            List<Button> _buttons = new List<Button>();
            var column = new GameObject("column");
            column.AddComponent<RectTransform>();
            column.transform.SetParent(canvas.transform, false);
            var screen = canvas.GetComponent<RectTransform>();

            buttonPrefab.GetComponent<RectTransform>().sizeDelta = 
                new Vector2(
                    buttonWidth - buttonWidth/4,
                    buttonHeight - buttonHeight/4);

            var label = Instantiate(labelPrefab);
            label.transform.SetParent(column.transform, false);
            label.text = dimension.ToString();
            label.GetComponent<RectTransform>().localPosition = topLeft;

            int i = 0;
            foreach (var dim in visualizer.dataSource) {
                int index = i;
                Button button = Instantiate(buttonPrefab);
                _buttons.Add(button);
                button.GetComponent<RectTransform>().localPosition = new Vector3(
                    0,
                    -i * buttonHeight) + topLeft;
                button.transform.SetParent(column.transform, false);
                button.GetComponentInChildren<Text>().text = dimension.ToString();
                button.onClick.AddListener(() => {
                    clickButton(index, dimension);
                });

                button.gameObject.SetActive(true);
                i++;
            }

            if (buttons == null) {
                buttons = new List<List<Button>>();
            }
            buttons.Add(_buttons);

            return column;
        }

        GameObject generateRow(string data_column) {
            List<Button> _buttons = new List<Button>();
            var row = new GameObject("row");
            row.AddComponent<RectTransform>();
            row.transform.SetParent(canvas.transform, false);
            var screen = canvas.GetComponent<RectTransform>();

            buttonPrefab.GetComponent<RectTransform>().sizeDelta = 
                new Vector2(
                    buttonWidth - buttonWidth/4,
                    buttonHeight - buttonHeight/4);
            
            var label = Instantiate(labelPrefab);
            label.transform.SetParent(row.transform, false);
            label.text = data_column;
            label.GetComponent<RectTransform>().localPosition = topLeft;

            int i = 1;
            foreach(var dimension in EnumUtil
                    .GetValues<MatrixVisualizer.VIEW_DIMENSION>()) {
                print(dimension);
                int index = i;
                Button button = Instantiate(buttonPrefab);
                _buttons.Add(button);
                button.GetComponent<RectTransform>().localPosition = new Vector3(
                    i * buttonWidth,
                    0) + topLeft;
                button.transform.SetParent(row.transform, false);
                button.GetComponentInChildren<Text>().text = dimension.ToString();
                button.GetComponent<Button>().onClick.AddListener(() => {
                    print("Test onClick");
                    clickButton(index-1, dimension);
                });

                button.gameObject.SetActive(true);
                i++;
            }

            buttons.Add(_buttons);
            
            return row;
        }
    }

    static class EnumUtil {
        public static IEnumerable<T> GetValues<T>() {
            return (T[])System.Enum.GetValues(typeof(T));
        }

        public static string[] GetNames<T>() {
            return System.Enum.GetNames(typeof(T));
        }

        public static int Count<T>() {
            return EnumUtil.GetNames<T>().Length;
        }
    }
}

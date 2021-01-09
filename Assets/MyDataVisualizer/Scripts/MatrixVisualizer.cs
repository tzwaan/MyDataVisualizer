using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IATK;
using System.Linq;


namespace MyDataVisualizer
{

    public class MatrixVisualizer : MonoBehaviour
    {
        public enum VIEW_DIMENSION { X, Y, Z, COLOR, SIZE };
        public SelectionMatrix selectionMatrix;
        public GameObject x_axis;
        public GameObject y_axis;
        public GameObject z_axis;
        CSVDataSource _dataSource;
        public CSVDataSource dataSource {
            get {
                return _dataSource;
            }
        }
        private bool initialized = false;
        private string _name;
        public string visualizer_name {
            get => _name;
        }

        View currentView;
        ViewBuilder vb;
        Material mt;
        List<string> columns;
        Gradient g;

        // Start is called before the first frame update
        void Start()
        {
        }

        public Vector3 CenterPosition {
            get => currentView.gameObject.transform.position;
        }

        public void Init(string filename) {
            if (initialized) return;
            _name = filename;
            columns = new List<string>();
            mt = IATKUtil.GetMaterialFromTopology(
                AbstractVisualisation.GeometryType.Arrows);
            mt.SetFloat("_MinSize", 0.01f);
            mt.SetFloat("_MaxSize", 0.5f);
            g = new Gradient {
                colorKeys = new GradientColorKey[] {
                    new GradientColorKey(Color.blue, 0),
                    new GradientColorKey(Color.green, 0.5f),
                    new GradientColorKey(Color.red, 1),
                }
            };
            initialized = true;
        }

        public void setDataSource(CSVDataSource source) {
            _dataSource = source;
            columns = new List<string>();
            foreach (var dim in dataSource) 
            {
                columns.Add(dim.Identifier); 
            } 
            vb = CreateViewBuilder();
            currentView = vb.updateView().apply(gameObject, mt);
            selectionMatrix.initDataSource();
        }

        public void setViewDimension(string column, VIEW_DIMENSION dimension) {
            switch (dimension) {
                case VIEW_DIMENSION.X:
                    vb = vb.setDataDimension(
                        dataSource[column].Data, 
                        ViewBuilder.VIEW_DIMENSION.X);
                    x_axis.GetComponentInChildren<Text>().text = column;
                    break;
                case VIEW_DIMENSION.Y:
                    vb = vb.setDataDimension(
                        dataSource[column].Data,
                        ViewBuilder.VIEW_DIMENSION.Y);
                    y_axis.GetComponentInChildren<Text>().text = column;
                    break;
                case VIEW_DIMENSION.Z:
                    vb = vb.setDataDimension(
                        dataSource[column].Data,
                        ViewBuilder.VIEW_DIMENSION.Z);
                    z_axis.GetComponentInChildren<Text>().text = column;
                    break;
                case VIEW_DIMENSION.COLOR:
                    vb = vb.setColors(
                        dataSource[column].Data.Select(
                            x => g.Evaluate(x)).ToArray()
                    );
                    break;
                case VIEW_DIMENSION.SIZE:
                    vb = vb.setSize(
                        dataSource[column].Data
                    );
                    break;
                default:
                    throw new System.NotImplementedException();
            }
            UpdateView();
        }

        void UpdateView() {
            Destroy(currentView.gameObject);
            currentView = vb.updateView().apply(gameObject, mt);
        }

        ViewBuilder CreateViewBuilder() {
            var viewBuilder = new ViewBuilder(
                MeshTopology.Points,
                visualizer_name
            ).initialiseDataView(dataSource.DataCount);
            return viewBuilder;
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
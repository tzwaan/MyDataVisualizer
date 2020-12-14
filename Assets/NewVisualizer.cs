using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IATK;

public class NewVisualizer : MonoBehaviour
{

    public TextAsset dataFile;

    CSVDataSource dataSource;
    View currentView;
    List<string> columns;

    // Start is called before the first frame update
    void Start()
    {
        dataSource = createCSVDataSource(dataFile.text); 
        columns = new List<string>(); 
        foreach (var dim in dataSource) 
        {
            print(dim.Identifier); 
            columns.Add(dim.Identifier); 
        } 
        print(columns);
        print(dataSource.DataCount);

        currentView = Uber(dataSource);

        InvokeRepeating("GenerateNewView", 4.0f, 4.0f);
    }

    CSVDataSource createCSVDataSource(string data)
    {
        CSVDataSource dataSource;
        dataSource = gameObject.AddComponent<CSVDataSource>();
        dataSource.load(data, null);
        return dataSource;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    View GenerateNewView()
    {

        string RandomColumn() {
            return columns[Random.Range(0, columns.Count)];
        };
        print(RandomColumn());

        ViewBuilder vb = new ViewBuilder(MeshTopology.Points, "Uber pick up point visualisation").
            initialiseDataView(dataSource.DataCount);

        vb = vb.setDataDimension(dataSource[RandomColumn()].Data, ViewBuilder.VIEW_DIMENSION.X);
        vb = vb.setDataDimension(dataSource[RandomColumn()].Data, ViewBuilder.VIEW_DIMENSION.Y);

        Material mt = IATKUtil.GetMaterialFromTopology(AbstractVisualisation.GeometryType.Points);
        mt.SetFloat("_MinSize", 0.01f);
        mt.SetFloat("_MaxSize", 0.5f);

        Destroy(currentView.gameObject);
        currentView = vb.updateView().apply(gameObject, mt);
        return currentView;
    }

    View Uber(CSVDataSource csvds)
    {
        Gradient g = new Gradient
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.blue, 0),
                new GradientColorKey(Color.red, 1),
            }
        };

        ViewBuilder vb = new ViewBuilder(MeshTopology.Points, "Uber pick up point visualisation").
            initialiseDataView(csvds.DataCount).
            setDataDimension(csvds["Lat"].Data, ViewBuilder.VIEW_DIMENSION.X).
            setDataDimension(csvds["Lon"].Data, ViewBuilder.VIEW_DIMENSION.Y).
            setDataDimension(csvds["Time"].Data, ViewBuilder.VIEW_DIMENSION.Z);
            //setSize(csvds["Base"].Data).
            //setColors(csvds["Lat"].Data.Select(x => g.Evaluate(x)).ToArray());

        Material mt = IATKUtil.GetMaterialFromTopology(AbstractVisualisation.GeometryType.Points);
        mt.SetFloat("_MinSize", 0.01f);
        mt.SetFloat("_MaxSize", 0.5f);

        View v = vb.updateView().apply(gameObject, mt);
        return v;
    }
}

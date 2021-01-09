using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Unity.PerformanceTesting;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using VRTK;
using MyDataVisualizer;
#if ENABLE_VR
using UnityEngine.XR;
#endif
using Tests;

namespace Tests
{
    public class CustomException : Exception
    {
    }
    public class NewTestScript
    {
        private readonly string basicSceneName = "Assets/MyDataVisualizer/Scenes/DefaultScene.unity";
        protected readonly float SettleTimeSeconds = 2f;
        private string defaultDataSet = "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_10000.csv";
        private string[] dataSets = new string[] {
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_100.csv",
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_1000.csv",
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_10000.csv",
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_20000.csv",
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_30000.csv",
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_40000.csv",
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_50000.csv",
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_60000.csv",
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_70000.csv",
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_80000.csv",
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_90000.csv",
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_100000.csv",
            "C:\\Users\\tijme\\Unity Projects\\IATK-master\\Assets\\MyDataVisualizer\\Datasets\\random\\export_150000.csv"
        };

        protected string[] SamplerNames = {
            "Camera.Render",
            "Render.Mesh",
        };
        private void FixateCamera(Vector3 position, Vector3 target) {
            XRDevice.DisableAutoXRCameraTracking(Camera.main, true);
            var transform = Camera.main.gameObject.transform;
            transform.position = position;
            transform.LookAt(target);
            // transform.rotation.SetFromToRotation(transform.position, target);
        }

        [SetUp]
        public void Setup() {
            #if ENABLE_VR

            #endif
        }

        [UnityTest, Performance]
        public IEnumerator SingleVisualizerPlay() {
            int sceneCount = SceneManager.sceneCount;
            Scene [] scenes = new Scene[sceneCount];
            for (int i=0; i<sceneCount; i++) {
                scenes[i] = SceneManager.GetSceneAt(i);
            }

            yield return SceneManager.LoadSceneAsync(basicSceneName);

            // var scene = SceneManager.GetSceneByName(basicSceneName);
            // yield return scene;
            // SceneManager.SetActiveScene(scene);
            var scene = SceneManager.GetActiveScene();

            yield return new WaitForSecondsRealtime(SettleTimeSeconds);

            using (Measure.ProfilerMarkers(SamplerNames)) {
                GameObject SceneObjects = null;
                var objects = scene.GetRootGameObjects();
                for (int i=0; i<objects.Length; i++) {
                    if (objects[i].name == "SceneObjects") {
                        SceneObjects = objects[i];
                        break;
                    }
                }
                if (SceneObjects == null) {
                    throw new MissingComponentException();
                } 

                var data_selector = SceneObjects.GetComponentInChildren<DataSelector>();
                var camera = Camera.main;

                SelectionMatrix selection_matrix;
                SampleGroup samplegroup;

                for (int i=0; i<dataSets.Length; i++) {
                    samplegroup = new SampleGroup("DataSet" + "test");
                    selection_matrix = data_selector.loadFile(new string[] {dataSets[i]});
                    FixateCamera(
                        new Vector3(0f, 0f, -3f),
                        selection_matrix.visualizer.CenterPosition
                    );

                    yield return Measure.Frames()
                        .WarmupCount(50)
                        .MeasurementCount(50)
                        .ProfilerMarkers(SamplerNames)
                        .SampleGroup(selection_matrix.visualizer.visualizer_name)
                        .Run();

                    selection_matrix.CloseVisualization();
                }
            }
        }
    }
}

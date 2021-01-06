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

namespace Tests
{
    public class CustomException : Exception
    {
    }
    public class NewTestScript
    {
        private readonly string basicSceneName = "Assets/MyDataVisualizer/Scenes/DefaultScene.unity";
        protected readonly float SettleTimeSeconds = 2f;

        protected string[] SamplerNames = {
            "Camera.Render",
            "Render.Mesh",
        };

        [SetUp]
        public void Setup() {
#if ENABLE_VR

#endif
        }
        [UnityTest, Performance]
        public IEnumerator NewPerformanceTest() {
            var camera = Camera.main.gameObject;
            yield return Measure.Frames().Run();
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
                } else {
                    SceneObjects.GetComponentInChildren<DataSelector>();
                    var camera = Camera.main;
                }
            }


            yield return Measure.Frames()
                .WarmupCount(3)
                .MeasurementCount(10)
                .ProfilerMarkers(SamplerNames)
                .Run();


        }

        [UnityTest, Performance]
        public IEnumerator SingleVisualizerEditor() {
            int sceneCount = EditorSceneManager.sceneCount;
            Scene [] scenes = new Scene[sceneCount];
            for (int i=0; i<sceneCount; i++) {
                scenes[i] = EditorSceneManager.GetSceneAt(i);
            }
            var scene = EditorSceneManager.OpenScene(basicSceneName);

            EditorSceneManager.SetActiveScene(scene);

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
                } else {
                    SceneObjects.GetComponentInChildren<DataSelector>();
                    var camera = Camera.main;
                }
            }


            yield return Measure.Frames()
                .WarmupCount(3)
                .MeasurementCount(10)
                .ProfilerMarkers(SamplerNames)
                .Run();

        }
    }
}

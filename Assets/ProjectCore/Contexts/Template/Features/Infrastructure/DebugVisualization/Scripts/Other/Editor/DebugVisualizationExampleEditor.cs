#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectCore.Template
{
    public static class DebugVisualizationExampleEditor
    {
        [MenuItem("Tools/Debug Visualization/Create Example Objects")]
        public static void CreateExampleObjects()
        {
            var root = new GameObject("Debug Visualization Examples");
            Undo.RegisterCreatedObjectUndo(root, "Create Debug Visualization Examples");

            EnsureCameraAndLight();
            CreateGround(root.transform);
            CreateTarget(
                root.transform, "Example Enemy A", new Vector3(-4f, 0.6f, 0f), Color.red,
                100f, 5f, 5.5f, 2.2f, 0.8f);
            CreateTarget(
                root.transform, "Example Enemy B", new Vector3(0f, 0.6f, 2.5f),
                new Color(1f, 0.55f, 0.1f), 80f, 3f, 4f, 1.8f, 1.2f);
            CreateTarget(
                root.transform, "Example Ally", new Vector3(4f, 0.6f, 0f), Color.cyan,
                120f, 7f, 6.5f, 2.8f, 0.6f);
            CreateCollisionDemo(root.transform);

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void CreateTarget(
            Transform parent,
            string objectName,
            Vector3 position,
            Color color,
            float health,
            float regen,
            float vision,
            float attackRange,
            float moveRadius)
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Undo.RegisterCreatedObjectUndo(target, $"Create {objectName}");
            target.name = objectName;
            target.transform.SetParent(parent);
            target.transform.position = position;
            target.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"{objectName} Material", color);

            var debugTarget = target.AddComponent<DebugVisualizationExampleTargetComponent>();
            debugTarget.Configure(health, regen, vision, attackRange, moveRadius);
        }

        private static void CreateCollisionDemo(Transform parent)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(wall, "Create Debug Collision Wall");
            wall.name = "Collision Test Backdrop";
            wall.transform.SetParent(parent);
            wall.transform.position = new Vector3(0f, 0.75f, -4f);
            wall.transform.localScale = new Vector3(7f, 1.5f, 0.35f);
            wall.GetComponent<Renderer>().sharedMaterial =
                CreateMaterial("Collision Wall Material", new Color(0.25f, 0.25f, 0.28f));

            CreateBumper(parent, "Left Collision Bumper", new Vector3(-4.3f, 0.75f, -2.8f));
            CreateBumper(parent, "Right Collision Bumper", new Vector3(4.3f, 0.75f, -2.8f));

            var mover = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Undo.RegisterCreatedObjectUndo(mover, "Create Debug Collision Emitter");
            mover.name = "Collision Damage Emitter";
            mover.transform.SetParent(parent);
            mover.transform.position = new Vector3(0f, 0.6f, -2.8f);
            mover.GetComponent<Renderer>().sharedMaterial =
                CreateMaterial("Collision Emitter Material", Color.magenta);

            var rigidbody = mover.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.constraints = RigidbodyConstraints.FreezePositionY |
                                    RigidbodyConstraints.FreezePositionZ |
                                    RigidbodyConstraints.FreezeRotation;
            mover.AddComponent<DebugVisualizationCollisionEmitterComponent>();
        }

        private static void CreateBumper(Transform parent, string objectName, Vector3 position)
        {
            var bumper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(bumper, $"Create {objectName}");
            bumper.name = objectName;
            bumper.transform.SetParent(parent);
            bumper.transform.position = position;
            bumper.transform.localScale = new Vector3(0.35f, 1.5f, 1.2f);
            bumper.GetComponent<Renderer>().sharedMaterial =
                CreateMaterial($"{objectName} Material", new Color(0.3f, 0.32f, 0.38f));
        }

        private static void CreateGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Undo.RegisterCreatedObjectUndo(ground, "Create Debug Visualization Ground");
            ground.name = "Debug Visualization Ground";
            ground.transform.SetParent(parent);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(1.4f, 1f, 1.4f);
            ground.GetComponent<Renderer>().sharedMaterial =
                CreateMaterial("Debug Ground Material", new Color(0.12f, 0.14f, 0.16f));
        }

        private static void EnsureCameraAndLight()
        {
            if (Camera.main == null)
            {
                var cameraObject = new GameObject("Main Camera");
                Undo.RegisterCreatedObjectUndo(cameraObject, "Create Main Camera");
                var camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.transform.position = new Vector3(0f, 8f, -10f);
                camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            }

            if (Object.FindFirstObjectByType<Light>() == null)
            {
                var lightObject = new GameObject("Directional Light");
                Undo.RegisterCreatedObjectUndo(lightObject, "Create Directional Light");
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }

        private static Material CreateMaterial(string materialName, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                name = materialName,
                color = color
            };

            return material;
        }
    }
}
#endif

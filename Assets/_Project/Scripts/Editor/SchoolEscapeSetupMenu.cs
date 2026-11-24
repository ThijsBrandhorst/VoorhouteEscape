#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using SchoolEscape.Interaction;
using SchoolEscape.Player;
using SchoolEscape.Puzzles;
using SchoolEscape.UI;
using SchoolEscape.World;

namespace SchoolEscape.EditorTools
{
    /// <summary>One-click scene setup and puzzle-station helpers.</summary>
    public static class SchoolEscapeSetupMenu
    {
        [MenuItem("School Escape/Setup/Add Puzzle Systems To Scene")]
        public static void AddPuzzleSystems()
        {
            PlayerMotor motor = Object.FindFirstObjectByType<PlayerMotor>();
            if (motor == null)
            {
                EditorUtility.DisplayDialog("School Escape", "No Player (PlayerMotor) found in the scene. Build the Phase 1 player first.", "OK");
                return;
            }

            GameObject player = motor.gameObject;
            var input = player.GetComponent<PlayerInputReader>();
            var controlLock = player.GetComponent<PlayerControlLock>();
            var look = player.GetComponent<PlayerLook>();
            Camera cam = player.GetComponentInChildren<Camera>(true);
            var headBob = player.GetComponentInChildren<HeadBob>(true);
            Object settings = new SerializedObject(motor).FindProperty("settings").objectReferenceValue;

            // Interactor on the player
            var interactor = GetOrAdd<PlayerInteractor>(player);
            SetRef(interactor, "input", input);
            SetRef(interactor, "controlLock", controlLock);
            SetRef(interactor, "viewCamera", cam);
            var so = new SerializedObject(interactor);
            so.FindProperty("interactionLayers").intValue = player.layer == 0 ? ~0 : ~(1 << player.layer);
            so.ApplyModifiedProperties();

            // Camera effects on the camera
            CameraEffects fx = null;
            if (cam != null)
            {
                fx = GetOrAdd<CameraEffects>(cam.gameObject);
                SetRef(fx, "motor", motor);
                SetRef(fx, "settings", settings);
                SetRef(fx, "targetCamera", cam);
            }

            // GameSystems object
            GameObject systems = GameObject.Find("GameSystems");
            if (systems == null)
            {
                systems = new GameObject("GameSystems");
                Undo.RegisterCreatedObjectUndo(systems, "Create GameSystems");
            }
            var manager = GetOrAdd<PuzzleManager>(systems);
            var window = GetOrAdd<PuzzleWindow>(systems);
            var hud = GetOrAdd<GameHud>(systems);
            var pause = GetOrAdd<PauseMenu>(systems);

            SetRef(manager, "window", window);
            SetRef(manager, "controlLock", controlLock);
            SetRef(hud, "interactor", interactor);
            SetRef(pause, "controlLock", controlLock);
            SetRef(pause, "look", look);
            SetRef(pause, "headBob", headBob);
            SetRef(pause, "cameraEffects", fx);

            EnsureEventSystem();
            EditorSceneManager.MarkSceneDirty(player.scene);
            Selection.activeGameObject = systems;
            Debug.Log("School Escape: puzzle systems added. Now add a Puzzle Station to any object (GameObject > School Escape).");
        }

        [MenuItem("GameObject/School Escape/Puzzle Station (Test Cube)", false, 10)]
        public static void CreateTestCube()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "PuzzleStation_Cube";
            Undo.RegisterCreatedObjectUndo(cube, "Create Puzzle Station");

            Vector3 pos = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.pivot : Vector3.zero;
            cube.transform.position = pos + Vector3.up * 0.5f;

            var station = Undo.AddComponent<PuzzleStation>(cube);
            AssignDefaultPuzzle(station);
            Selection.activeGameObject = cube;
        }

        [MenuItem("GameObject/School Escape/Add Puzzle Station To Selected", false, 11)]
        public static void AddStationToSelected()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                if (go.GetComponent<PuzzleStation>() != null) continue;
                var station = Undo.AddComponent<PuzzleStation>(go);
                AssignDefaultPuzzle(station);
            }
        }

        [MenuItem("GameObject/School Escape/Add Puzzle Station To Selected", true)]
        private static bool ValidateAddStation() => Selection.gameObjects.Length > 0;

        // ------------------------------------------------------------ helpers

        private static void AssignDefaultPuzzle(PuzzleStation station)
        {
            PuzzleDefinition def = Selection.activeObject as PuzzleDefinition;
            if (def == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:QuestionPuzzleDefinition");
                if (guids.Length > 0) def = AssetDatabase.LoadAssetAtPath<PuzzleDefinition>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            if (def != null) SetRef(station, "puzzle", def);
            else Debug.LogWarning("No puzzle assets found. Use School Escape > Create Sample Dutch Puzzles first, then drag one into the station.");
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            return c != null ? c : Undo.AddComponent<T>(go);
        }

        private static void SetRef(Object target, string field, Object value)
        {
            if (target == null) return;
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"Field '{field}' not found on {target.GetType().Name}."); return; }
            p.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        private static void EnsureEventSystem()
        {
            EventSystem es = Object.FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
                return;
            }
            // The old input module throws errors when only the new Input System is active.
            var legacy = es.GetComponent<StandaloneInputModule>();
            if (legacy != null) Undo.DestroyObjectImmediate(legacy);
            if (es.GetComponent<InputSystemUIInputModule>() == null) Undo.AddComponent<InputSystemUIInputModule>(es.gameObject);
        }
    }
}
#endif

using UnityEngine;
using System.Collections.Generic;

namespace RTG
{
    public class PostObjectSpawnAction : IUndoRedoAction
    {
        private bool _cleanupOnRemovedFromStack;
        private List<GameObject> _spawnedParents = new List<GameObject>();

        // MODIFIED: Parallel list to remember whether a spawned parent (or one of its children)
        // was selected at the moment of Undo, so we can restore selection on Redo.
        private List<bool> _wasSelectedFlags = new List<bool>();

        public PostObjectSpawnAction(List<GameObject> spawnedParents)
        {
            _spawnedParents = new List<GameObject>(spawnedParents);
        }

        public void Execute()
        {
            RTUndoRedo.Get.RecordAction(this);
        }

        // MODIFIED
        public void Undo()
        {
            if (_spawnedParents == null) return;

            // Сheck if our custom SelectionManager is present in the scene
            var selMgr = SelectionManager.Instance;

            _wasSelectedFlags.Clear();

            foreach (var parent in _spawnedParents)
            {
                bool wasSelected = false;

                if (parent != null)
                {
                    if (selMgr != null && selMgr.Current != null)
                    {
                        GameObject currentGO = selMgr.Current.gameObject;

                        if (currentGO == parent || currentGO.transform.IsChildOf(parent.transform))
                        {
                            wasSelected = true;
                            selMgr.DeselectCurrent();
                        }
                    }

                    parent.SetActive(false);
                }

                _wasSelectedFlags.Add(wasSelected);
            }

            _cleanupOnRemovedFromStack = true;
        }

        // MODIFIED
        public void Redo()
        {
            if (_spawnedParents == null) return;

            var selMgr = SelectionManager.Instance;

            for (int i = 0; i < _spawnedParents.Count; i++)
            {
                var parent = _spawnedParents[i];
                if (parent != null)
                {
                    parent.SetActive(true);

                    // Restore the selection only if there is a custom SelectionManager
                    if (selMgr != null && _wasSelectedFlags[i])
                    {
                        var selectable = parent.GetComponentInChildren<SelectableObject>();
                        if (selectable != null)
                        {
                            selMgr.Select(selectable);
                        }
                    }
                }
            }

            _cleanupOnRemovedFromStack = false;
        }

        // MODIFIED
        public void OnRemovedFromUndoRedoStack()
        {
            if (_cleanupOnRemovedFromStack && _spawnedParents.Count != 0)
            {
                foreach (var parent in _spawnedParents) GameObject.Destroy(parent);
                _spawnedParents.Clear();
                _wasSelectedFlags.Clear();
            }
        }
    }

    public class PostObjectTransformsChangedAction : IUndoRedoAction
    {
        private List<LocalTransformSnapshot> _preChangeTransformSnapshots = new List<LocalTransformSnapshot>();
        private List<LocalTransformSnapshot> _postChangeTransformSnapshots = new List<LocalTransformSnapshot>();

        public PostObjectTransformsChangedAction(List<LocalTransformSnapshot> preChangeTransformSnapshots,
                                                 List<LocalTransformSnapshot> postChangeTransformSnapshots)
        {
            _preChangeTransformSnapshots = new List<LocalTransformSnapshot>(preChangeTransformSnapshots);
            _postChangeTransformSnapshots = new List<LocalTransformSnapshot>(postChangeTransformSnapshots);
        }

        public void Execute()
        {
            RTUndoRedo.Get.RecordAction(this);
        }

        public void Undo()
        {
            foreach (var snapshot in _preChangeTransformSnapshots)
            {
                snapshot.Apply();
            }
        }

        public void Redo()
        {
            foreach (var snapshot in _postChangeTransformSnapshots)
            {
                snapshot.Apply();
            }
        }

        public void OnRemovedFromUndoRedoStack()
        {
        }
    }

    public class PostGizmoTransformsChangedAction : IUndoRedoAction
    {
        private List<LocalGizmoTransformSnapshot> _preChangeTransformSnapshots = new List<LocalGizmoTransformSnapshot>();
        private List<LocalGizmoTransformSnapshot> _postChangeTransformSnapshots = new List<LocalGizmoTransformSnapshot>();

        public PostGizmoTransformsChangedAction(List<LocalGizmoTransformSnapshot> preChangeTransformSnapshots,
                                                List<LocalGizmoTransformSnapshot> postChangeTransformSnapshots)
        {
            _preChangeTransformSnapshots = new List<LocalGizmoTransformSnapshot>(preChangeTransformSnapshots);
            _postChangeTransformSnapshots = new List<LocalGizmoTransformSnapshot>(postChangeTransformSnapshots);
        }

        public void Execute()
        {
            RTUndoRedo.Get.RecordAction(this);
        }

        public void Undo()
        {
            foreach (var snapshot in _preChangeTransformSnapshots)
            {
                snapshot.Apply();
            }
        }

        public void Redo()
        {
            foreach (var snapshot in _postChangeTransformSnapshots)
            {
                snapshot.Apply();
            }
        }

        public void OnRemovedFromUndoRedoStack()
        {
        }
    }

    public class DuplicateObjectsAction : IUndoRedoAction
    {
        private List<GameObject> _rootsToDuplicate;
        private List<GameObject> _duplicateResult = new List<GameObject>();
        private bool _cleanupOnRemovedFromStack;

        public List<GameObject> DuplicateResult { get { return new List<GameObject>(_duplicateResult); } }

        public DuplicateObjectsAction(List<GameObject> rootsToDuplicate)
        {
            _rootsToDuplicate = GameObjectEx.FilterParentsOnly(rootsToDuplicate);
        }

        public void Execute()
        {
            if (_rootsToDuplicate.Count != 0)
            {
                var cloneConfig = ObjectCloning.DefaultConfig;

                foreach (var root in _rootsToDuplicate)
                {
                    Transform rootTransform = root.transform;
                    cloneConfig.Layer = root.layer;
                    cloneConfig.Parent = rootTransform.parent;

                    GameObject clonedRoot = ObjectCloning.CloneHierarchy(root, cloneConfig);
                    _duplicateResult.Add(clonedRoot);
                }

                RTUndoRedo.Get.RecordAction(this);
            }
        }

        public void Undo()
        {
            if (_duplicateResult != null)
            {
                foreach (var duplicateRoot in _duplicateResult)
                {
                    duplicateRoot.SetActive(false);
                }
                _cleanupOnRemovedFromStack = true;
            }
        }

        public void Redo()
        {
            if (_duplicateResult != null)
            {
                foreach (var duplicateRoot in _duplicateResult)
                {
                    duplicateRoot.SetActive(true);
                }
                _cleanupOnRemovedFromStack = false;
            }
        }

        public void OnRemovedFromUndoRedoStack()
        {
            if (_cleanupOnRemovedFromStack && _duplicateResult.Count != 0)
            {
                foreach (var duplicateRoot in _duplicateResult) GameObject.Destroy(duplicateRoot);
                _duplicateResult.Clear();
            }
        }
    }
}

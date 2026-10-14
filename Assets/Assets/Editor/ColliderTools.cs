
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class ColliderTools
{
    [MenuItem("Tools/Colliders/Remove Box Colliders")]
    private static void RemoveBoxColliders()
    {
        RemoveColliders(1);
    }

    [MenuItem("Tools/Colliders/Remove Capsule Colliders")]
    private static void RemoveCapsuleColliders()
    {
        RemoveColliders(2);
    }

    [MenuItem("Tools/Colliders/Remove Box + Capsule Colliders")]
    private static void RemoveBothColliders()
    {
        RemoveColliders(3);
    }

    [MenuItem("Tools/Colliders/Remove ALL Colliders")]
    private static void RemoveAllColliders()
    {
        RemoveColliders(4);
    }

    private static void RemoveColliders(int mode)
    {
        // Use only top-level selected objects.
        GameObject[] selectedObjects = Selection.gameObjects;

        HashSet<Collider> collidersToRemove = new HashSet<Collider>();

        foreach (GameObject obj in selectedObjects)
        {
            // Search all children, including inactive objects.
            Collider[] colliders =
                obj.GetComponentsInChildren<Collider>(true);

            foreach (Collider col in colliders)
            {
                bool shouldRemove = false;

                switch (mode)
                {
                    case 1:
                        shouldRemove = col is BoxCollider;
                        break;

                    case 2:
                        shouldRemove = col is CapsuleCollider;
                        break;

                    case 3:
                        shouldRemove =
                            col is BoxCollider ||
                            col is CapsuleCollider;
                        break;

                    case 4:
                        shouldRemove = true;
                        break;
                }

                if (shouldRemove)
                {
                    collidersToRemove.Add(col);
                }
            }
        }

        // Show confirmation before removing anything.
        bool confirmed = EditorUtility.DisplayDialog(
            "Remove Colliders?",
            $"Remove {collidersToRemove.Count} colliders?",
            "Remove",
            "Cancel"
        );

        if (!confirmed)
            return;

        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Remove Selected Colliders");

        foreach (Collider col in collidersToRemove)
        {
            if (col != null)
                Undo.DestroyObjectImmediate(col);
        }

        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log(
            $"Successfully removed {collidersToRemove.Count} colliders."
        );
    }
}

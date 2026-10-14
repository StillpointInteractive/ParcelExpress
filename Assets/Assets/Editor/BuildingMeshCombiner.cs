
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

public static class BuildingMeshCombiner
{
    // Keep combined meshes reasonably sized for culling.
    private const int MaxVerticesPerMesh = 60000;

    private class MeshEntry
    {
        public MeshRenderer renderer;
        public MeshFilter filter;
    }

    private static int combinedCount;
    private static int replacedCount;

    [MenuItem("Tools/Buildings/Combine Selected Building")]
    public static void CombineSelectedBuilding()
    {
        GameObject original = Selection.activeGameObject;

        if (original == null ||
            EditorUtility.IsPersistent(original))
        {
            EditorUtility.DisplayDialog(
                "No Building Selected",
                "Select a building in the scene Hierarchy.",
                "OK"
            );
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning(
                "Exit Play Mode before combining meshes."
            );
            return;
        }

        // Create output folder.
        const string baseFolder = "Assets/CombinedBuildings";

        if (!AssetDatabase.IsValidFolder(baseFolder))
        {
            AssetDatabase.CreateFolder(
                "Assets",
                "CombinedBuildings"
            );
        }

        string safeName = string.Concat(
            original.name.Select(c =>
                char.IsLetterOrDigit(c) ||
                c == '_' || c == '-'
                    ? c
                    : '_')
        );

        string outputFolder =
            AssetDatabase.GenerateUniqueAssetPath(
                baseFolder + "/" + safeName
            );

        string folderName = outputFolder.Substring(
            outputFolder.LastIndexOf('/') + 1
        );

        AssetDatabase.CreateFolder(
            baseFolder,
            folderName
        );

        // Duplicate building.
        GameObject copy = UnityEngine.Object.Instantiate(
            original,
            original.transform.parent
        );

        copy.name = original.name + "_Optimized";

        copy.transform.SetSiblingIndex(
            original.transform.GetSiblingIndex() + 1
        );

        Undo.RegisterCreatedObjectUndo(
            copy,
            "Create Optimized Building"
        );

        // Unpack root prefab instance if necessary.
        if (PrefabUtility.IsAnyPrefabInstanceRoot(copy))
        {
            PrefabUtility.UnpackPrefabInstance(
                copy,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction
            );
        }

        combinedCount = 0;
        replacedCount = 0;

        // Track renderers assigned to any LOD Group.
        HashSet<MeshRenderer> lodRenderers =
            new HashSet<MeshRenderer>();

        LODGroup[] lodGroups =
            copy.GetComponentsInChildren<LODGroup>(true);

        // Count memberships to avoid modifying shared LOD renderers.
        Dictionary<MeshRenderer, int> membershipCount =
            new Dictionary<MeshRenderer, int>();

        foreach (LODGroup group in lodGroups)
        {
            foreach (LOD lod in group.GetLODs())
            {
                foreach (Renderer renderer in lod.renderers)
                {
                    if (renderer is MeshRenderer mr)
                    {
                        lodRenderers.Add(mr);

                        if (!membershipCount.ContainsKey(mr))
                            membershipCount[mr] = 0;

                        membershipCount[mr]++;
                    }
                }
            }
        }

        try
        {
            // Process each LOD level separately.
            foreach (LODGroup group in lodGroups)
            {
                LOD[] levels = group.GetLODs();
                bool changed = false;

                for (int level = 0;
                     level < levels.Length;
                     level++)
                {
                    List<MeshRenderer> candidates =
                        levels[level].renderers
                            .OfType<MeshRenderer>()
                            .Where(r =>
                                r != null &&
                                r.enabled &&
                                r.gameObject.activeInHierarchy &&
                                membershipCount[r] == 1)
                            .ToList();

                    HashSet<Renderer> replaced =
                        new HashSet<Renderer>();

                    List<Renderer> generated =
                        CombineRenderers(
                            candidates,
                            group.transform,
                            outputFolder,
                            "LOD_" + group.GetInstanceID()
                                + "_" + level,
                            replaced
                        );

                    if (generated.Count == 0)
                        continue;

                    changed = true;

                    // Preserve all original LOD entries
                    // except the renderers we successfully replaced.
                    List<Renderer> finalRenderers =
                        levels[level].renderers
                            .Where(r =>
                                r != null &&
                                !replaced.Contains(r))
                            .ToList();

                    finalRenderers.AddRange(generated);

                    levels[level].renderers =
                        finalRenderers.ToArray();
                }

                if (changed)
                {
                    group.SetLODs(levels);
                    group.RecalculateBounds();
                    EditorUtility.SetDirty(group);
                }
            }

            // Process meshes outside any LOD Group.
            List<MeshRenderer> nonLOD =
                copy.GetComponentsInChildren<MeshRenderer>(true)
                    .Where(r =>
                        !lodRenderers.Contains(r) &&
                        r.enabled &&
                        r.gameObject.activeInHierarchy)
                    .ToList();

            CombineRenderers(
                nonLOD,
                copy.transform,
                outputFolder,
                "NonLOD",
                new HashSet<Renderer>()
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = copy;

            Debug.Log(
                "Building Mesh Combiner finished!\n" +
                "Original renderers replaced: " +
                replacedCount + "\n" +
                "Combined renderers created: " +
                combinedCount + "\n" +
                "Meshes saved in: " + outputFolder
            );

            EditorUtility.DisplayDialog(
                "Optimization Complete",
                "An optimized building copy was created.\n\n" +
                "Renderers replaced: " + replacedCount +
                "\nCombined renderers created: " +
                combinedCount +
                "\n\nDisable the original building " +
                "before comparing performance.",
                "OK"
            );
        }
        catch (Exception e)
        {
            Debug.LogException(e);

            EditorUtility.DisplayDialog(
                "Mesh Combining Error",
                "The process encountered an error.\n" +
                "Check the Console.\n\n" +
                "Your original building has not been modified.",
                "OK"
            );
        }
    }

    private static List<Renderer> CombineRenderers(
        List<MeshRenderer> renderers,
        Transform parent,
        string folder,
        string prefix,
        HashSet<Renderer> replaced)
    {
        List<Renderer> output = new List<Renderer>();

        // Each group must use compatible rendering settings.
        Dictionary<string, List<MeshEntry>> groups =
            new Dictionary<string, List<MeshEntry>>();

        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            MeshFilter filter =
                renderer.GetComponent<MeshFilter>();

            if (filter == null ||
                filter.sharedMesh == null)
                continue;

            Mesh mesh = filter.sharedMesh;

            // Leave non-readable meshes untouched.
            if (!mesh.isReadable)
                continue;

            // Leave baked lightmaps untouched.
            if (renderer.lightmapIndex >= 0)
                continue;

            // Only simple single-material meshes.
            if (mesh.subMeshCount != 1 ||
                renderer.sharedMaterials.Length != 1)
                continue;

            if (mesh.vertexCount > MaxVerticesPerMesh)
                continue;

            Material material = renderer.sharedMaterial;

            if (material == null)
                continue;

            // Avoid merging objects with custom property blocks.
            if (renderer.HasPropertyBlock())
                continue;

            // Skip specially configured lighting and motion.
            if (renderer.lightProbeUsage !=
                    LightProbeUsage.BlendProbes ||
                renderer.reflectionProbeUsage !=
                    ReflectionProbeUsage.BlendProbes)
                continue;

            // Avoid meshes with mirrored transforms.
            if (renderer.transform.localToWorldMatrix
                    .determinant <= 0f)
                continue;

            string key =
                material.GetInstanceID() + "_" +
                renderer.shadowCastingMode + "_" +
                renderer.receiveShadows + "_" +
                renderer.gameObject.layer + "_" +
                renderer.motionVectorGenerationMode + "_" +
                renderer.lightProbeUsage + "_" +
                renderer.reflectionProbeUsage + "_" +
                renderer.sortingLayerID + "_" +
                renderer.sortingOrder;

            if (!groups.ContainsKey(key))
            {
                groups[key] = new List<MeshEntry>();
            }

            groups[key].Add(new MeshEntry
            {
                renderer = renderer,
                filter = filter
            });
        }

        foreach (List<MeshEntry> materialGroup
                 in groups.Values)
        {
            // Split large groups into smaller meshes.
            List<MeshEntry> batch = new List<MeshEntry>();
            int vertexCount = 0;

            foreach (MeshEntry entry in materialGroup)
            {
                int count = entry.filter.sharedMesh.vertexCount;

                if (vertexCount + count > MaxVerticesPerMesh)
                {
                    CreateCombinedMesh(
                        batch,
                        parent,
                        folder,
                        prefix,
                        output,
                        replaced
                    );

                    batch.Clear();
                    vertexCount = 0;
                }

                batch.Add(entry);
                vertexCount += count;
            }

            CreateCombinedMesh(
                batch,
                parent,
                folder,
                prefix,
                output,
                replaced
            );
        }

        return output;
    }

    private static void CreateCombinedMesh(
        List<MeshEntry> entries,
        Transform parent,
        string folder,
        string prefix,
        List<Renderer> output,
        HashSet<Renderer> replaced)
    {
        // Do not combine just one renderer.
        if (entries.Count < 2)
            return;

        List<CombineInstance> instances =
            new List<CombineInstance>();

        foreach (MeshEntry entry in entries)
        {
            CombineInstance instance = new CombineInstance
            {
                mesh = entry.filter.sharedMesh,
                subMeshIndex = 0,
                transform =
                    parent.worldToLocalMatrix *
                    entry.filter.transform.localToWorldMatrix
            };

            instances.Add(instance);
        }

        Mesh combinedMesh = new Mesh
        {
            name = prefix + "_CombinedMesh",
            indexFormat = IndexFormat.UInt32
        };

        try
        {
            combinedMesh.CombineMeshes(
                instances.ToArray(),
                true,
                true,
                false
            );

            combinedMesh.RecalculateBounds();
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "Skipped a mesh group: " + e.Message
            );

            UnityEngine.Object.DestroyImmediate(combinedMesh);
            return;
        }

        string meshPath =
            AssetDatabase.GenerateUniqueAssetPath(
                folder + "/" + prefix +
                "_Mesh_" + combinedCount + ".asset"
            );

        AssetDatabase.CreateAsset(
            combinedMesh,
            meshPath
        );

        // New GameObject uses parent-local mesh coordinates.
        GameObject combinedObject = new GameObject(
            prefix + "_Combined_" + combinedCount
        );

        combinedObject.transform.SetParent(parent, false);

        combinedObject.transform.localPosition = Vector3.zero;
        combinedObject.transform.localRotation =
            Quaternion.identity;
        combinedObject.transform.localScale = Vector3.one;

        MeshRenderer source = entries[0].renderer;

        combinedObject.layer = source.gameObject.layer;

        MeshFilter filter =
            combinedObject.AddComponent<MeshFilter>();

        MeshRenderer renderer =
            combinedObject.AddComponent<MeshRenderer>();

        filter.sharedMesh = combinedMesh;

        renderer.sharedMaterial = source.sharedMaterial;
        renderer.shadowCastingMode =
            source.shadowCastingMode;
        renderer.receiveShadows =
            source.receiveShadows;
        renderer.lightProbeUsage =
            source.lightProbeUsage;
        renderer.reflectionProbeUsage =
            source.reflectionProbeUsage;
        renderer.motionVectorGenerationMode =
            source.motionVectorGenerationMode;
        renderer.sortingLayerID = source.sortingLayerID;
        renderer.sortingOrder = source.sortingOrder;

        // Preserve rendering configuration where supported.
        renderer.allowOcclusionWhenDynamic =
            source.allowOcclusionWhenDynamic;

        // Disable only the replaced Mesh Renderers.
        // Keep original geometry objects and colliders.
        foreach (MeshEntry entry in entries)
        {
            entry.renderer.enabled = false;

            replaced.Add(entry.renderer);
            replacedCount++;
        }

        output.Add(renderer);
        combinedCount++;
    }
}

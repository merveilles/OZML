using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
Attach this script as a parent to some game objects. The script will then combine the meshes at startup.
This is useful as a performance optimization since it is faster to render one big mesh than many small meshes. See the docs on graphics performance optimization for more info.

Different materials will cause multiple meshes to be created, thus it is useful to share as many textures/material as you can.
*/
//[ExecuteInEditMode()]
[AddComponentMenu("Mesh/Combine Children")]
public class CombineChildren : MonoBehaviour
{
    public string combinedMeshName = "Combined Mesh";

    public bool addMeshCollider;
    public bool castShadow = true;

    public bool combineOnStart = true,
                destroyAfterOptimized;

    /// Usually rendering with triangle strips is faster.
    /// However when combining objects with very low triangle counts, it can be faster to use triangles.
    /// Best is to try out which value is faster in practice.
    public int frameToWait;

    public bool generateTriangleStrips = true;

    public bool keepLayer = true;

    public bool receiveShadow = true;

    private void Start()
    {
        if (combineOnStart && frameToWait == 0) Combine();
        else StartCoroutine(CombineLate());
    }

    private IEnumerator CombineLate()
    {
        for (int i = 0; i < frameToWait; i++) yield return 0;
        Combine();
    }

    [ContextMenu("Combine Now on Childs")]
    public void CallCombineOnAllChilds()
    {
        CombineChildren[] c = gameObject.GetComponentsInChildren<CombineChildren>();
        int count = c.Length;
        for (int i = 0; i < count; i++) if (c[i] != this) c[i].Combine();
        combineOnStart = enabled = false;
    }

    /// This option has a far longer preprocessing time at startup but leads to better runtime performance.
    [ContextMenu("Combine Now")]
    public void Combine()
    {
        var filters = GetComponentsInChildren<MeshFilter>();
        Matrix4x4 myTransform = transform.worldToLocalMatrix;
        var materialToMesh = new Dictionary<Material, List<MeshCombineUtility.MeshInstance>>();

        foreach (var sourceFilter in filters)
        {
            Renderer curRenderer = sourceFilter.renderer;
            if (curRenderer == null || !curRenderer.enabled) continue;

            var instance = new MeshCombineUtility.MeshInstance
                               {
                                   mesh = sourceFilter.sharedMesh,
                                   transform = myTransform * sourceFilter.transform.localToWorldMatrix
                               };
            if(instance.mesh == null) continue;

            Material[] materials = curRenderer.sharedMaterials;
            for (int m = 0; m < materials.Length; m++)
            {
                instance.subMeshIndex = Math.Min(m, instance.mesh.subMeshCount - 1);

                List<MeshCombineUtility.MeshInstance> objects;
                if (!materialToMesh.TryGetValue(materials[m], out objects))
                {
                    objects = new List<MeshCombineUtility.MeshInstance>();
                    materialToMesh.Add(materials[m], objects);   
                }
                objects.Add(instance);
            }

            if (Application.isPlaying && destroyAfterOptimized && combineOnStart) 
                Destroy(curRenderer.gameObject);
            else if (destroyAfterOptimized) 
                DestroyImmediate(curRenderer.gameObject);
            else 
                curRenderer.enabled = false;
        }

        int targetMeshIndex = 0;
        foreach(var de in materialToMesh)
        {
            foreach (var instance in de.Value)
                instance.targetSubMeshIndex = targetMeshIndex;
            ++targetMeshIndex;
        }

        if (!GetComponent<MeshFilter>()) gameObject.AddComponent<MeshFilter>();
        var filter = GetComponent<MeshFilter>();
        if (!GetComponent<MeshRenderer>()) gameObject.AddComponent<MeshRenderer>();

        var mesh = MeshCombineUtility.Combine(materialToMesh.SelectMany(kvp => kvp.Value), generateTriangleStrips);
        mesh.name = combinedMeshName;
        if (Application.isPlaying) filter.mesh = mesh;
        else filter.sharedMesh = mesh;
        renderer.materials = materialToMesh.Keys.ToArray();
        renderer.enabled = true;
        if (addMeshCollider) gameObject.AddComponent<MeshCollider>();
        renderer.castShadows = castShadow;
        renderer.receiveShadows = receiveShadow;
    }
}
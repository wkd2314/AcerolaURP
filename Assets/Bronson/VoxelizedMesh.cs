using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VoxelizedMesh : MonoBehaviour
{
    public List<Vector3Int> GridPoints = new List<Vector3Int>();
    public float HalfSize = 0.1f;
    public Vector3 LocalOrigin;

    public Vector3 PointToPosition(Vector3Int point)
    {
        float size = HalfSize * 2f;
        Vector3 pos = new Vector3(HalfSize + point.x * size, HalfSize + point.y * size,
            HalfSize + point.z * size);
        return transform.TransformPoint(LocalOrigin + pos);
    }

    [MenuItem("Voxels/Voxelize Meshes")]
    public static void VoxelizeMeshesWithTag()
    {
        var objects = GameObject.FindGameObjectsWithTag("Voxelize");
        foreach (var obj in objects)
        {
            if(obj.TryGetComponent(out MeshFilter meshFilter))
                VoxelizeMesh(meshFilter);
        }
    }
    
    /// <summary>
    /// adds object information to GridPoints on VoxelizedMesh Class.
    /// </summary>
    /// <param name="meshFilter"></param>
    public static void VoxelizeMesh(MeshFilter meshFilter)
    {
        if (!meshFilter.TryGetComponent(out MeshCollider meshCollider))
        {
            meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
        }
        if (!meshFilter.TryGetComponent(out VoxelizedMesh voxelizedMesh))
        {
            voxelizedMesh = meshFilter.gameObject.AddComponent<VoxelizedMesh>();
        }

        Bounds bounds = meshCollider.bounds;
        // extents = half the size of bounding box
        Vector3 minExtents = bounds.center - bounds.extents;
        float halfSize = voxelizedMesh.HalfSize;
        Vector3 count = bounds.extents / halfSize;

        int xMax = Mathf.CeilToInt(count.x);
        int yMax = Mathf.CeilToInt(count.y);
        int zMax = Mathf.CeilToInt(count.z);

        voxelizedMesh.GridPoints.Clear();
        // set local origin to left bottom corner
        voxelizedMesh.LocalOrigin = voxelizedMesh.transform.InverseTransformPoint(minExtents);
        for (int x = 0; x < xMax; x++)
        {
            for (int y = 0; y < yMax; y++)
            {
                for (int z = 0; z < zMax; z++)
                {
                    Vector3 pos = voxelizedMesh.PointToPosition(new Vector3Int(x, y, z));
                    // true if overlaps with any colliders.
                    if (Physics.CheckBox(pos, new Vector3(halfSize, halfSize, halfSize)))
                    {
                        voxelizedMesh.GridPoints.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }
}

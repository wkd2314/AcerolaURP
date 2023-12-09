using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneVoxelizer))]
public class VoxelizerEditor : Editor
{
    private void OnSceneGUI()
    {
        SceneVoxelizer voxelizer = target as SceneVoxelizer;
        
        Handles.color = Color.cyan;
        
        if(voxelizer)
            Handles.DrawWireCube(voxelizer.transform.localPosition, voxelizer.GetBoundExtent() * 2);
    }
}

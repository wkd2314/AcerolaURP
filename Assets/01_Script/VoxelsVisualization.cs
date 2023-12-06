using System;
using UnityEngine;
using UnityEngine.Rendering;
using static System.Runtime.InteropServices.Marshal;

[ExecuteAlways]
public class VoxelsVisualization : MonoBehaviour
{
    // compute position and send to vertex shader.
    [SerializeField] private ComputeShader initializeVoxelsShader;
    [SerializeField] private Mesh instancedMesh;
    [SerializeField] private Vector3Int boundVector = new Vector3Int(30, 5, 30);
    [SerializeField] private Vector3 ellipsoidSize = new Vector3(1, 1, 1);
    [SerializeField][Range(0.01f, 1)] private float voxelSize = 0.5f;

    private int instanceCount;
    private Bounds bounds;
    private Material instancedMaterial;

    private ComputeBuffer voxelsBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    
    private static readonly int Voxels = Shader.PropertyToID("_Voxels");
    private static readonly int BoundAndSize = Shader.PropertyToID("_BoundAndSize");
    private static readonly int VoxelsBuffer = Shader.PropertyToID("_VoxelsBuffer");
    private Camera mainCam;
    private Vector3 smokeOrigin;

    private struct VoxelData
    {
        public Vector3 position;
        public Vector3 color;
    }

    private void Start()
    {
        mainCam = Camera.main;
    }
    private void OnEnable()
    {
        if (!instancedMaterial) instancedMaterial = CoreUtils.CreateEngineMaterial("Hidden/Voxel");
        
        Vector3Int instanceBound = Vector3Int.FloorToInt((Vector3)boundVector / voxelSize);
        instanceCount = instanceBound.x * instanceBound.y * instanceBound.z;
        voxelsBuffer = new ComputeBuffer(instanceCount, SizeOf(typeof(VoxelData)));
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        bounds = new Bounds(smokeOrigin, boundVector);

        Vector4 boundAndSize = (Vector3)instanceBound;
        boundAndSize.w = voxelSize;
        
        Shader.SetGlobalVector("_SmokeOrigin", smokeOrigin);
        
        initializeVoxelsShader.SetBuffer(0, Voxels, voxelsBuffer);
        initializeVoxelsShader.SetVector(BoundAndSize, boundAndSize);
        initializeVoxelsShader.SetVector("_EllipsoidSize", ellipsoidSize);
        initializeVoxelsShader.Dispatch(0, Mathf.CeilToInt(instanceCount / 256.0f), 1, 1);
        
        // https://docs.unity3d.com/kr/current/ScriptReference/Graphics.DrawMeshInstancedIndirect.html
        uint[] args = new uint[5];
        // Indirect args
        if (instancedMesh) {
            args[0] = (uint)instancedMesh.GetIndexCount(0);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instancedMesh.GetIndexStart(0);
            args[3] = (uint)instancedMesh.GetBaseVertex(0);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        
        argsBuffer.SetData(args);
        
        instancedMaterial.SetBuffer(VoxelsBuffer, voxelsBuffer);
        instancedMaterial.SetFloat("_VoxelSize", voxelSize);
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            OnDisable();
            OnEnable();

            smokeOrigin = transform.position;
        
            if(instancedMaterial)
                Graphics.DrawMeshInstancedIndirect(instancedMesh, 0, instancedMaterial, bounds, argsBuffer);
        }
        else
        {
            // actual playmode
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out var hitInfo))
                {
                    smokeOrigin = hitInfo.point;
                }
            }

            if (smokeOrigin != Vector3.zero)
            {
                Shader.SetGlobalVector("_SmokeOrigin", smokeOrigin);
                Graphics.DrawMeshInstancedIndirect(instancedMesh, 0, instancedMaterial, bounds, argsBuffer);
            }
        }
    }

    private void OnDisable()
    {
        argsBuffer?.Release();
        voxelsBuffer?.Release();
        if(instancedMaterial) CoreUtils.Destroy(instancedMaterial);
    }
} 

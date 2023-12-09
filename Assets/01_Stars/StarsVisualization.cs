using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using static System.Runtime.InteropServices.Marshal;

namespace _01Scripts.VisualizationScripts
{
    // https://docs.unity3d.com/kr/current/ScriptReference/Graphics.DrawMeshInstancedIndirect.html
    [ExecuteInEditMode]
    public class StarsVisualization : MonoBehaviour
    {
        [FormerlySerializedAs("StarDensity")]
        [Range(0, 3400)] [Tooltip("Need limit because of compute buffer size.")]
        [SerializeField] private int starDensity = 100;
        [FormerlySerializedAs("BoundRadius")]
        [SerializeField] private float boundRadius = 10;
        [FormerlySerializedAs("StarScale")]
        [SerializeField] private float starScale = 1f;
        
        [Header("Explode Setting")]
        [SerializeField] private float expandSpeed = 3f;
        [SerializeField] private float expandRadius = 70;
        

        public Material starMaterial;

        public Mesh starMesh;

        public ComputeShader initializeStarsShader;
        private ComputeBuffer starsBuffer, argsBuffer;

        private Bounds bounds;

        private int layerIdx;
        
        private struct StarData
        {
            public Vector4 position;
            public Matrix4x4 rsMat;
        }

        private void OnEnable()
        {
            starsBuffer = new ComputeBuffer(starDensity, SizeOf(typeof(StarData)));
            argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            bounds = new Bounds(Vector3.zero, new Vector3(500.0f, 200.0f, 500.0f));

            SetShaderValue();
        }

        void SetShaderValue()
        {
            initializeStarsShader.SetBuffer(0, "_Stars", starsBuffer);
            initializeStarsShader.SetFloat("_Radius", boundRadius);
            initializeStarsShader.SetFloat("_StarScale", starScale);
            initializeStarsShader.Dispatch(0, Mathf.CeilToInt(starDensity/256.0f), 1,1);

            // https://rito15.github.io/posts/unity-compute-buffer-gpu-instancing/
            uint[] args = new uint[5];
            args[0] = (uint)starMesh.GetIndexCount(0);
            args[1] = (uint)starsBuffer.count;
            args[2] = (uint)starMesh.GetIndexStart(0);
            args[3] = (uint)starMesh.GetBaseVertex(0);
            
            argsBuffer.SetData(args);
            
            starMaterial.SetBuffer("_StarsBuffer", starsBuffer);
        }
        

        private void Update()
        {
            if (!Application.isPlaying)
            {
                OnDisable();
                OnEnable();
                Graphics.DrawMeshInstancedIndirect(starMesh, 0, starMaterial, bounds, argsBuffer);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    boundRadius = 1f;
                    if (boundRadius < expandRadius)
                    {
                        boundRadius = 1;
                        boundRadius = Mathf.Lerp(boundRadius, expandRadius, Time.deltaTime * expandSpeed);
                        SetShaderValue();
                    }
                }
                Graphics.DrawMeshInstancedIndirect(starMesh, 0, starMaterial, bounds, argsBuffer);
            }
            GC.Collect();
        }

        private void OnDisable()
        {
            starsBuffer?.Release();
            argsBuffer?.Release();
            starsBuffer = null;
            argsBuffer = null;
        }
    }
}
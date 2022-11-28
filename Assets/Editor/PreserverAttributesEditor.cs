using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VoxelSystem;

[CustomEditor(typeof(PreserveAttributes))]
public class PreserverAttributesEditor : Editor
{
    private PreserveAttributes _manager;
    private MeshFilter[] _meshFilters;
    private MultiValueVoxelModel _voxelColorModel;

    private void OnEnable()
    {
        _manager = (PreserveAttributes)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Run"))
        {
            if (_meshFilters == null || _meshFilters.Length == 0)
            {
                _meshFilters = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];
            }

            if (AssetDatabase.LoadAssetAtPath("Assets/Scripts/Shaders/Voxelizer.compute", typeof(ComputeShader)) ==
                null)
            {
                throw new FileLoadException("Voxelizer compute shader is not present");
            }

            _manager.Voxelizer = (ComputeShader)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Shaders/Voxelizer.compute",
                    typeof(ComputeShader));

            if (_voxelColorModel == null)
            {
                _voxelColorModel = VoxelFunctions.GetMultiValueVoxelData(_meshFilters, _manager.Voxelizer,
                        _manager.VoxelSize, VoxelizationGeomType.surface);


                Debug.Log("Voxels are successfully created!");
            }

            var PreNeighbours = GetPreNeighbours(_voxelColorModel);

            if (_manager.VisualizeVoxels)
            {
                VfxFunctions.VisualiseVfxColorVoxels(_voxelColorModel, _manager.VoxelSize, VoxelVisualizationType.quad);
            }

        }
    }

    public static List<int> GetPreNeighbours(MultiValueVoxelModel voxelModel)
    {
        var SizeX = voxelModel.Width;
        var SizeY = voxelModel.Height;
        var SizeZ = voxelModel.Depth;
        var NavigableVoxelsIds = new List<int>();

        for (int i = 0; i < voxelModel.Voxels.Length; i++)
        {
            var voxel = voxelModel.Voxels[i];
            if (voxel != null)
            {
                var index3D = ArrayFunctions.Index1DTo3D(i, voxelModel.Width, voxelModel.Height);
                var neighbours = new int[5, 5, 5];                 

                for (int i1 = -2; i1 < 3; i1++)      
                {
                    for (int j1 = 1; j1 < 6; j1++)
                    {
                        for (int k1 = -2; k1 < 3; k1++)
                        {
                            var m = index3D.X + i1;
                            var n = index3D.Y + j1;
                            var p = index3D.Z + k1;
                            if (m < 0)    
                            m = 0;
                            if (m > SizeX)
                            m = SizeX;
                            if (n > SizeY)
                                n = SizeY;
                            if (p < 0)
                            p = 0;
                            if (p > SizeZ)
                                p = SizeZ;
                            var neighboursId = ArrayFunctions.Index3DTo1D(m, n, p, voxelModel.Width, voxelModel.Height);
                            //Why the largest Id value of the voxelModel outnumbers the voxelModel.Voxels.Length? My intention: LargestneighboursId = ArrayFunctions.Index3DTo1D(SizeX, SizeY, SizeZ, voxelModel.Width, voxelModel.Height)
                            if (neighboursId >= voxelModel.Voxels.Length)
                                neighboursId = voxelModel.Voxels.Length - 1;  // At this moment, I try that the largestNeighboursId equals to "voxelModel.Voxels.Length - 1".
                            neighbours[i1+2, j1 - 1, k1+2] = neighboursId;
                        }
                    }
                }

                bool AllNeighboursIsNavigable = true;

                foreach (int neighboursId in neighbours)
                {
                    if (voxelModel.Voxels[neighboursId] != null)                     
                    {
                        AllNeighboursIsNavigable = false;
                        break;
                    }                                                       
                }                                                                     
              
                if (AllNeighboursIsNavigable)
                {
                    foreach (int currentNeighbour in neighbours)
                    {
                        if (!NavigableVoxelsIds.Contains(currentNeighbour))
                            NavigableVoxelsIds.Add(currentNeighbour);
                    }
                }
            }
        }
        return NavigableVoxelsIds;                      
    }
 }














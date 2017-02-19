using System;
using DirectX11;
using MHGameWork.TheWizards.SkyMerchant._Engine.DataStructures;
using UnityEngine;

namespace Assets.MarchingCubes.VoxelWorldMVP
{
    /// <summary>
    /// World generation using a lambda
    /// </summary>
    public class DelegateVoxelWorldGenerator : IWorldGenerator
    {
        private readonly Func<Vector3, float> worldFunction;

        public DelegateVoxelWorldGenerator(Func<Vector3, float> worldFunction)
        {
            this.worldFunction = worldFunction;
        }

        public UniformVoxelData Generate(Point3 start, Point3 chunkSize)
        {
            var ret = new UniformVoxelData();
            ret.Data = new Array3D<float>(chunkSize);
            ret.Data.ForEach((v, p) =>
            {
                ret.Data[p] = worldFunction(p + start);
            });

            return ret;
        }
    }
}
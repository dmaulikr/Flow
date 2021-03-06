using System;
using System.Linq;
using DirectX11;
using MHGameWork.TheWizards;
using UnityEngine;

namespace Assets.MarchingCubes.VoxelWorldMVP
{
    public class SmoothTool : IState
    {
        private VoxelWorldEditorScript script;
        private PlaceSphereState tool;
        private IEditableVoxelWorld world;
        private readonly GameObject sphereGizmo;
        private Vector3 point;

        public string Name
        {
            get { return "Smooth"; }
        }

        public SmoothTool(VoxelWorldEditorScript script, IEditableVoxelWorld world, GameObject sphereGizmo)
        {
            this.script = script;
            this.world = world;
            this.sphereGizmo = sphereGizmo;
            //this.tool = new PlaceSphereState(script, world);
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Update(RaycastHit? raycast)
        {
            if (!raycast.HasValue) return;

            point = raycast.Value.point;

            sphereGizmo.transform.position = point;
            sphereGizmo.transform.localScale = Vector3.one * script.ActiveSize;
            var range = script.ActiveSize;
            var material = script.ActiveMaterial;
            var radius = new Point3(1, 1, 1) * (int)Math.Ceiling(script.ActiveSize);

            if (Input.GetMouseButton(0))
            {
                //var weights = new[] {0.5f, 1f, 0.5f};
                var weights = new[] { 0.1f, 2f, 0.1f };
                weights = weights.Select(f => f / weights.Sum()).ToArray();

                world.RunKernelXbyXUnrolled(point.ToFloored() - radius, point.ToCeiled() + radius, (data, p) =>
                {
                    var val = data[1];
                    //if ((p - point).magnitude <= range)
                    var d = 0f;
                    for (int i = 0; i < weights.Length; i++)
                    {
                        d += Mathf.Clamp(data[i].Density, -1, 1) * weights[i];
                    }
                    {
                        if (d > 0 && val.Material == null)
                            val.Material = material;
                        //val.Material = material;
                        val.Density = d;
                        return val;
                    }
                    return val;
                }, 3, Time.frameCount);



            }

        }

        public void OnDrawGizmos()
        {
        }
    }
}
;
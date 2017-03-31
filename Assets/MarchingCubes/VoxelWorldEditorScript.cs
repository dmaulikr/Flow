﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DirectX11;
using MHGameWork.TheWizards;
using MHGameWork.TheWizards.DualContouring;
using UnityEditor;
using UnityEngine;

namespace Assets.MarchingCubes.VoxelWorldMVP
{
    /// <summary>
    /// Contains logic for converting user input to voxel world edits
    /// Also holds the state for the editor
    /// </summary>
    public class VoxelWorldEditorScript : MonoBehaviour
    {
        private VoxelWorldRaycaster raycaster;

        private VoxelMaterial MaterialGreen = new VoxelWorldMVP.VoxelMaterial() { color = Color.green };
        private VoxelMaterial MaterialRed = new VoxelWorldMVP.VoxelMaterial() { color = Color.red };
        private VoxelMaterial MaterialBlue = new VoxelWorldMVP.VoxelMaterial() { color = Color.blue };

        private IEditableVoxelWorld world;

        public List<VoxelMaterial> VoxelMaterials;
        public VoxelMaterial ActiveMaterial;
        public float ActiveSize = 3;

        public void Start()
        {


        }

        public void Init(IEditableVoxelWorld world)
        {
            this.world = world;

            ActiveMaterial = MaterialGreen;
            VoxelMaterials = new List<VoxelMaterial>(new[] { MaterialGreen, MaterialRed, MaterialBlue });

            tools.Add(KeyCode.Alpha0, new NullState());
            tools.Add(KeyCode.Alpha1, new PlaceSphereState(this, world));
            tools.Add(KeyCode.Alpha2, new PlaceSphereStateMidair(this, world));

            activeState = tools[KeyCode.Alpha1];

            activeState.Start();

            raycaster = new VoxelWorldRaycaster();
        }

        private IState activeState;
        private Dictionary<KeyCode, IState> tools = new Dictionary<KeyCode, IState>();
        public void Update()
        {
            if (this.world == null) return;

            if (Input.mouseScrollDelta.y != 0)
                ActiveSize = changeSize(ActiveSize, Mathf.Sign(Input.mouseScrollDelta.y));

            if (Input.GetKeyDown(KeyCode.KeypadPlus))
                ActiveMaterial = changeMaterial(1);
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
                ActiveMaterial = changeMaterial(-1);



            foreach (var k in tools)
            {
                if (!Input.GetKeyDown(k.Key)) continue;
                activeState.Stop();
                activeState = k.Value;
                activeState.Start();
                break;

            }

            activeState.Update(raycaster.raycast());
        }

        private float changeSize(float currentSize, float v)
        {
            return Mathf.Clamp(currentSize + v, 1, 100);
        }

        private VoxelMaterial changeMaterial(int indexChange)
        {
            return VoxelMaterials[(VoxelMaterials.IndexOf(ActiveMaterial) + indexChange + VoxelMaterials.Count) % VoxelMaterials.Count];
        }

        private Vector3 mod(Vector3 p, Vector3 c)
        {
            return new Vector3(p.x % c.x, p.y % c.y, p.z % c.z);
        }
    }

    internal interface IState
    {
        void Stop();
        void Update(RaycastHit? raycast);
        void Start();
    }

    class PlaceSphereState : IState
    {
        private readonly IEditableVoxelWorld world;

        private WorldEditTool tool = new WorldEditTool();

        private VoxelWorldEditorScript voxelWorldScript;

        public PlaceSphereState(VoxelWorldEditorScript voxelWorldScript, IEditableVoxelWorld world)
        {
            this.voxelWorldScript = voxelWorldScript;
            this.world = world;
        }

        public void Stop()
        {
        }

        public void Update(RaycastHit? raycast)
        {
            if (!raycast.HasValue) return;

            var range = voxelWorldScript.ActiveSize;
            var material = voxelWorldScript.ActiveMaterial;

            var point = raycast.Value.point;
            tryPlaceSphere(range, material, point);

        }

        public void tryPlaceSphere(float range, VoxelMaterial material, Vector3 point)
        {
            var min = (point - new Vector3(1, 1, 1) * range).ToFloored();
            var max = (point + new Vector3(1, 1, 1) * range).ToCeiled();
            var radius = new Point3(1, 1, 1) * (int)Math.Ceiling(range);

            if (Input.GetMouseButtonDown(0))
            {
                world.RunKernel1by1(point.ToFloored() - radius, point.ToCeiled() + radius, WorldEditTool.createAddSphereKernel(point, range, material), Time.frameCount);

                //world.ForChunksInRange(min, max, (p, c) =>
                //{
                //    var offset = p.Multiply(world.ChunkSize);
                //    var localHit = point - offset;

                //    tool.addSphere(c, localHit, range, material);
                //    c.LastChangeFrame = Time.frameCount;

                //});


            }
            else if (Input.GetMouseButtonDown(1))
            {
                world.RunKernel1by1(point.ToFloored() - radius, point.ToCeiled() + radius, WorldEditTool.createRemoveSphereKernel(point, range), Time.frameCount);

                //world.ForChunksInRange(min, max, (p, c) =>
                //{
                //    var offset = p.Multiply(world.ChunkSize);
                //    var localHit = point - offset;

                //    tool.removeSphere(c, localHit, range, material);
                //    c.LastChangeFrame = Time.frameCount;

                //});
            }
        }

        public void Start()
        {
        }
    }

    public class PlaceSphereStateMidair : IState
    {
        private VoxelWorldEditorScript script;
        private PlaceSphereState tool;
        private IEditableVoxelWorld world;

        public PlaceSphereStateMidair(VoxelWorldEditorScript script, IEditableVoxelWorld world)
        {
            this.script = script;
            this.world = world;
            this.tool = new PlaceSphereState(script, world);
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Update(RaycastHit? raycast)
        {
            var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            var point = ray.origin + ray.direction * script.ActiveSize * 2f;

            tool.tryPlaceSphere(script.ActiveSize, script.ActiveMaterial, point);
        }
    }

    class NullState : IState
    {
        public void Stop()
        {
        }

        public void Update(RaycastHit? raycast)
        {
        }

        public void Start()
        {
        }
    }
}
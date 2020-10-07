﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SplineMesh {
    /// <summary>
    /// Deform a mesh and place it along a spline, given various parameters.
    /// 
    /// This class intend to cover the most common situations of mesh bending. It can be used as-is in your project,
    /// or can serve as a source of inspiration to write your own procedural generator.
    /// </summary>
    [ExecuteInEditMode]
    [SelectionBase]
    [DisallowMultipleComponent]
    public class SplineMeshTiling : MonoBehaviour {
        private GameObject generated;
        private Spline spline = null;
        private bool toUpdate = false;

        [Tooltip("Mesh to bend along the spline.")]
        public Mesh mesh;
        [Tooltip("Material to apply on the bent mesh.")]
        public Material material;
        [Tooltip("Physic material to apply on the bent mesh.")]
        public PhysicMaterial physicMaterial;
        [Tooltip("Translation to apply on the mesh before bending it.")]
        public Vector3 translation;
        [Tooltip("Rotation to apply on the mesh before bending it.")]
        public Vector3 rotation;
        [Tooltip("Scale to apply on the mesh before bending it.")]
        public Vector3 scale = Vector3.one;

        [Tooltip("If true, a mesh collider will be generated.")]
        public bool generateCollider = true;

        [Tooltip("If true, the mesh will be bent on play mode. If false, the bent mesh will be kept from the editor mode, allowing lighting baking.")]
        public bool updateInPlayMode;

        [Tooltip("If true, a mesh will be placed on each curve of the spline. If false, a single mesh will be placed for the whole spline.")]
        public bool curveSpace = false;

        [Tooltip("The mode to use to fill the choosen interval with the bent mesh.")]
        public MeshBender.FillingMode mode = MeshBender.FillingMode.Repeat;

        private void OnEnable() {
            // tip : if you name all generated content in the same way, you can easily find all of it
            // at once in the scene view, with a single search.
            string generatedName = "generated by " + GetType().Name;
            var generatedTranform = transform.Find(generatedName);
            generated = generatedTranform != null ? generatedTranform.gameObject : UOUtility.Create(generatedName, gameObject);

            spline = GetComponentInParent<Spline>();
            spline.NodeListChanged += (s, e) => toUpdate = true;

            toUpdate = true;
        }

        private void OnValidate() {
            if (spline == null) return;
            toUpdate = true;
        }

        private void Update() {
            // we can prevent the generated content to be updated during playmode to preserve baked data saved in the scene
            if (!updateInPlayMode && Application.isPlaying) return;

            if (toUpdate) {
                toUpdate = false;
                CreateMeshes();
            }
        }

        public void CreateMeshes() {
            var used = new List<GameObject>();

            if (curveSpace) {
                int i = 0;
                foreach (var curve in spline.curves) {
                    var go = FindOrCreate("segment " + i++ + " mesh");
                    go.GetComponent<MeshBender>().SetInterval(curve);
                    go.GetComponent<MeshCollider>().enabled = generateCollider;
                    used.Add(go);
                }
            } else {
                var go = FindOrCreate("segment 1 mesh");
                go.GetComponent<MeshBender>().SetInterval(spline, 0);
                go.GetComponent<MeshCollider>().enabled = generateCollider;
                used.Add(go);
            }

            // we destroy the unused objects. This is classic pooling to recycle game objects.
            foreach (var go in generated.transform
                .Cast<Transform>()
                .Select(child => child.gameObject).Except(used)) {
                UOUtility.Destroy(go);
            }
        }

        private GameObject FindOrCreate(string name) {
            var childTransform = generated.transform.Find(name);
            GameObject res;
            if (childTransform == null) {
                res = UOUtility.Create(name,
                    generated,
                    typeof(MeshFilter),
                    typeof(MeshRenderer),
                    typeof(MeshBender),
                    typeof(MeshCollider));
                res.isStatic = true;
            } else {
                res = childTransform.gameObject;
            }
            res.GetComponent<MeshRenderer>().material = material;
            res.GetComponent<MeshCollider>().material = physicMaterial;
            MeshBender mb = res.GetComponent<MeshBender>();
            mb.Source = SourceMesh.Build(mesh)
                .Translate(translation)
                .Rotate(Quaternion.Euler(rotation))
                .Scale(scale);
            mb.Mode = mode;
            return res;
        }
    }
}

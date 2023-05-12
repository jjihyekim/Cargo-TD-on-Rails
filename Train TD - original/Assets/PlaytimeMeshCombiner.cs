using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlaytimeMeshCombiner {
    public static IEnumerator MeshCombineAlternative(GameObject root, Transform newParentRoot, MeshFilter[] meshFilters, MeshRenderer[] meshRenderers) {
        //MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        //CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        Dictionary<Material, List<List<CombineInstance>>> myCombiners = new Dictionary<Material, List<List<CombineInstance>>>();

        CreateMeshCombineDictionary(meshFilters, meshRenderers, myCombiners);
        
        yield return null;

        var newParent = new GameObject("Mesh Combine Parent");

        foreach (var key in myCombiners.Keys) {
            foreach (var combineList in myCombiners[key]) {
                var combinedObject = new GameObject("Combined " + key.name);
                combinedObject.transform.SetParent(newParent.transform);

                combinedObject.AddComponent<MeshFilter>();
                combinedObject.AddComponent<MeshRenderer>();

                /*var mesh = new Mesh();
                mesh.CombineMeshes(combineList.ToArray());
                mesh.Optimize();*/
                
                combinedObject.GetComponent<MeshFilter>().mesh = new Mesh();
                combinedObject.GetComponent<MeshFilter>().mesh.CombineMeshes(combineList.ToArray());

                combinedObject.GetComponent<MeshRenderer>().material = key;
            }
        }

        
        newParent.transform.SetParent(newParentRoot);
        
        /*root.GetComponent<MeshFilter>().mesh = new Mesh();
        root.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        root.gameObject.SetActive(true);*/
    }

    private static void CreateMeshCombineDictionary(MeshFilter[] meshFilters, MeshRenderer[] meshRenderers, Dictionary<Material, List<List<CombineInstance>>> myCombiners) {
        var combineCount = 200;
        int i = 0;
        while (i < meshFilters.Length) {
            var material = meshRenderers[i].sharedMaterial;

            if (!myCombiners.ContainsKey(material)) {
                var listToAdd = new List<List<CombineInstance>>();
                listToAdd.Add(new List<CombineInstance>());
                myCombiners.Add(material, listToAdd);
            }

            var theList = myCombiners[material];

            var findIndex = 0;
            while (theList[findIndex].Count >= combineCount) {
                findIndex += 1;
                if (theList.Count <= findIndex) {
                    theList.Add(new List<CombineInstance>());
                }
            }

            {
                theList[findIndex].Add(new CombineInstance() {
                    mesh = meshFilters[i].sharedMesh, 
                    transform = meshFilters[i].transform.localToWorldMatrix /* * meshFilters[i].transform.parent.worldToLocalMatrix*/
                });
            }

            //meshFilters[i].gameObject.SetActive(false);

            i++;
        }
    }

    private class MeshCombineMaterial {
        public Material Material;
        public List<MeshInstance> MeshInstances = new List<MeshInstance>();
    }

    private class MeshInstance {
        public Mesh SourceMesh;
        public int SubmeshIndex;
        public Transform MeshTransform;
    }

    private class CombinedMeshData {
        public List<Vector4> VertTangents;
        public List<Vector3> VertPositions;
        public List<Vector3> VertNormals;
        public List<Vector2> VertUV1;
        public List<Vector2> VertUV2;
        public List<Vector2> VertUV3;
        public List<Vector2> VertUV4;
        public List<Color> VertColors;
        public List<int> VertIndices;
        public int CurrentVertIndex;

        public CombinedMeshData(int combinedNumVertsGuess) {
            VertTangents = new List<Vector4>(combinedNumVertsGuess);
            VertPositions = new List<Vector3>(combinedNumVertsGuess);
            VertNormals = new List<Vector3>(combinedNumVertsGuess);
            VertUV1 = new List<Vector2>(combinedNumVertsGuess);
            VertUV2 = new List<Vector2>(combinedNumVertsGuess);
            VertUV3 = new List<Vector2>(combinedNumVertsGuess);
            VertUV4 = new List<Vector2>(combinedNumVertsGuess);
            VertColors = new List<Color>(combinedNumVertsGuess);

            // Assume each group of 3 verts forms a triangle
            VertIndices = new List<int>(combinedNumVertsGuess / 3);
        }

        public void Reset() {
            VertTangents.Clear();
            VertPositions.Clear();
            VertNormals.Clear();
            VertUV1.Clear();
            VertUV2.Clear();
            VertUV3.Clear();
            VertUV4.Clear();
            VertColors.Clear();
            VertIndices.Clear();
            CurrentVertIndex = 0;
        }

        public void AddCurrentVertIndex() {
            // Note: Assumes that vertices are stored in the combined mesh buffers in the same
            //       way as they are encountered when reading the vertex data using indices
            //       from the source mesh.
            VertIndices.Add(CurrentVertIndex++);
        }

        public void ReverseWindingOrderForLastTriangle() {
            int lastIndexPtr = VertIndices.Count - 1;
            int tempIndex = VertIndices[lastIndexPtr];
            VertIndices[lastIndexPtr] = VertIndices[lastIndexPtr - 2];
            VertIndices[lastIndexPtr - 2] = tempIndex;
        }
    }

    public static GameObject CombineMeshes(GameObject root) {

        List<GameObject> allMeshObjects = GetAllMeshObjectsInHierarchy(root);

        List<MeshCombineMaterial> meshCombineMaterials = GetMeshCombineMaterials(allMeshObjects, root);
        if (meshCombineMaterials.Count == 0) {
            return null;
        }
        
        var combinedMeshParent = new GameObject(GetNameForCombinedMeshesParent());

        Combine(meshCombineMaterials, combinedMeshParent);

        return combinedMeshParent;
    }
    
    public static List<GameObject> GetAllMeshObjectsInHierarchy( GameObject root)
    {
        var meshFilters = root.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0) return new List<GameObject>();

        var meshObjects = new List<GameObject>(meshFilters.Length);
        foreach(var meshFilter in meshFilters)
        {
            if(meshFilter.sharedMesh != null) meshObjects.Add(meshFilter.gameObject);
        }

        return meshObjects;
    }
    
    private static List<MeshCombineMaterial> GetMeshCombineMaterials(List<GameObject> meshObjects, GameObject sourceParent)
        {
            var meshCombineMaterialsMap = new Dictionary<Material, MeshCombineMaterial>();
            var meshCombineMaterials = new List<MeshCombineMaterial>();

            Transform sourceParentTransform = sourceParent != null ? sourceParent.transform : null;
            foreach (var meshObject in meshObjects)
            {
                Transform meshTransform = meshObject.transform;

                MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null) continue;

                MeshRenderer meshRenderer = meshObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null) continue;

                Mesh sharedMesh = meshFilter.sharedMesh;
                for (int submeshIndex = 0; submeshIndex < sharedMesh.subMeshCount; ++submeshIndex)
                {
                    Material subMeshMaterial = meshRenderer.sharedMaterials[submeshIndex];
                    MeshCombineMaterial meshCombineMaterial = null;

                    if (!meshCombineMaterialsMap.ContainsKey(subMeshMaterial))
                    {
                        meshCombineMaterial = new MeshCombineMaterial();
                        meshCombineMaterial.Material = subMeshMaterial;

                        meshCombineMaterialsMap.Add(subMeshMaterial, meshCombineMaterial);
                        meshCombineMaterials.Add(meshCombineMaterial);
                    }
                    else meshCombineMaterial = meshCombineMaterialsMap[subMeshMaterial];

                    var meshInstance = new MeshInstance();
                    meshInstance.SourceMesh = sharedMesh;
                    meshInstance.SubmeshIndex = submeshIndex;
                    meshInstance.MeshTransform = meshTransform;
                    meshCombineMaterial.MeshInstances.Add(meshInstance);
                }
            }

            return meshCombineMaterials;
        }
    
    private static void Combine(List<MeshCombineMaterial> meshCombineMaterials, GameObject combinedMeshesParent)
    {
        List<GameObject> allCombinedMeshObjects = new List<GameObject>(100);
        if (combinedMeshesParent == null)
        {
            combinedMeshesParent = new GameObject(GetNameForCombinedMeshesParent());
        }
        for (int combMaterialIndex = 0; combMaterialIndex < meshCombineMaterials.Count; ++combMaterialIndex)
        {
            MeshCombineMaterial meshCombMaterial = meshCombineMaterials[combMaterialIndex];

            List<GameObject> combineMeshdObjects = Combine(meshCombMaterial, combinedMeshesParent);
        }

        //PreserveColliders(meshCombineMaterials, combinedMeshesParent);
        
    }
    
    private static List<GameObject> Combine(MeshCombineMaterial meshCombineMaterial, GameObject combinedMeshesParent)
        {
            List<MeshInstance> meshInstances = meshCombineMaterial.MeshInstances;
            if (meshInstances.Count == 0) return new List<GameObject>();

            int maxNumMeshVerts = GetMaxNumberOfMeshVerts();
            const int numVertsPerMeshGuess = 500;
            int totalNumVertsGuess = meshCombineMaterial.MeshInstances.Count * numVertsPerMeshGuess;
            var combinedMeshData = new CombinedMeshData(totalNumVertsGuess);

            List<GameObject> combinedMeshObjects = new List<GameObject>(100);
            for (int meshInstanceIndex = 0; meshInstanceIndex < meshInstances.Count; ++meshInstanceIndex)
            {
                MeshInstance meshInstance = meshInstances[meshInstanceIndex];
                Mesh mesh = meshInstance.SourceMesh;
                if (mesh.vertexCount == 0) continue;

                Matrix4x4 worldMatrix = meshInstance.MeshTransform.localToWorldMatrix;
                Matrix4x4 worldInverseTranspose = worldMatrix.inverse.transpose;

                int numNegativeScaleComps = 0;
                Vector3 worldScale = meshInstance.MeshTransform.lossyScale;
                if (worldScale[0] < 0.0f) ++numNegativeScaleComps;
                if (worldScale[1] < 0.0f) ++numNegativeScaleComps;
                if (worldScale[2] < 0.0f) ++numNegativeScaleComps;
                bool reverseVertexWindingOrder = (numNegativeScaleComps % 2 != 0);

                int[] submeshVertIndices = mesh.GetTriangles(meshInstance.SubmeshIndex);
                if (submeshVertIndices.Length == 0) continue;

                Vector4[] vertTangents = mesh.tangents;
                Vector3[] vertPositions = mesh.vertices;
                Vector3[] vertNormals = mesh.normals;
                Vector2[] vertUV1 = mesh.uv;
                Vector2[] vertUV2 = mesh.uv2;
                Vector2[] vertUV3 = mesh.uv3;
                Vector2[] vertUV4 = mesh.uv4;
                Color[] vertColors = mesh.colors;

                foreach (var vertIndex in submeshVertIndices)
                {
                    if (vertTangents.Length != 0)
                    {
                        Vector3 transformedTangent = new Vector3(vertTangents[vertIndex].x, vertTangents[vertIndex].y, vertTangents[vertIndex].z);
                        transformedTangent = worldInverseTranspose.MultiplyVector(transformedTangent);
                        transformedTangent.Normalize();

                        combinedMeshData.VertTangents.Add(new Vector4(transformedTangent.x, transformedTangent.y, transformedTangent.z, vertTangents[vertIndex].w));
                    }

                    if (vertNormals.Length != 0)
                    {
                        Vector3 transformedNormal = worldInverseTranspose.MultiplyVector(vertNormals[vertIndex]);
                        transformedNormal.Normalize();

                        combinedMeshData.VertNormals.Add(transformedNormal);
                    }

                    if (vertPositions.Length != 0) combinedMeshData.VertPositions.Add(worldMatrix.MultiplyPoint(vertPositions[vertIndex]));
                    if (vertColors.Length != 0) combinedMeshData.VertColors.Add(vertColors[vertIndex]);
                    if (vertUV1.Length != 0) combinedMeshData.VertUV1.Add(vertUV1[vertIndex]);
                    if (vertUV3.Length != 0) combinedMeshData.VertUV2.Add(vertUV3[vertIndex]);
                    if (vertUV4.Length != 0) combinedMeshData.VertUV3.Add(vertUV4[vertIndex]);
                    //if (vertUV2.Length != 0 && !_combineSettings.GenerateLightmapUVs) combinedMeshData.VertUV2.Add(vertUV2[vertIndex]);

                    combinedMeshData.AddCurrentVertIndex();

                    int numIndices = combinedMeshData.VertIndices.Count;
                    if (reverseVertexWindingOrder && numIndices % 3 == 0) combinedMeshData.ReverseWindingOrderForLastTriangle();

                    int numMeshVerts = combinedMeshData.VertPositions.Count;
                    if (combinedMeshData.VertIndices.Count % 3 == 0 && (maxNumMeshVerts - numMeshVerts) < 3)
                    {
                        combinedMeshObjects.Add(CreateCombinedMeshObject(combinedMeshData, meshCombineMaterial.Material, combinedMeshesParent));
                        combinedMeshData.Reset();
                    }
                }
            }

            combinedMeshObjects.Add(CreateCombinedMeshObject(combinedMeshData, meshCombineMaterial.Material, combinedMeshesParent));

            return combinedMeshObjects;
        }
    
    private static GameObject CreateCombinedMeshObject(CombinedMeshData combinedMeshData, Material meshMaterial, GameObject combinedMeshesParent)
        {
            Mesh combinedMesh = CreateCombinedMesh(combinedMeshData);

            GameObject combinedMeshObject = new GameObject("yeet 2");
            combinedMeshObject.transform.parent = combinedMeshesParent.transform;
            combinedMeshObject.isStatic =  true;

            MeshFilter meshFilter = combinedMeshObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = combinedMesh;

            MeshRenderer meshRenderer = combinedMeshObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = meshMaterial;

            /*if(_combineSettings.CollHandling == MeshCombineSettings.ColliderHandling.CreateNew)
            {
                if(_combineSettings.NewCollidersType == MeshCombineSettings.ColliderType.Mesh)
                {
                    MeshCollider meshCollider = combinedMeshObject.AddComponent<MeshCollider>();
                    if (_combineSettings.UseConvexMeshColliders) meshCollider.convex = true;
                    else meshCollider.convex = false;

                    if (_combineSettings.NewCollidersAreTriggers) meshCollider.isTrigger = true;
                    else meshCollider.isTrigger = false;
                }
                else
                if(_combineSettings.NewCollidersType == MeshCombineSettings.ColliderType.Box)
                {
                    BoxCollider boxCollider = combinedMeshObject.AddComponent<BoxCollider>();
                    if (_combineSettings.NewCollidersAreTriggers) boxCollider.isTrigger = true;
                    else boxCollider.isTrigger = false;
                }
            }*/

            //Vector3 meshPivotPt = MeshCombineSettings.MeshPivotToWorldPoint(combinedMeshObject, _combineSettings.CombinedMeshesPivot);
            //combinedMeshObject.SetMeshPivotPoint(meshPivotPt);

            return combinedMeshObject;
        }
    
    private static Mesh CreateCombinedMesh(CombinedMeshData combinedMeshData)
    {
        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "yeet";

        combinedMesh.vertices = combinedMeshData.VertPositions.ToArray();
        if (combinedMeshData.VertTangents.Count != 0) combinedMesh.tangents = combinedMeshData.VertTangents.ToArray();
        if (combinedMeshData.VertNormals.Count != 0) combinedMesh.normals = combinedMeshData.VertNormals.ToArray();
        if (combinedMeshData.VertUV1.Count != 0) combinedMesh.uv = combinedMeshData.VertUV1.ToArray();
        if (combinedMeshData.VertUV3.Count != 0) combinedMesh.uv3 = combinedMeshData.VertUV3.ToArray();
        if (combinedMeshData.VertUV4.Count != 0) combinedMesh.uv4 = combinedMeshData.VertUV4.ToArray();
        if (combinedMeshData.VertColors.Count != 0) combinedMesh.colors = combinedMeshData.VertColors.ToArray();
        combinedMesh.SetIndices(combinedMeshData.VertIndices.ToArray(), MeshTopology.Triangles, 0);

        //if (_combineSettings.GenerateLightmapUVs) Unwrapping.GenerateSecondaryUVSet(combinedMesh);
        /*else*/ if (combinedMeshData.VertUV2.Count != 0) combinedMesh.uv2 = combinedMeshData.VertUV2.ToArray();

        combinedMesh.UploadMeshData(true);
        //SaveCombinedMeshAsAsset(combinedMesh);

        return combinedMesh;
    }
    
    private static int GetMaxNumberOfMeshVerts()
    {
        //if (_combineSettings.GenerateLightmapUVs) return 32000;
        return 65000;
    }
    
    private static string GetNameForCombinedMeshesParent()
    {
        return "cmbParent_Octave3D";
    }
}

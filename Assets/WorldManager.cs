using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ren;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEditor;

/// <summary>
/// Sample script to demostrate RWImport's asset streaming capabilities. 
/// </summary>
public class WorldManager : MonoBehaviour
{
    public string[] archives;
    public string[] files;

    // public string gameDirectory;

    public string filenameTextureSet = "robroads";
    public string filepathModel = "rd_Road2A20";

    // Start is called before the first frame update
    void Start()
    {
        RwiUnity.Init(log: true, forceFlush: true);

        if (archives != null)
        {
            for (int i = 0; i < archives.Length; i++)
            {
                RwiUnity.AddArchive(archives[i]);
            }
        }

        if (files != null)
        {
            for (int i = 0; i < files.Length; i++)
            {
                RwiUnity.AddFilepath(files[i]);
            }
        }

        RwiUnity.CacheCollfiles();

        handleTXD = RwiUnity.CreateTexsetLoadJob(filenameTextureSet);
        handleCOL = RwiUnity.CreateCollisionLoadJob(filepathModel);
        handleDFF = RwiUnity.CreateMeshLoadJob(filepathModel);
    }

    private void OnDestroy()
    {
        RwiUnity.Quit();
    }

    // Update is called once per frame
    void Update()
    { 
        if (!loadedTXD)
        {
            jobStatusTXD = RwiUnity.QueryJobStatus(handleTXD);
            textureQueryCount++;

            if (jobStatusTXD == JobStatus.FINISHED)
            {
                AddTextureSet("testTXD", handleTXD);
                loadedTXD = true;
                RUTextureSet textureSet = p_textureSets["testTXD"];
                loadedTextures = new Texture2D[textureSet.textures.Count];
                int index = 0;
                foreach (var item in textureSet.textures)
                {
                    loadedTextures[index] = item.Value;
                    Debug.Log("Trying to add " + item.Value.name + " to the list...");
                    index++;
                }
            }
            else if (jobStatusTXD == JobStatus.FAILED)
            {
                Debug.LogError(RwiUnity.GetFailMessage(handleTXD));
            }
        }

        if (!loadedDFF)
        {
            jobStatusCOL = RwiUnity.QueryJobStatus(handleCOL);
            jobStatusDFF = RwiUnity.QueryJobStatus(handleDFF);
            modelQueryCount++;
            
            if (jobStatusDFF == JobStatus.FINISHED)
            {
                AddModel("testname", handleDFF, handleCOL, "testTXD");
                InstantiateModel("testname", Vector3.zero, Quaternion.identity);
                loadedDFF = true;
                loadedCOL = true;
            }
            else if (jobStatusDFF == JobStatus.FAILED)
            {
                Debug.LogError(RwiUnity.GetFailMessage(handleDFF));
            }
            
        }
    }

    private Dictionary<string, RUTextureSet> p_textureSets = new Dictionary<string, RUTextureSet>();
    private Dictionary<string, RUInstantiationTree> p_models = new Dictionary<string, RUInstantiationTree>();
    private Dictionary<int, PhysicsMaterial> p_physicsMaterials = new Dictionary<int, PhysicsMaterial>();
    public void AddTextureSet(string texsetName, int texsetHandle)
    {
        RUTextureSet textureSet = RwiUnity.RetrieveTextureSet(texsetHandle);
        p_textureSets.Add(texsetName, textureSet);
    }

    public void AddModel(string modelName, int modelHandle, int collHandle, string textureSetName)
    {
        RUTextureSet textureSet = null;
        Debug.Log("Got texture set: " + p_textureSets.TryGetValue(textureSetName, out textureSet));

            RUInstantiationTree instantiationTree = RwiUnity.CreateInstantiationTree(modelHandle, collHandle, textureSet);
        
        p_models.Add(modelName, instantiationTree);
    }

    public void RemoveModel(string modelName)
    {
        RUInstantiationTree model = null;
        if (p_models.TryGetValue(modelName, out model))
        {
            p_models.Remove(modelName);
        }
    }
    public void InstantiateModel(string modelName, Vector3 position, Quaternion rotation)
    {
        RUInstantiationTree instantiationTree = p_models[modelName];

        GameObject rootObject = new GameObject(modelName);
        GameObject[] gameObjects = new GameObject[instantiationTree.transforms.Length];
        Debug.Log("Transform Count: " + instantiationTree.transforms.Length);
        for (int i = 0; i < instantiationTree.transforms.Length; i++)
        {
            gameObjects[i] = new GameObject();
            gameObjects[i].name = instantiationTree.transforms[i].name;
            Debug.Log("Transform name: " + instantiationTree.transforms[i].name + " Transform data: " + instantiationTree.transforms[i].meshIndex + " " + instantiationTree.transforms[i].parentIndex);
            int currentParentIndex = instantiationTree.transforms[i].parentIndex;
            if (currentParentIndex < 0)
            {
                gameObjects[i].transform.parent = rootObject.transform;
            }
            else
            {
                gameObjects[i].transform.parent = gameObjects[currentParentIndex].transform;
            }
            gameObjects[i].transform.localPosition = instantiationTree.transforms[i].position;
            gameObjects[i].transform.localRotation = instantiationTree.transforms[i].rotation;
            int meshIndex = instantiationTree.transforms[i].meshIndex;
            if (meshIndex >= 0 && meshIndex < instantiationTree.meshes.Length)
            {
                RUMesh currentMesh = instantiationTree.meshes[meshIndex];
                currentMesh.mesh.name = gameObjects[i].name;
                MeshFilter currentMeshFilter = gameObjects[i].AddComponent<MeshFilter>();
                currentMeshFilter.sharedMesh = currentMesh.mesh;
                MeshRenderer currentMeshRenderer = gameObjects[i].AddComponent<MeshRenderer>();
                currentMeshRenderer.materials = currentMesh.materials;
            }
        }
        // COLLISION NEEDS TO BE IMPLEMENTED!!!

        rootObject.transform.parent = transform;
        rootObject.transform.localPosition = position;
        rootObject.transform.localRotation = rotation;

        if (instantiationTree.collisionObject != null)
        {
            // Ignore the material for now!

            RUCollision collisionObject = instantiationTree.collisionObject;
            GameObject collisionRoot = new() { name = "collision" };
            for (int collmeshIndex = 0; collmeshIndex < collisionObject.collMeshes.Length; collmeshIndex++)
            {
                GameObject currentMeshObject = new() { name = "collmesh" + collmeshIndex };
                MeshCollider currentMeshCollider = currentMeshObject.AddComponent<MeshCollider>();
                currentMeshCollider.sharedMesh = collisionObject.collMeshes[collmeshIndex].mesh;
                currentMeshObject.transform.parent = collisionRoot.transform;
            }
            for (int collsphereIndex = 0; collsphereIndex < collisionObject.collSpheres.Length; collsphereIndex++)
            {
                GameObject currentCollsphereObject = new() { name = "collsphere" + collsphereIndex };
                SphereCollider currentSphereCollider = currentCollsphereObject.AddComponent<SphereCollider>();
                currentSphereCollider.radius = collisionObject.collSpheres[collsphereIndex].radius;
                currentCollsphereObject.transform.parent = collisionRoot.transform;
                currentCollsphereObject.transform.localPosition = collisionObject.collSpheres[collsphereIndex].center;
            }
            for (int collboxIndex = 0; collboxIndex < collisionObject.collBoxes.Length; collboxIndex++)
            {
                GameObject currentCollboxObject = new() { name = "collbox" + collboxIndex };
                BoxCollider currentBoxCollider = currentCollboxObject.AddComponent<BoxCollider>();
                currentBoxCollider.size = collisionObject.collBoxes[collboxIndex].dimensions;
                currentCollboxObject.transform.parent = collisionRoot.transform;
                currentCollboxObject.transform.localPosition = collisionObject.collBoxes[collboxIndex].center;
                currentCollboxObject.transform.localRotation = collisionObject.collBoxes[collboxIndex].rotation;
            }
            collisionRoot.transform.parent = rootObject.transform;
            collisionRoot.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f); 
        }
    }

    // What needs to be done?

    // Debug stuff:
    public int handleTXD = -1;
    public int handleCOL = -1;
    public int handleDFF = -1;
    public bool loadedTXD = false;
    public bool loadedCOL = false;
    public bool loadedDFF = false;
    public JobStatus jobStatusTXD = JobStatus.UNDEFINED;
    public JobStatus jobStatusCOL = JobStatus.UNDEFINED;
    public JobStatus jobStatusDFF = JobStatus.UNDEFINED;
    public int modelQueryCount = 0;
    public int textureQueryCount = 0;
    public Texture2D[] loadedTextures;
}

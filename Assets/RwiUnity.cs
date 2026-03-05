using Ren;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using static Ren.RwiDll;

/// @file 
/// @brief Defines helpers to turn the imported data into actual Unity objects.

namespace Ren
{
    /// <summary>
    /// Mirrors ren::MaterialDescriptor_blittable in "ren/ModelData.hpp".
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialDescriptor_blittable
    {
        public IntPtr textureName;
        public uint isTextured;
        public Vector3 color;
        public float ambient;
        public float specular;
        public float diffuse;
        public int padding;
    }

    /// <summary>
    /// Mirrors ren::Transform_blittable in "ren/ModelData.hpp".
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Transform_blittable
    {
        public IntPtr name;
        public Vector3 position;
        public Quaternion rotation;
        public int parentIndex;
        public int modelIndex;
        public int padding;
    };

    /// <summary>
    /// Mirrors ren::SubmeshDescriptor in "ren/ModelData.hpp".
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SubmeshDescriptor
    {
        public uint trianglesStart;
        public uint trianglesCount;
        public uint materialIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RUBox
    {
        public Vector3 lower;
        public Vector3 upper;
    }

    /// <summary>
    /// Mirrors ren::CollBox in "ren/ModelData.hpp".
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RUCollBox
    {
        public Vector3 center;
        public Vector3 dimensions;
        public Quaternion rotation;
        public int materialID;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RUSphere
    {
        public Vector3 center;
        public float radius;
    }

    /// <summary>
    /// Mirrors ren::CollSphere in "ren/ModelData.hpp".
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RUCollSphere
    {
        public Vector3 center;
        public float radius;
        public int materialID;
    }

    /// <summary>
    /// Helper enum for state communication.
    /// </summary>
    public enum JobStatus
    {
        UNDEFINED = -1, FINISHED = 0, PROCESSING, FAILED, OTHER
    }

    /// <summary>
    /// Managed (C#) equivalent of Transform_blittable.
    /// </summary>
    public struct RUTransform
    {
        public string name;
        public int parentIndex;
        public int meshIndex;
        public Vector3 position;
        public Quaternion rotation;
    }

    /// <summary>
    /// Contains a Unity mesh indended for rendering and the appropriate materials.
    /// </summary>
    public class RUMesh
    {
        public Mesh mesh;
        public Material[] materials;
    }

    /// <summary>
    /// Contains a Unity mesh indended for collision and the material ID intended for reassignment later.
    /// </summary>
    public class RUCollMesh
    {
        public Mesh mesh;
        public int materialID;
    }

    /// <summary>
    /// A unified collision model.
    /// </summary>
    public class RUCollision
    {
        public RUCollMesh[] collMeshes;
        public RUCollBox[] collBoxes;
        public RUCollSphere[] collSpheres;
    }

    /// <summary>
    /// Contrary to the name, a linear representation of a model graph. 
    /// A graph template -- designed for quick instantiation (streaming). 
    /// Basically a combination of a DFF object combined with its collision object.
    /// </summary>
    public class RUInstantiationTree
    {
        public RUTransform[] transforms;
        public RUMesh[] meshes;
        public RUCollision collisionObject;
    }

    /// <summary>
    /// A searchable collection of textures; mirrors a TXD texture dictionary.
    /// </summary>
    public class RUTextureSet
    {
        public Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    }

}

/// <summary>
/// Collection of helper functions/methods to interface with the native RWImport DLL and obtain actual Unity objects.
/// </summary>
public static class RwiUnity
{
    /// <summary>
    /// Commissions a DFF load job in RWImport. </summary>
    /// <param name="filename">
    /// The filename within RWImport's virtual filesystem.</param>
    /// <returns>
    /// The handle required to retrieve the converted data. 
    /// This handle must be freed once the data has been completely retrieved. </returns>
    /// <exception cref="System.Exception">If the load job creation fails.</exception>
    public static int CreateMeshLoadJob(string filename)
    {
        int jobID;
        RwiDll.RwiResultCode result = RwiDll.rwiEnqueueFile(out jobID, filename, RwiDll.MDType.MD_MODEL, /*ProcessFlags.FLIP_NORMALS |*/ ProcessFlags.FLIP_TRIANGLES | ProcessFlags.SWAP_YZ | ProcessFlags.PRUNE_TRANSFORMS);
        if (result == RwiDll.RwiResultCode.OK)
        {
            return jobID;
        }
        else
        {
            throw new System.Exception("Could not create meshmodel load job: " + result.ToString());
        }
    }

    /// <summary>
    /// Commissions a collision load job (single collision model) in RWImport. </summary>
    /// <param name="filename">
    /// The collision model's name within RWImport's collision object list ("cached collfiles").</param>
    /// <returns>
    /// The handle required to retrieve the converted data. 
    /// This handle must be freed once the data has been completely retrieved. </returns>
    /// <exception cref="Exception">
    /// If the load job creation fails.</exception>
    public static int CreateCollisionLoadJob(string filename)
    {
        int jobID;
        RwiResultCode result = rwiEnqueueFile(out jobID, filename, MDType.MD_COLLISION, ProcessFlags.SWAP_YZ);
        if (result == RwiResultCode.OK)
        {
            return jobID;
        }
        else
        {
            throw new Exception("Could not create collision load job: " + result.ToString());
        }
    }

    /// <summary>
    /// Commissions a TXD load job in RWImport.  </summary>
    /// <param name="filename">
    /// The filename within RWImport's virtual filesystem.</param>
    /// <returns>
    /// The handle required to retrieve the converted data. 
    /// This handle must be freed once the data has been completely retrieved.</returns>
    /// <exception cref="System.Exception">
    /// If the load job creation fails.</exception>
    public static int CreateTexsetLoadJob(string filename)
    {
        int jobID;
        RwiResultCode resultCode = rwiEnqueueFile(out jobID, filename, MDType.MD_TEXSET, ProcessFlags.NONE);
        if (resultCode == RwiResultCode.OK)
        {
            return jobID;
        }
        else
        {
            throw new System.Exception("Could not create texture set load job: " + resultCode.ToString());
        }
    }

    /// <summary>
    /// Queries the current satus of the load job. </summary>
    /// <param name="handle">
    /// The job handle.</param>
    /// <returns>
    /// The JobStatus enum.</returns>
    public static JobStatus QueryJobStatus(int handle)
    {
        RwiDll.RwiResultCode result_code = RwiDll.rwiQueryStatus(handle);
        switch (result_code)
        {
            case RwiDll.RwiResultCode.OK:
                {
                    return JobStatus.FINISHED;
                }
            case RwiDll.RwiResultCode.PROCESSING:
                {
                    return JobStatus.PROCESSING;
                }
            case RwiDll.RwiResultCode.OPERATION_FAILED:
                {
                    return JobStatus.FAILED;
                }
            default:
                {
                    return JobStatus.OTHER;
                }
        }
    }

    /// <summary>
    /// Retrieves a single mesh out of a Meshmodel instance. </summary>
    /// <param name="handle">
    /// The handle received when the load job was commissioned. </param>
    /// <param name="meshIndex">
    /// The index of the mesh within the Meshmodel instance.</param>
    /// <param name="textureSet">
    /// The Texture Set instance to be used for material creation. </param>
    /// <returns>
    /// The mesh.</returns>
    /// <exception cref="Exception">
    /// If the mesh data could not be retrieved.</exception>
    public static RUMesh /* new */ RetrieveMesh(int handle, int meshIndex, RUTextureSet textureSet)
    {
        RwiDll.MeshDefinition meshDefinition;
        // Debug.Log("Calling QueryMesh()...");
        RwiDll.RwiResultCode resultCode = RwiDll.rwiQueryMesh(handle, meshIndex, out meshDefinition);
        if (resultCode != RwiDll.RwiResultCode.OK)
        {
            throw new Exception("Cannot query mesh: " + resultCode.ToString());
        }
        Vector3[] vertices = new Vector3[meshDefinition.vertexCount];
        Vector2[] uvs = new Vector2[meshDefinition.vertexCount];
        Vector3[] normals = new Vector3[meshDefinition.vertexCount];
        Color[] colors = new Color[meshDefinition.vertexCount];
        ushort[] indices = new ushort[meshDefinition.triangleCount * 3];
        MaterialDescriptor_blittable[] materials = new MaterialDescriptor_blittable[meshDefinition.materialCount];
        SubmeshDescriptor[] submeshes = new SubmeshDescriptor[meshDefinition.submeshCount];

        GCHandle vertexHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
        GCHandle uvHandle = GCHandle.Alloc(uvs, GCHandleType.Pinned);
        GCHandle normalHandle = GCHandle.Alloc(normals, GCHandleType.Pinned);
        GCHandle colorHandle = GCHandle.Alloc(colors, GCHandleType.Pinned);
        GCHandle indexHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
        GCHandle materialHandle = GCHandle.Alloc(materials, GCHandleType.Pinned);
        GCHandle submeshHandle = GCHandle.Alloc(submeshes, GCHandleType.Pinned);

        RwiDll.MeshCopyTarget copyTarget;
        copyTarget.vertices = vertexHandle.AddrOfPinnedObject();
        copyTarget.UVs = uvHandle.AddrOfPinnedObject();
        copyTarget.normals = normalHandle.AddrOfPinnedObject();
        copyTarget.colors = colorHandle.AddrOfPinnedObject();
        copyTarget.triangles = indexHandle.AddrOfPinnedObject();
        copyTarget.materials = materialHandle.AddrOfPinnedObject();
        copyTarget.submeshes = submeshHandle.AddrOfPinnedObject();

        // Debug.Log("Calling GetMeshSC...");
        resultCode = RwiDll.rwiGetMeshSC(handle, meshIndex, in copyTarget);

        vertexHandle.Free();
        uvHandle.Free();
        normalHandle.Free();
        colorHandle.Free();
        indexHandle.Free();
        materialHandle.Free();
        submeshHandle.Free();

        if (resultCode != RwiDll.RwiResultCode.OK) 
        {
            throw new Exception("Cannot copy mesh data: " + resultCode.ToString());
        }

        Mesh resultMesh = new Mesh();
        resultMesh.vertices = vertices;
        resultMesh.uv = uvs;
        resultMesh.normals = normals;

        resultMesh.colors = colors;

        resultMesh.subMeshCount = submeshes.Length;
        for (int i = 0; i < submeshes.Length; i++)
        {
            // Indices are probably in the wrong format right now (32 bit int would be better)
            // Debug.Log("Submesh data "+ i + ": start " + submeshes[i].trianglesStart + ", count: " + submeshes[i].trianglesCount);
            resultMesh.SetTriangles(indices, (int)submeshes[i].trianglesStart * 3, (int)submeshes[i].trianglesCount * 3, i);
        }

        Material[] resultMaterials = new Material[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            resultMaterials[i] = new Material(Shader.Find("Standard"));
            resultMaterials[i].color = new Color { r = materials[i].color.x, g = materials[i].color.y, b = materials[i].color.z };
            // The shader properties need to be set!

            if (textureSet != null)
            {
                string textureName = Marshal.PtrToStringAnsi(materials[i].textureName);
                try
                {
                    resultMaterials[i].mainTexture = textureSet.textures[textureName];
                    Debug.Log("Set texture \"" + textureName + "\"");
                }
                catch 
                {
                    Debug.Log("Failed to set texture \"" + textureName + "\"");
                }
            }
        }

        RUMesh result = new RUMesh();
        result.mesh = resultMesh;
        result.materials = resultMaterials;

        return result;
    }

    /// <summary>
    /// Retrieves a single collmesh out of a collision object. </summary>
    /// <param name="handle">
    /// The handle received when the load job was commissioned.</param>
    /// <param name="index">
    /// The index of the mesh within the collision object.</param>
    /// <returns>
    /// The collision mesh.</returns>
    /// <exception cref="Exception">
    /// If the collision mesh data could not be retrieved.</exception>
    public static RUCollMesh RetrieveCollMesh(int handle, int index)
    {
        CollmeshDefinition collmeshDefinition;
        RwiResultCode resultCode = rwiQueryCollmesh(handle, index, out collmeshDefinition);
        if (resultCode != RwiResultCode.OK)
        {
            throw new Exception("Could not query collision mesh: " + resultCode.ToString());
        }

        Vector3[] vertices = new Vector3[collmeshDefinition.vertexCount];
        ushort[] indices = new ushort[collmeshDefinition.triangleCount * 3];

        GCHandle vertexHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
        GCHandle indexHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);

        CollmeshCopyTarget copyTarget;
        copyTarget.vertices = vertexHandle.AddrOfPinnedObject();
        copyTarget.triangles = indexHandle.AddrOfPinnedObject();

        resultCode = rwiGetCollmeshSC(handle, index, in copyTarget);

        vertexHandle.Free();
        indexHandle.Free();

        if (resultCode != RwiResultCode.OK)
        {
            throw new Exception("Cannot copy collision mesh data: " + resultCode.ToString());
        }

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.SetTriangles(indices, 0);

        RUCollMesh collMesh = new RUCollMesh();
        collMesh.mesh = mesh;
        collMesh.materialID = collmeshDefinition.materialID;

        return collMesh;
    }

    /// <summary>
    /// Retrieves an entire collision object. </summary>
    /// <param name="handle">
    /// The handle received when the load job was commissioned. </param>
    /// <returns>
    /// The collision object.</returns>
    /// <exception cref="Exception">
    /// If there is an error in retrieving the constituent data.</exception>
    public static RUCollision RetrieveCollisionObject(int handle)
    {
        CollisionDefinition collisionDefinition;
        RwiResultCode resultCode = rwiQueryCollision(handle, out collisionDefinition);
        if (resultCode != RwiResultCode.OK)
        {
            return null;
        }

        RUCollMesh[] collMeshes = new RUCollMesh[collisionDefinition.meshCount];
        for (int meshIndex = 0; meshIndex < collMeshes.Length; meshIndex++)
        {
            collMeshes[meshIndex] = RetrieveCollMesh(handle, meshIndex);
            collMeshes[meshIndex].mesh.name = "collmesh" + meshIndex;
        }

        RUCollSphere[] collSpheres = new RUCollSphere[collisionDefinition.sphereCount];
        RUCollBox[] collBoxes = new RUCollBox[collisionDefinition.boxCount];
        GCHandle sphereHandle = GCHandle.Alloc(collSpheres, GCHandleType.Pinned);
        GCHandle boxHandle = GCHandle.Alloc(collBoxes, GCHandleType.Pinned);

        CollprimitivesCopyTarget copyTarget;
        copyTarget.spheres = sphereHandle.AddrOfPinnedObject();
        copyTarget.boxes = boxHandle.AddrOfPinnedObject();
        resultCode = rwiGetCollprimitivesSC(handle, in copyTarget);
        if (resultCode != RwiResultCode.OK)
        {
            throw new Exception("Cannot copy collision primitives: " +  resultCode.ToString());
        }

        RUCollision result = new RUCollision();
        result.collMeshes = collMeshes;
        result.collBoxes = collBoxes;
        result.collSpheres = collSpheres;

        return result;
    }

    /// <summary>
    /// Retrieves a Texture2D out of a texture set. </summary>
    /// <param name="handle">
    /// The handle obtained when the texture set load job was commissioned.</param>
    /// <param name="textureIndex">
    /// The index of the texture within the texture set. </param>
    /// <returns>
    /// The texture.</returns>
    /// <exception cref="Exception">
    /// If there is an error in retrieving the data.</exception>
    public static Texture2D RetrieveTextureCopy(int handle, int textureIndex)
    {
        // This might be done better using a NativeArray or IntPtr explicitly
        TextureDefinition textureDefinition;
        RwiResultCode resultCode = rwiQueryTexture(handle, textureIndex, out textureDefinition);
        if (resultCode != RwiResultCode.OK)
        {
            throw new Exception("Cannot query texture: " + resultCode.ToString());
        }

        Texture2D result = null;

        switch (textureDefinition.type)
        {
            case MDTextureType.RGB:
                {
                    byte[] data = new byte[textureDefinition.width * textureDefinition.height * 3];
                    GCHandle textureHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    TextureCopyTarget copyTarget;
                    copyTarget.data = textureHandle.AddrOfPinnedObject();
                    resultCode = rwiGetTextureSC(handle, textureIndex, in copyTarget);
                    textureHandle.Free();
                    if (resultCode != RwiResultCode.OK)
                    {
                        throw new Exception("Cannot copy raster data: " + resultCode.ToString());
                    }
                    result = new Texture2D((int)textureDefinition.width, (int)textureDefinition.height, TextureFormat.RGB24, mipChain: false);
                    result.LoadRawTextureData(data);
                    break;
                }
            case MDTextureType.RGBA:
                {
                    byte[] data = new byte[textureDefinition.width * textureDefinition.height * 4];
                    GCHandle textureHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    TextureCopyTarget copyTarget;
                    copyTarget.data = textureHandle.AddrOfPinnedObject();
                    resultCode = rwiGetTextureSC(handle, textureIndex, in copyTarget);
                    textureHandle.Free();
                    if (resultCode != RwiResultCode.OK)
                    {
                        throw new Exception("Cannot copy raster data: " + resultCode.ToString());
                    }
                    result = new Texture2D((int)textureDefinition.width, (int)textureDefinition.height, TextureFormat.RGBA32, mipChain: false);
                    result.LoadRawTextureData(data);
                    break;
                }
            case MDTextureType.BGRA:
                {
                    byte[] data = new byte[textureDefinition.width * textureDefinition.height * 4];
                    GCHandle textureHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    TextureCopyTarget copyTarget;
                    copyTarget.data = textureHandle.AddrOfPinnedObject();
                    resultCode = rwiGetTextureSC(handle, textureIndex, in copyTarget);
                    textureHandle.Free();
                    if (resultCode != RwiResultCode.OK)
                    {
                        throw new Exception("Cannot copy raster data: " + resultCode.ToString());
                    }
                    result = new Texture2D((int)textureDefinition.width, (int)textureDefinition.height, TextureFormat.BGRA32, mipChain: false);
                    result.LoadRawTextureData(data);
                    break;
                }
            default:
                {
                    throw new Exception("Unsupported data type: " + textureDefinition.type.ToString());
                }
        }
        result.name = Marshal.PtrToStringAnsi(textureDefinition.nameASCII);
        result.Apply(updateMipmaps: true); // Uploads it to the GPU, possible better postponed???
        return result;
    }

    /// <summary>
    /// Retrieves a texture set. </summary>
    /// <param name="handle">
    /// The handle received when the texture set load job was commissioned.</param>
    /// <returns>
    /// The texture set.</returns>
    /// <exception cref="Exception">
    /// If there is an error in loading the constituent textures. </exception>
    public static RUTextureSet RetrieveTextureSet(int handle)
    {
        TextureSetDefinition textureSetDefinition;
        RwiResultCode resultCode = rwiQueryTextureSet(handle, out textureSetDefinition);

        if (resultCode != RwiResultCode.OK)
        {
            throw new Exception("Cannot query texture set: " + resultCode.ToString());
        }

        RUTextureSet textureSet = new();
        for (int i = 0; i < textureSetDefinition.textureCount; i++)
        {
            Texture2D currentTexture = RetrieveTextureCopy(handle, i);
            textureSet.textures.Add(currentTexture.name, currentTexture);
        }

        return textureSet;
    }

    /// <summary>
    /// Frees a handle. </summary>
    /// <param name="handle">
    /// The handle received when the load job was commissioned.</param>
    public static void FreeHandle(int handle)
    {
        RwiDll.rwiFree(handle);
    }

    /// <summary>
    /// Retrieves the error message associated with a failed load job. </summary>
    /// <param name="handle">
    /// The handle received when the load job was commissioned.</param>
    /// <returns>
    /// The message.</returns>
    public static string GetFailMessage(int handle)
    {
        if (RwiDll.rwiQueryStatus(handle) == RwiDll.RwiResultCode.OPERATION_FAILED)
        {
            return Marshal.PtrToStringAnsi(RwiDll.rwiGetFailMessage(handle));
        }
        else
        {
            return "No fail message!";
        }
    }

    /// <summary>
    /// Retrieves the array of transforms of a meshmodel. </summary>
    /// <param name="handle">
    /// The handle received when the meshmodel laod job was commissioned.</param>
    /// <returns>
    /// The array of transforms. </returns>
    /// <exception cref="Exception">
    /// If there is an error in retrieving the data.</exception>
    public static RUTransform[] GetMeshmodelTransforms(int handle)
    {
        RwiDll.TransformCopyDefinition copyDefinition;
        RwiDll.RwiResultCode resultCode = RwiDll.rwiGetTransformsCC(handle, out copyDefinition);
        if (resultCode != RwiDll.RwiResultCode.OK)
        {
            throw new Exception("Cannot retrieve transform tree information: " + resultCode.ToString());
        }

        // Debug.Log("Transform count by copy definition: " + copyDefinition.transformCount);

        Transform_blittable[] transforms_blittable = new Transform_blittable[copyDefinition.transformCount];

        GCHandle transformsHandle = GCHandle.Alloc(transforms_blittable, GCHandleType.Pinned);
        RwiDll.TransformCopyTarget copyTarget;
        copyTarget.transforms = transformsHandle.AddrOfPinnedObject();
        resultCode = RwiDll.rwiGetTransformsSC(handle, in copyTarget);
        transformsHandle.Free();
        if (resultCode != RwiDll.RwiResultCode.OK)
        {
            throw new Exception("Cannot copy transform tree: " + resultCode.ToString());
        }

        /* Debug:
        string output = "Raw transform data: count = " + copyDefinition.transformCount + "\n";
        for (int i = 0; i < transforms_blittable.Length; i++)
        {
            output += "Parent index: " + transforms_blittable[i].parentIndex + " ";
            output += "Model index: " + transforms_blittable[i].modelIndex + " ";
            output += "Position: " + transforms_blittable[i].position + " ";
            output += "Name: " + transforms_blittable[i].name + "\n";
        }
        Debug.Log(output);
        // Debug end.*/

        RUTransform[] transforms = new RUTransform[copyDefinition.transformCount];

        for (int i = 0;  i < copyDefinition.transformCount; i++)
        {
            transforms[i].name = Marshal.PtrToStringAnsi(transforms_blittable[i].name);
            transforms[i].parentIndex = transforms_blittable[i].parentIndex;
            transforms[i].meshIndex = transforms_blittable[i].modelIndex;
            transforms[i].position = transforms_blittable[i].position;
            transforms[i].rotation = transforms_blittable[i].rotation;
        }
        return transforms;
    }

    /// <summary>
    /// Creates an instantiation tree (a graph template) from a valid meshmodel, a collision model and an existing texture set. </summary>
    /// <param name="modelHandle">
    /// The handle received when the meshmodel load job was commissioned.</param>
    /// <param name="collHandle">
    /// The handle received when the collision model load job was commissioned.</param>
    /// <param name="textureSet">
    /// The texture set to be used in the instantiation.</param>
    /// <returns>
    /// The instantiation tree.</returns>
    /// <exception cref="Exception">
    /// If there is an error in obtaining any of the constituent data. </exception>
    public static RUInstantiationTree CreateInstantiationTree(int modelHandle, int collHandle, RUTextureSet textureSet)
    {
        RwiDll.MeshmodelDefinition meshmodelDefinition;

        RwiDll.RwiResultCode resultCode = RwiDll.rwiQueryMeshmodel(modelHandle, out meshmodelDefinition);
        if (resultCode != RwiDll.RwiResultCode.OK)
        {
            throw new Exception("Cannot retrieve meshmodel definition: " + resultCode.ToString());
        }

        RUInstantiationTree instantiationTree = new()
        {
            transforms = GetMeshmodelTransforms(modelHandle),
            meshes = new RUMesh[meshmodelDefinition.meshCount]
        };

        for (int i = 0; i < meshmodelDefinition.meshCount; i++)
        {
            instantiationTree.meshes[i] = RetrieveMesh(modelHandle, i, textureSet);
        }

        instantiationTree.collisionObject = RetrieveCollisionObject(collHandle);

        return instantiationTree;
    }

    /// <summary>
    /// Adds an IMG archive (i.e. all the files defined within it) to RWImport's virtual filesystem. </summary>
    /// <param name="filepath">
    /// The filepath of the IMG archive.</param>
    /// <exception cref="Exception">
    /// If the archive could not be added.</exception>
    public static void AddArchive(string filepath)
    {
        RwiResultCode resultCode = rwiAddArchive(filepath);
        if (resultCode != RwiResultCode.OK)
        {
            throw new Exception("Failed to add \"" + filepath + "\" to archives list: " + resultCode.ToString());
        }
    }

    /// <summary>
    /// Adds a single file to RWImport's virtual filesystem. </summary>
    /// <param name="filepath">
    /// The filepath of the single file.</param>
    /// <exception cref="Exception">
    /// If the file could not be added.</exception>
    public static void AddFilepath(string filepath)
    {
        RwiResultCode resultCode = rwiAddFilepath(filepath);
        if (resultCode != RwiResultCode.OK)
        {
            throw new Exception("Failed to add \"" + filepath + "\" to the unified filesystem: " + resultCode.ToString());
        }
    }

    /// <summary>
    /// Cache all collision models. This must be used excactly once before collision models are available. </summary>
    /// <exception cref="Exception">
    /// If the caching fails (e.g. if the files have already been cached).</exception>
    public static void CacheCollfiles()
    {
        RwiResultCode resultCode = rwiCacheCollfiles();
        if (resultCode != RwiResultCode.OK) 
        { 
            throw new Exception("Failed to cache collision files: " + resultCode.ToString()); 
        }
    }

    /// <summary>
    /// Starts the loading-conversion thread. This must be called before anything happens. </summary>
    /// <param name="log">
    /// Enable/disable the logger.</param>
    /// <param name="forceFlush">
    /// Writes each log message to disk immediately. </param>
    /// <exception cref="Exception">
    /// If the thread cannot be started. </exception>
    public static void Init(bool log = false, bool forceFlush = false)
    {
        if (log)
        {
            rwiEnableLogging("RwiLog.txt");
            if (forceFlush)
            {
                rwiForceLogFlush();
            }
        }

        RwiResultCode resultCode = rwiStartLoadthread();
        if (resultCode != RwiResultCode.OK)
        {
            throw new Exception("Cannot start Loadthread: " +  resultCode.ToString());
        }
    }

    /// <summary>
    /// Stops the loading-conversion thread. 
    /// </summary>
    public static void Quit()
    {
        rwiStopLoadthread();
        rwiDisableLogging();
    }
}

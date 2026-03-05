using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Numerics;

/// @file
/// @brief Import definition for the RWImport DLL and various struct definitions used by the interface directly. 

namespace Ren
{
    /// <summary>
    /// Enum that mirrors ren::MDTextureType in "ren/ModelData.hpp".
    /// </summary>
    public enum MDTextureType : int
    {
        RGB =       0,
        RGBA =      1,
        BGR =       2,
        BGRA =      3,
        DXT1 =      4,
        DXT3 =      5,
        DXT5 =      6
    }

    /// <summary>
    /// Enum that mirrors various processing flags of the conversion functions.
    /// </summary>
    [Flags]
    public enum ProcessFlags : uint
    {
        NONE = 0x0,

        // Geometry Processing Flags
        SWAP_YZ = 0x1,
        FLIP_TRIANGLES = 0x2,
        FLIP_NORMALS = 0x4,
        PRUNE_TRANSFORMS = 0x8
    }

    /// <summary>
    /// Contains the import side of the RWImport DLL. Please consult the RWImport documentation for details on these functions and structs. 
    /// </summary>
    static class RwiDll
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MeshmodelDefinition
        {
            public uint meshCount;
            public uint transformCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TransformCopyTarget
        {
            public IntPtr transforms;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TransformCopyDefinition
        {
            public IntPtr transforms;
            public uint transformCount;
            public int padding;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MeshDefinition
        {
            public uint vertexCount;
            public uint triangleCount;
            public uint materialCount;
            public uint submeshCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MeshCopyTarget
        {
            public IntPtr vertices;
            public IntPtr UVs;
            public IntPtr normals;
            public IntPtr colors;
            public IntPtr triangles;
            public IntPtr materials;
            public IntPtr submeshes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MeshCopyDefinition
        {
            public IntPtr vertices;
            public IntPtr UVs;
            public IntPtr normals;
            public IntPtr colors;
            public IntPtr triangles;
            public IntPtr materials;
            public IntPtr submeshes;

            public uint vertexCount;
            public uint triangleCount;
            public uint materialCount;
            public uint submeshCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TextureSetDefinition // Changed 2025-10-08
        {
            public uint textureCount;
        }

        /*
        [StructLayout(LayoutKind.Sequential)]
        public struct TextureNamePtrsCopyTarget // Added 2025-10-08
        {
            public IntPtr textureNames;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct TextureNamePtrsCopyDefinition // Added 2025-10-08
        {
            public IntPtr textureNames;
            public uint nameCount;
            public uint padding;
        };*/

        [StructLayout(LayoutKind.Sequential)]
        public struct TextureDefinition
        {
            public IntPtr nameASCII;
            public MDTextureType type;
            public uint width;
            public uint height;
            public uint _padding_;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TextureCopyTarget
        {
            public IntPtr data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TextureCopyDefinition
        {
            public IntPtr data;
            public uint type;
            public uint width;
            public uint height;
            public int _padding_;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct CollisionDefinition
        {
            public IntPtr nameASCII;
            public uint meshCount;
            public uint boxCount;
            public uint sphereCount;
            public float boundsCenterX;
            public float boundsCenterY;
            public float boundsCenterZ;
            public float boundsDimX;
            public float boundsDimY;
            public float boundsDimZ;
            int padding;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CollmeshDefinition
        {
            public uint vertexCount;
            public uint triangleCount;
            public int materialID;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CollmeshCopyTarget
        {
            public IntPtr vertices;
            public IntPtr triangles;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CollmeshCopyDefinition
        {
            public IntPtr vertices;
            public IntPtr triangles;
            public uint vertexCount;
            public uint triangleCount;
            public int materialID;
            int padding;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CollprimitivesCopyTarget
        {
            public IntPtr boxes;
            public IntPtr spheres;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CollprimitivesCopyDefinition
        {
            IntPtr boxes;
            IntPtr spheres;
            uint boxCount;
            uint sphereCount;
        }

        public enum MDType : int
        {
            MD_MODEL = 0,
            MD_TEXSET = 1,
            MD_COLLISION = 2
        }

        public enum RwiResultCode : int
        {
            OK = 0,
            HANDLE_DOES_NOT_EXIST,
            OPERATION_FAILED,
            PROCESSING,
            NOT_INITIALIZED,
            NULLPTR_CPY_TARGET,
            OUT_OF_BOUNDS_MESH_INDEX,
            NULLPTR_STRUCT,
            DATA_FORMAT_MISMATCH,
            OUT_OF_BOUNDS_TEXTURE_INDEX,
            FILE_CANNOT_BE_OPENED,
            COLLFILES_ALREADY_CACHED,
            LOADTHREAD_ALREADY_RUNNING,
            LOADTHREAD_NOT_RUNNING,
            LOGGER_ALREADY_RUNNING,
            _LOGGER_NOT_RUNNING
        }

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiThreadStatus();

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiQueryStatus(int handle);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiEnqueueFile(out int handle, [MarshalAs(UnmanagedType.LPStr)] string path, MDType jobType, ProcessFlags jobFlags);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiQueryMeshmodel(int handle, out MeshmodelDefinition meshmodelDefinition);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiQueryMesh(int handle, int meshIndex, out MeshDefinition meshDefinition);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiQueryMeshesAll(int handle, IntPtr outArray); // NEW!!!

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiGetTransformsSC(int handle, in TransformCopyTarget copyTarget);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiGetTransformsCC(int handle, out TransformCopyDefinition copyDefinition);

        [DllImport("RWImport.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiGetMeshSC(int handle, int meshIndex, in MeshCopyTarget copyTarget);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiGetMeshCC(int handle, int meshIndex, out MeshCopyDefinition copyDefinition);

        [DllImport("RWImport.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiQueryTextureSet(int handle, out TextureSetDefinition textureSetDefinition);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiQueryTexture(int handle, int textureIndex, out TextureDefinition textureDefinition);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiQueryTexturesAll(int handle, IntPtr outArray); // NEW!!!

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiGetTextureSC(int handle, int textureIndex, in TextureCopyTarget copyTarget);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiGetTextureCC(int handle, int textureIndex, out TextureCopyDefinition copyDefinition);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiQueryCollision(int handle, out CollisionDefinition collisionDefinition);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiQueryCollmesh(int handle, int meshIndex, out CollmeshDefinition collmeshDefinition);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiGetCollprimitivesSC(int handle, in CollprimitivesCopyTarget copyTarget);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiGetCollprimitivesCC(int handle, out CollprimitivesCopyDefinition copyDefinition);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiGetCollmeshSC(int handle, int meshIndex, in CollmeshCopyTarget copyTarget);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiGetCollmeshCC(int handle, int meshIndex, out CollmeshCopyDefinition copyDefinition);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiClearQueue();

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiFree(int handle);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rwiQueueLength();

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rwiFinishedJobs();

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr rwiGetFailMessage(int handle);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiAddArchive([MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiAddFilepath([MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiCacheCollfiles();

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiStartLoadthread();

        [DllImport("RWImport.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern RwiResultCode rwiStopLoadthread();

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rwiEnableLogging([MarshalAs(UnmanagedType.LPStr)] string logfileName);

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rwiDisableLogging();

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rwiForceLogFlush();

        [DllImport("RWImport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rwiNoforceLogFlush();
    }
}

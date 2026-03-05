using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ren;

public class ProvisionalObjectLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // handle = RwiUnity.CreateMeshLoadJob("C:\\Users\\Robert\\Desktop\\rd_Road1A5.dff");
    }

    // Update is called once per frame
    void Update()
    {/*
        if (!loaded)
        {
            jobStatus = RwiUnity.QueryJobStatus(handle);
            if (jobStatus == JobStatus.FINISHED)
            {
                Mesh mesh = RwiUnity.RetrieveMesh(handle, 0);
                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
                gameObject.AddComponent<MeshRenderer>();
                meshFilter.mesh = mesh;
                loaded = true;
            }
            else if (jobStatus == JobStatus.FAILED)
            {
                Debug.LogError(RwiUnity.GetFailMessage(handle));
            }
        }*/
    }

    public int handle = -1;
    public bool loaded = false;
    public JobStatus jobStatus = JobStatus.UNDEFINED;
}

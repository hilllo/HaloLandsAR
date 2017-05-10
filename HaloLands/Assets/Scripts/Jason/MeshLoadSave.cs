using UnityEngine;
using System.Collections;
//using UnityEngine.Windows;
using System;
using System.IO;
using Game;

public class MeshLoadSave : Singleton<MeshLoadSave>
{

    public delegate void LoadMeshFinished(bool success, Vector3[] meshVertices, Vector3[] meshNormals, Int32[] meshIndices);
    public LoadMeshFinished loadMeshCallback;

    public delegate void SaveMeshFinished(bool success);
    public SaveMeshFinished saveMeshCallback;

    private Vector3[] vecArray;
    private Int32[] intArray;
    private byte[] workingBytes;

    public void LoadRoomMesh()
    {
        LoadMeshWithFilePath(Application.persistentDataPath + "/RoomMesh");
    }

    public void LoadMeshWithFilePath(string meshPath)
    {
        StartCoroutine(LoadMeshCoroutine(meshPath));
    }

    public void SaveMeshWithFilePath(string meshPath, Vector3[] meshVertices, Vector3[] meshNormals, Int32[] meshIndices)
    {
        StartCoroutine(SaveMeshCoroutine(meshPath, meshVertices, meshNormals, meshIndices));
    }

    public void SaveRoomMesh(Vector3[] meshVertices, Vector3[] meshNormals, Int32[] meshIndices)
    {
        SaveMeshWithFilePath(Application.persistentDataPath + "/RoomMesh", meshVertices, meshNormals, meshIndices);
    }

    IEnumerator LoadMeshCoroutine(string meshPath)
    {
        if(!File.Exists(meshPath + "Vertices") || !File.Exists(meshPath + "Normals") || !File.Exists(meshPath + "Indices"))
        {
            DebugDisplay.Log("Load failed");
            loadMeshCallback(false, null, null, null);
        }
        else
        {
            byte[] meshVerticesBytes = File.ReadAllBytes(meshPath + "Vertices");
            yield return BytesToVector3ArrayCoroutine(meshVerticesBytes);
            Vector3[] meshVerticesVectors = vecArray;

            byte[] meshNormalsBytes = File.ReadAllBytes(meshPath + "Normals");
            yield return BytesToVector3ArrayCoroutine(meshNormalsBytes);
            Vector3[] meshNormalsVectors = vecArray;

            byte[] meshIndicesBytes = File.ReadAllBytes(meshPath + "Indices");
            yield return BytesToInt32ArrayCoroutine(meshIndicesBytes);
            Int32[] meshIndicesArray = intArray;
            
            DebugDisplay.Log("Load success");
            loadMeshCallback(true, meshVerticesVectors, meshNormalsVectors, meshIndicesArray);
        }
        yield return null;
        
    }


    IEnumerator SaveMeshCoroutine(string meshPath, Vector3[] meshVertices, Vector3[] meshNormals, Int32[] meshIndices)
    {
        yield return Vector3ArrayToBytesCoroutine(meshVertices);
        File.WriteAllBytes(meshPath + "Vertices", workingBytes);

        yield return Vector3ArrayToBytesCoroutine(meshNormals);
        File.WriteAllBytes(meshPath + "Normals", workingBytes);

        yield return Int32ArrayToBytesCoroutine(meshIndices);
        File.WriteAllBytes(meshPath + "Indices", workingBytes);

        saveMeshCallback(true);

        yield return null;
    }

    IEnumerator BytesToVector3ArrayCoroutine(byte[] bytes)
    {
        int Length = bytes.Length;
        int size = Length / (3*sizeof(float));
        vecArray = new Vector3[size];
        int baseNum = 0;
        float currentTime = Time.time;
        for (int i = 0; i < size; i++)
        {
            vecArray[i] = new Vector3(BitConverter.ToSingle(bytes, baseNum + 0), BitConverter.ToSingle(bytes, baseNum + 4), BitConverter.ToSingle(bytes, baseNum + 8));
            baseNum += (3 * sizeof(float));
            if(Time.time - currentTime > 0.01f)
            {
                yield return null;
                currentTime = Time.time;
            }
        }
    }

    IEnumerator BytesToInt32ArrayCoroutine(byte[] bytes)
    {
        int Length = bytes.Length;
        int size = Length / sizeof(Int32);
        intArray = new Int32[size];
        int baseNum = 0;
        float currentTime = Time.time;
        for (int i = 0; i < size; i++)
        {
            intArray[i] = BitConverter.ToInt32(bytes, baseNum);
            baseNum += sizeof(Int32);
            if (Time.time - currentTime > 0.01f)
            {
                yield return null;
                currentTime = Time.time;
            }
        }
    }

    IEnumerator Vector3ArrayToBytesCoroutine(Vector3[] vec3Array)
    {
        workingBytes = new byte[sizeof(float) * vec3Array.Length * 3];
        int baseNum = 0;
        float currentTime = Time.time;
        for (int i = 0; i < vec3Array.Length; i++)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(vec3Array[i].x), 0, workingBytes, 0 + baseNum, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(vec3Array[i].y), 0, workingBytes, 4 + baseNum, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(vec3Array[i].z), 0, workingBytes, 8 + baseNum, sizeof(float));
            baseNum += (3 * sizeof(float));

            if (Time.time - currentTime > 0.01f)
            {
                yield return null;
                currentTime = Time.time;
            }
        }
    }

    IEnumerator Int32ArrayToBytesCoroutine(Int32[] intArray)
    {
        workingBytes = new byte[sizeof(Int32) * intArray.Length];
        int baseNum = 0;
        float currentTime = Time.time;
        for (int i = 0; i < intArray.Length; i++)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(intArray[i]), 0, workingBytes, 0 + baseNum, sizeof(Int32));
            baseNum += sizeof(Int32);

            if (Time.time - currentTime > 0.01f)
            {
                yield return null;
                currentTime = Time.time;
            }
        }
    }
}

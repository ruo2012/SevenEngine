﻿// SEVENENGINE LISCENSE:
// You are free to use, modify, and distribute any or all code segments/files for any purpose
// including commercial use with the following condition: any code using or originally taken 
// from the SevenEngine project must include citation to its original author(s) located at the
// top of each source code file, or you may include a reference to the SevenEngine project as
// a whole but you must include the current SevenEngine official website URL and logo.
// - Thanks.  :)  (support: seven@sevenengine.com)

// Author(s):
// - Zachary Aaron Patten (aka Seven) seven@sevenengine.com
// Last Edited: 10-26-13

using System;
using System.IO;

using SevenEngine.DataStructures;
using SevenEngine.Models;
using SevenEngine.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SevenEngine
{
  /// <summary>This static StaticModelManager class is storage for static meshes and models. It also includes the code for
  /// loading models from files.</summary>
  public static class StaticModelManager
  {
    private static AvlTree<StaticMesh> _staticMeshDatabase = new AvlTree<StaticMesh>();
    private static AvlTree<StaticModel> _staticModelDatabase = new AvlTree<StaticModel>();

    /// <summary>The number of meshes currently loaded onto the graphics card.</summary>
    public static int Count { get { return _staticMeshDatabase.Count; } }

    public static StaticMesh GetMesh(string staticMeshId)
    {
      StaticMesh mesh = _staticMeshDatabase.Get(staticMeshId);
      return mesh;
    }

    /// <summary>Gets a static model you loaded that you have loaded.</summary>
    /// <param name="staticModelId">The name id of the model you wish to obtain.</param>
    /// <returns>The desired static model if it exists.</returns>
    public static StaticModel GetModel(string staticModelId)
    {
      //return _staticModelDatabase.Get(staticModelId).Clone();
      StaticModel modelToGet = _staticModelDatabase.Get(staticModelId);

      //List<Link3<string, Texture, StaticMesh>> meshes = new List<Link3<string, Texture, StaticMesh>>();

      //Link3<string, Texture, StaticMesh> looper;
      //modelToGet.Meshes.IteratorInitialize();
      //while (modelToGet.Meshes.IteratorGetNext(out looper))
      //{
      //  looper.Middle.ExistingReferences++;
      //  looper.Right.ExistingReferences++;
      //  meshes.Add(looper.Left, new Link3<string,Texture,StaticMesh>(looper.Left, looper.Middle, looper.Right));
      //}

      List<Link3<string, Texture, StaticMesh>> meshes = modelToGet.Meshes.Clone(PullOutModelComponents);

      //return new StaticModel(modelToGet.Id, modelToGet.Meshes);
      return new StaticModel(modelToGet.Id, meshes);
    }

    /// <summary>This function is used as a delegate to determine the cloning process of static model meshes.</summary>
    private static void PullOutModelComponents(string currentId, Link3<string, Texture, StaticMesh> link,
      out string newId, out Link3<string, Texture, StaticMesh> newLink)
    {
      link.Middle.ExistingReferences++;
      link.Right.ExistingReferences++;
      newId = currentId;
      newLink = new Link3<string,Texture,StaticMesh>(link.Left, link.Middle, link.Right);
    }

    /// <summary>Loads an 3d model file. NOTE that only obj files are currently supported.</summary>
    /// <param name="textureManager">The texture manager so that the mesh can automatically texture itself.</param>
    /// <param name="staticMeshId">The key used to look up this mesh in the database.</param>
    /// <param name="filePath">The filepath of the model file you are attempting to load.</param>
    public static void LoadMesh(string staticMeshId, string filePath)
    {
      _staticMeshDatabase.Add(staticMeshId, LoadObj(staticMeshId, filePath));
      string[] pathSplit = filePath.Split('\\');
      Output.WriteLine("Model file loaded: \"" + pathSplit[pathSplit.Length - 1] + "\".");
    }

    /// <summary>Loads an 3d model file. NOTE that only obj files are currently supported.</summary>
    /// <param name="textureManager">The texture manager so that the mesh can automatically texture itself.</param>
    /// <param name="staticMeshId">The key used to look up this mesh in the database.</param>
    /// <param name="filePath">The filepath of the model file you are attempting to load.</param>
    public static void LoadSevenModel(string staticModelId, string filePath)
    {
      _staticModelDatabase.Add(staticModelId, LoadSevenModelFromDisk(staticModelId, filePath));
      string[] pathSplit = filePath.Split('\\');
      Output.WriteLine("Model file loaded: \"" + pathSplit[pathSplit.Length - 1] + "\".");
    }

    public static void LoadModel(string staticModelId, string[] textures, string[] meshs, string[] meshNames) { _staticModelDatabase.Add(staticModelId, new StaticModel(staticModelId, textures, meshs, meshNames)); }

    public static void RemoveModel(string staticMeshId)
    {
      // Get the struct with the GPU mappings.
      StaticMesh removal = GetMesh(staticMeshId);

      // If the game tries to remove a texture that still has active references then
        // lets warn them.
      if (removal.ExistingReferences > 1)
      {
        Output.WriteLine("WARNING: texture removal \"" + staticMeshId + "\" still has active references.");
      }

      // Delete the vertex buffer if it exists.
      int vertexBufferId = removal.VertexBufferHandle;
      if (vertexBufferId != 0)
        GL.DeleteBuffers(1, ref vertexBufferId);
      // Delete the normal buffer if it exists.
      int normalbufferId = removal.NormalBufferHandle;
      if (normalbufferId != 0)
        GL.DeleteBuffers(1, ref normalbufferId);
      // Delete the color buffer if it exists.
      int colorBufferId = removal.ColorBufferHandle;
      if (colorBufferId != 0)
        GL.DeleteBuffers(1, ref colorBufferId);
      // Delete the texture coordinate buffer if it exists.
      int textureCoordinateBufferId = removal.TextureCoordinateBufferHandle;
      if (textureCoordinateBufferId != 0)
        GL.DeleteBuffers(1, ref textureCoordinateBufferId);
      // Delete the element buffer if it exists.
      int elementBufferId = removal.ElementBufferHandle;
      if (elementBufferId != 0)
        GL.DeleteBuffers(1, ref elementBufferId);
      // Now we can remove it from the dictionary.
      _staticMeshDatabase.Remove(staticMeshId);
    }

    private static StaticMesh LoadObj(string staticMeshId, string filePath)
    {
      // These are temporarily needed lists for storing the parsed data as you read it.
      // Its better to use "ListArrays" vs "Lists" because they will be accessed by indeces
      // by the faces of the obj file.
      ListArray<float> fileVerteces = new ListArray<float>(10000);
      ListArray<float> fileNormals = new ListArray<float>(10000);
      ListArray<float> fileTextureCoordinates = new ListArray<float>(10000);
      ListArray<int> fileIndeces = new ListArray<int>(10000);

      // Lets read the file and handle each line separately for ".obj" files
      using (StreamReader reader = new StreamReader(filePath))
      {
        while (!reader.EndOfStream)
        {
          string[] parameters = reader.ReadLine().Trim().Split(' ');
          switch (parameters[0])
          {
            // Vertex
            case "v":
              fileVerteces.Add(float.Parse(parameters[1]));
              fileVerteces.Add(float.Parse(parameters[2]));
              fileVerteces.Add(float.Parse(parameters[3]));
              break;

            // Texture Coordinate
            case "vt":
              fileTextureCoordinates.Add(float.Parse(parameters[1]));
              fileTextureCoordinates.Add(float.Parse(parameters[2]));
              break;

            // Normal
            case "vn":
              fileNormals.Add(float.Parse(parameters[1]));
              fileNormals.Add(float.Parse(parameters[2]));
              fileNormals.Add(float.Parse(parameters[3]));
              break;

            // Face
            case "f":
              // NOTE! This does not yet triangulate faces
              // NOTE! This needs all possible values (position, texture mapping, and normal).
              for (int i = 1; i < parameters.Length; i++)
              {
                string[] indexReferences = parameters[i].Split('/');
                fileIndeces.Add(int.Parse(indexReferences[0]));
                if (indexReferences[1] != "")
                  fileIndeces.Add(int.Parse(indexReferences[1]));
                else
                  fileIndeces.Add(0);
                if (indexReferences[2] != "")
                  fileIndeces.Add(int.Parse(indexReferences[2]));
                else
                  fileIndeces.Add(0);
              }
              break;
          }
        }
      }

      // Pull the final vertex order out of the indexed references
      // Note, arrays start at 0 but the index references start at 1
      float[] verteces = new float[fileIndeces.Count];
      for (int i = 0; i < fileIndeces.Count; i += 3)
      {
        int index = (fileIndeces[i] - 1) * 3;
        verteces[i] = fileVerteces[index];
        verteces[i + 1] = fileVerteces[index + 1];
        verteces[i + 2] = fileVerteces[index + 2];
      }

      // Pull the final texture coordinates order out of the indexed references
      // Note, arrays start at 0 but the index references start at 1
      // Note, every other value needs to be inverse (not sure why but it works :P)
      float[] textureCoordinates = new float[fileIndeces.Count / 3 * 2];
      for (int i = 1; i < fileIndeces.Count; i += 3)
      {
        int index = (fileIndeces[i] - 1) * 2;
        int offset = (i - 1) / 3;
        textureCoordinates[i - 1 - offset] = fileTextureCoordinates[index];
        textureCoordinates[i - offset] = 1 - fileTextureCoordinates[(index + 1)];
      }

      // Pull the final normal order out of the indexed references
      // Note, arrays start at 0 but the index references start at 1
      float[] normals = new float[fileIndeces.Count];
      for (int i = 2; i < fileIndeces.Count; i += 3)
      {
        int index = (fileIndeces[i] - 1) * 3;
        normals[i - 2] = fileNormals[index];
        normals[i - 1] = fileNormals[(index + 1)];
        normals[i] = fileNormals[(index + 2)];
      }

      int vertexBufferId;
      if (verteces != null)
      {
        // Declare the buffer
        GL.GenBuffers(1, out vertexBufferId);
        // Select the new buffer
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
        // Initialize the buffer values
        GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(verteces.Length * sizeof(float)), verteces, BufferUsageHint.StaticDraw);
        // Quick error checking
        int bufferSize;
        GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
        if (verteces.Length * sizeof(float) != bufferSize)
          throw new ApplicationException("Vertex array not uploaded correctly");
        // Deselect the new buffer
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      }
      else { vertexBufferId = 0; }

      int textureCoordinateBufferId;
      if (textureCoordinates != null)
      {
        // Declare the buffer
        GL.GenBuffers(1, out textureCoordinateBufferId);
        // Select the new buffer
        GL.BindBuffer(BufferTarget.ArrayBuffer, textureCoordinateBufferId);
        // Initialize the buffer values
        GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(textureCoordinates.Length * sizeof(float)), textureCoordinates, BufferUsageHint.StaticDraw);
        // Quick error checking
        int bufferSize;
        GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
        if (textureCoordinates.Length * sizeof(float) != bufferSize)
          throw new ApplicationException("TexCoord array not uploaded correctly");
        // Deselect the new buffer
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      }
      else { textureCoordinateBufferId = 0; }

      int normalBufferId;
      if (normals != null)
      {
        // Declare the buffer
        GL.GenBuffers(1, out normalBufferId);
        // Select the new buffer
        GL.BindBuffer(BufferTarget.ArrayBuffer, normalBufferId);
        // Initialize the buffer values
        GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(normals.Length * sizeof(float)), normals, BufferUsageHint.StaticDraw);
        // Quick error checking
        int bufferSize;
        GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
        if (normals.Length * sizeof(float) != bufferSize)
          throw new ApplicationException("Normal array not uploaded correctly");
        // Deselect the new buffer
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      }
      else { normalBufferId = 0; }

      return new StaticMesh(
        filePath,
        staticMeshId,
        vertexBufferId,
        0, // Obj files don't support vertex colors
        textureCoordinateBufferId,
        normalBufferId,
        0, // I don't support an index buffer at this time
        verteces.Length);
    }

    public static StaticModel LoadSevenModelFromDisk(string staticModelId, string filePath)
    {
      // These are temporarily needed lists for storing the parsed data as you read it.
      ListArray<float> fileVerteces = new ListArray<float>(1000);
      ListArray<float> fileNormals = new ListArray<float>(1000);
      ListArray<float> fileTextureCoordinates = new ListArray<float>(1000);
      ListArray<int> fileIndeces = new ListArray<int>(1000);
      Texture texture = null;
      string meshName = "defaultMeshName";

      List<Link3<string, Texture, StaticMesh>> meshes = new List<Link3<string, Texture, StaticMesh>>();

      // Lets read the file and handle each line separately for ".obj" files
      using (StreamReader reader = new StreamReader(filePath))
      {
        while (!reader.EndOfStream)
        {
          string[] parameters = reader.ReadLine().Trim().Split(' ');
          switch (parameters[0])
          {
            // MeshName
            case "m":
              meshName = parameters[1];
              break;

            // Texture
            case "t":
              if (!TextureManager.TextureExists(parameters[1]))
                TextureManager.LoadTexture(parameters[1], parameters[2]);
              texture = TextureManager.Get(parameters[1]);
              break;

            // Vertex
            case "v":
              fileVerteces.Add(float.Parse(parameters[1]));
              fileVerteces.Add(float.Parse(parameters[2]));
              fileVerteces.Add(float.Parse(parameters[3]));
              break;

            // Texture Coordinate
            case "vt":
              fileTextureCoordinates.Add(float.Parse(parameters[1]));
              fileTextureCoordinates.Add(float.Parse(parameters[2]));
              break;

            // Normal
            case "vn":
              fileNormals.Add(float.Parse(parameters[1]));
              fileNormals.Add(float.Parse(parameters[2]));
              fileNormals.Add(float.Parse(parameters[3]));
              break;

            // Face
            case "f":
              // NOTE! This does not yet triangulate faces
              // NOTE! This needs all possible values (position, texture mapping, and normal).
              for (int i = 1; i < parameters.Length; i++)
              {
                string[] indexReferences = parameters[i].Split('/');
                fileIndeces.Add(int.Parse(indexReferences[0]));
                if (indexReferences[1] != "")
                  fileIndeces.Add(int.Parse(indexReferences[1]));
                else
                  fileIndeces.Add(0);
                if (indexReferences[2] != "")
                  fileIndeces.Add(int.Parse(indexReferences[2]));
                else
                  fileIndeces.Add(0);
              }
              break;

            // End Current Mesh
            case "7":
              // Pull the final vertex order out of the indexed references
              // Note, arrays start at 0 but the index references start at 1
              float[] verteces = new float[fileIndeces.Count];
              for (int i = 0; i < fileIndeces.Count; i += 3)
              {
                int index = (fileIndeces[i] - 1) * 3;
                verteces[i] = fileVerteces[index];
                verteces[i + 1] = fileVerteces[index + 1];
                verteces[i + 2] = fileVerteces[index + 2];
              }

              // Pull the final texture coordinates order out of the indexed references
              // Note, arrays start at 0 but the index references start at 1
              // Note, every other value needs to be inverse (not sure why but it works :P)
              float[] textureCoordinates = new float[fileIndeces.Count / 3 * 2];
              for (int i = 1; i < fileIndeces.Count; i += 3)
              {
                int index = (fileIndeces[i] - 1) * 2;
                int offset = (i - 1) / 3;
                textureCoordinates[i - 1 - offset] = fileTextureCoordinates[index];
                textureCoordinates[i - offset] = 1 - fileTextureCoordinates[(index + 1)];
              }

              // Pull the final normal order out of the indexed references
              // Note, arrays start at 0 but the index references start at 1
              float[] normals = new float[fileIndeces.Count];
              for (int i = 2; i < fileIndeces.Count; i += 3)
              {
                int index = (fileIndeces[i] - 1) * 3;
                normals[i - 2] = fileNormals[index];
                normals[i - 1] = fileNormals[(index + 1)];
                normals[i] = fileNormals[(index + 2)];
              }

              int vertexBufferId;
              if (verteces != null)
              {
                // Declare the buffer
                GL.GenBuffers(1, out vertexBufferId);
                // Select the new buffer
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
                // Initialize the buffer values
                GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(verteces.Length * sizeof(float)), verteces, BufferUsageHint.StaticDraw);
                // Quick error checking
                int bufferSize;
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (verteces.Length * sizeof(float) != bufferSize)
                  throw new ApplicationException("Vertex array not uploaded correctly");
                // Deselect the new buffer
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
              }
              else { vertexBufferId = 0; }

              int textureCoordinateBufferId;
              if (textureCoordinates != null)
              {
                // Declare the buffer
                GL.GenBuffers(1, out textureCoordinateBufferId);
                // Select the new buffer
                GL.BindBuffer(BufferTarget.ArrayBuffer, textureCoordinateBufferId);
                // Initialize the buffer values
                GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(textureCoordinates.Length * sizeof(float)), textureCoordinates, BufferUsageHint.StaticDraw);
                // Quick error checking
                int bufferSize;
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (textureCoordinates.Length * sizeof(float) != bufferSize)
                  throw new ApplicationException("TexCoord array not uploaded correctly");
                // Deselect the new buffer
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
              }
              else { textureCoordinateBufferId = 0; }

              int normalBufferId;
              if (normals != null)
              {
                // Declare the buffer
                GL.GenBuffers(1, out normalBufferId);
                // Select the new buffer
                GL.BindBuffer(BufferTarget.ArrayBuffer, normalBufferId);
                // Initialize the buffer values
                GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(normals.Length * sizeof(float)), normals, BufferUsageHint.StaticDraw);
                // Quick error checking
                int bufferSize;
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (normals.Length * sizeof(float) != bufferSize)
                  throw new ApplicationException("Normal array not uploaded correctly");
                // Deselect the new buffer
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
              }
              else { normalBufferId = 0; }

              meshes.Add(meshName,
                new Link3<string, Texture, StaticMesh>(
                  meshName,
                  texture,
                  new StaticMesh(
                  filePath,
                  staticModelId + "sub" + meshes.Count,
                  vertexBufferId,
                  0, // Obj files don't support vertex colors
                  textureCoordinateBufferId,
                  normalBufferId,
                  0, // I don't support an index buffer at this time
                  verteces.Length)));
              fileVerteces.Clear();
              fileNormals.Clear();
              fileTextureCoordinates.Clear();
              fileIndeces.Clear();
              texture = null;
              break;
          }
        }
      }
      return new StaticModel(staticModelId, meshes);
    }
  }
}
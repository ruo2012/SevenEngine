﻿using System;
using System.IO;

using SevenEngine;
using SevenEngine.Imaging;
using SevenEngine.DataStructures;
using SevenEngine.Shaders;

using Game.States;

namespace Game
{
  // This is an example of how to use my engine. Read this file and the "GameState.cs" file within the "State" folder.
  // Hope you enjoy using my engine :)
  public class Game : SevenEngineWindow
  {
    public Game() : base() { }

    public override void InitializeDisplay()
    {
      // SET INITIAL DISPLAY SETTINGS HERE.
      // Use the static class "GraphicsSettingsManager"

      GraphicsSettingsManager.BackFaceCulling = true;
      GraphicsSettingsManager.DepthBuffer = true;
      GraphicsSettingsManager.VerticalSyncronization = true;
      GraphicsSettingsManager.ClearColor = Color.Maroon;
      GraphicsSettingsManager.Texture2D = true;
      GraphicsSettingsManager.Blend = true;
      GraphicsSettingsManager.SetAlphaBlending();
      GraphicsSettingsManager.Lighting = true;
    }

    public override void InitializeSounds()
    {
      // LOAD SOUNDS HERE.
      // Use the static class "SoundManager"

      // Just keep this function here. I havn't finished the SoundManager class yet...
      Output.WriteLine("No sound effects currently loaded.");
    }

    public override void InitializeTextures()
    {
      // LOAD TEXTURES HERE.
      // Use the static class "TextureManager"

      // Note: I only support ".bmp"files at the moment.
      // Just pull non-bitmaps into any standard image editor and export them as bitmap files.

      TextureManager.LoadTexture("grass",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Textures\grass.bmp");
      TextureManager.LoadTexture("RedRanger",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Textures\RedRangerBody.bmp");
      TextureManager.LoadTexture("BlueRanger",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Textures\BlueRangerBody.bmp");
      TextureManager.LoadTexture("PinkRanger",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Textures\PinkRangerBody.bmp");
      TextureManager.LoadTexture("BlackRanger",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Textures\BlackRangerBody.bmp");
      TextureManager.LoadTexture("YellowRanger",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Textures\YellowRangerBody.bmp");

      TextureManager.LoadTexture("Menu",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Textures\Menu.bmp");

      TextureManager.LoadTexture("SkyboxLeft",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Textures\SkyBoxes\NightWalker\NightWalkerLeft.bmp");
      TextureManager.LoadTexture("SkyboxRight",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Textures\SkyBoxes\NightWalker\NightWalkerRight.bmp");
      TextureManager.LoadTexture("SkyboxFront",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Textures\SkyBoxes\NightWalker\NightWalkerFront.bmp");
      TextureManager.LoadTexture("SkyboxBack",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Textures\SkyBoxes\NightWalker\NightWalkerBack.bmp");
      TextureManager.LoadTexture("SkyboxTop",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Textures\SkyBoxes\NightWalker\NightWalkerTop.bmp");

    }

    public override void InitializeModels()
    {
      // LOAD MODEL FILES HERE.
      // Use the static class "StaticModelManager"

      // NOTE: only support obj files at the moment. ".obj" files MUST include vertex position, uvs, AND normals (my parser still needs to be more versatile)
      // Just use blender to export .obj model files with teh following settings:
      // (1) triangulated, (2) include normals, (3) include uvs, -z forward (i think), y up (i think) and all other settings off
      // ALSO: each model you load can only have a single texture that its mapped to (export each object separately)
      //   I know these are a lot of restrictions, but I'm only one dude writing all this... ill get to it eventually

      StaticModelManager.LoadMesh("terrain",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Models\Terrain.obj");
      StaticModelManager.LoadMesh("RedRanger",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Models\RedRanger.obj");

      StaticModelManager.LoadModel("Terrain", new string[] { "grass" }, new string[] { "terrain" }, new string[] { "Terrain" });
      StaticModelManager.LoadModel("RedRanger", new string[] { "RedRanger" }, new string[] { "RedRanger" }, new string[] { "Body" });
      StaticModelManager.LoadModel("BlueRanger", new string[] { "BlueRanger" }, new string[] { "RedRanger" }, new string[] { "Body" });
      StaticModelManager.LoadModel("BlackRanger", new string[] { "BlackRanger" }, new string[] { "RedRanger" }, new string[] { "Body" });
      StaticModelManager.LoadModel("PinkRanger", new string[] { "PinkRanger" }, new string[] { "RedRanger" }, new string[] { "Body" });
      StaticModelManager.LoadModel("YellowRanger", new string[] { "YellowRanger" }, new string[] { "RedRanger" }, new string[] { "Body" });

      StaticModelManager.LoadSevenModel("RedRangerSeven",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Models\RedRanger.obj7");
    }

    public override void InitializeShaders()
    {
      // LOAD SHADER FILES HERE.
      // Use the static class "ShaderManager"

      // These basic shaders do not include lighting effects.
      ShaderManager.LoadVertexShader("VertexShaderBasic",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Shaders\Vertex\VertexShaderBasic.glsl");
      ShaderManager.LoadFragmentShader("FragmentShaderBasic",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Shaders\Fragment\FragmentShaderBasic.glsl");

      // These shaders set stages based of normal values to give a solid shading toon look.
      // WARNING!!! i glitched these shaders up pretty bad, im stil new to shaders... try the other two
      ShaderManager.LoadVertexShader("VertexShaderToon",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Shaders\Vertex\VertexShaderToon.glsl");
      ShaderManager.LoadFragmentShader("FragmentShaderToon",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Shaders\Fragment\FragmentShaderToon.glsl");

      // These Lambertian shaders include an algorithm for lighting.
      ShaderManager.LoadVertexShader("VertexShaderLambertian",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Shaders\Vertex\VertexShaderLambertian.glsl");
      ShaderManager.LoadFragmentShader("FragmentShaderLambertian",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Shaders\Fragment\FragmentShaderLambertian.glsl");

      // These Phong shaders include an algorithm for lighting.
      ShaderManager.LoadVertexShader("VertexShaderPhong",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Shaders\Vertex\VertexShaderPhong.glsl");
      ShaderManager.LoadFragmentShader("FragmentShaderPhong",
        Directory.GetCurrentDirectory() + @"\..\..\Assets\Shaders\Fragment\FragmentShaderPhong.glsl");

      // LINK TOGETHER YOUR SHADERS HERE
      // Use the static class "ShaderManager"

      ShaderManager.MakeShaderProgram(
        "ShaderProgramBasic", // What to name this shader program.
        "VertexShaderBasic", // The vertex shader to use.
        "FragmentShaderBasic", // The fragment shader to use.
        null, // The geometry shader to use.
        null); // The extended geometry shader to use.

      // WARNING!!! im still new to shaders and i messed this one up pretty bad. try the others.
      ShaderManager.MakeShaderProgram(
        "ShaderProgramToon",
        "VertexShaderToon",
        "FragmentShaderToon",
        null,
        null);

      ShaderManager.MakeShaderProgram(
        "ShaderProgramLambertian",
        "VertexShaderLambertian",
        "FragmentShaderLambertian",
        null,
        null);

      ShaderManager.MakeShaderProgram(
        "ShaderProgramPhong",
        "VertexShaderPhong",
        "FragmentShaderPhong",
        null,
        null);

      // DONT FORGET TO SELECT YOUR SHADERS WHEN YOU WANT TO USE THEM
      // Use the static class "ShaderManager"

      ShaderManager.SetActiveShader("ShaderProgramBasic");
    }

    public override void InitializeStates()
    {
      // LOAD THE GAME STATES HERE
      // Use the static class "StateManager"

      //StateManager.AddState("gameState", new GameState());
      StateManager.AddState("priorityHeapTesting", new PowerRangerDNA());
      //StateManager.AddState("spriteTesting", new SpriteState());
      //StateManager.AddState("SkyboxTesting", new SkyboxState());
      StateManager.ChangeState("priorityHeapTesting");
    }

    public override void Update(double elapsedTime)
    {
      // DO NOT UPDATE LOW LEVEL GAME LOGIC HERE!!!
      // Only change states as need be with the static "StateManager" class.
      string stateStatus = StateManager.Update((float)elapsedTime);
      // Use the stateStatus string to determine the need for a state change.
      // It defaults to "Don't Change States". An example is if the string is
      // "menuState", you could call - StateManager.ChangeState("menuState").
    }
  }
}
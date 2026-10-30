using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor.Build.Reporting;
using AnimalArchive;

public static class BuildArchive {
 [MenuItem("Animal Archive/V8 Verify all models")]
 public static void VerifyV8(){VoyageVerification.Run();VoyageV8Verification.Run();}
 [MenuItem("Animal Archive/Build Web and verify rules")]
 public static void Build(){
  VerifyV8();VoyageV9Verification.Run();VoyageV10Verification.Run();
  foreach(string path in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/Art","Assets/Resources/Voyage"}).Select(AssetDatabase.GUIDToAssetPath)){
   var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Default;imp.alphaIsTransparency=true;imp.mipmapEnabled=path.Contains("/Voyage/walk-");imp.filterMode=imp.mipmapEnabled?FilterMode.Trilinear:FilterMode.Bilinear;imp.maxTextureSize=(path.Contains("/Voyage/walk-")||path.Contains("facilities-v5")||path.Contains("guide-v5")||path.Contains("human-v6")||path.Contains("ark-")||path.Contains("people-v8"))?1024:2048;if(path.Contains("harbor-v6")||path.Contains("storm-v6"))imp.maxTextureSize=256;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.npotScale=TextureImporterNPOTScale.None;imp.SaveAndReimport();
  }
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(scene,"Assets/Scenes/Archive.unity");
  PlayerSettings.companyName="Animal Archive Prototype";PlayerSettings.productName="ANIMAL ARCHIVE";PlayerSettings.bundleVersion="0.10.2";
  PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;PlayerSettings.runInBackground=false;
  PlayerSettings.colorSpace=ColorSpace.Gamma;PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;PlayerSettings.WebGL.decompressionFallback=true;
  PlayerSettings.SplashScreen.show=false;
  PlayerSettings.WebGL.initialMemorySize=128;PlayerSettings.WebGL.maximumMemorySize=512;PlayerSettings.WebGL.memoryGrowthMode=WebGLMemoryGrowthMode.Geometric;
  PlayerSettings.WebGL.exceptionSupport=WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;PlayerSettings.stripEngineCode=false;
  PlayerSettings.SetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.WebGL,ApiCompatibilityLevel.NET_Standard);
  QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=2;QualitySettings.shadows=ShadowQuality.Disable;
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Archive.unity",true)};AssetDatabase.SaveAssets();
  string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Web"));Directory.CreateDirectory(Path.GetFullPath("../Evidence"));
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Archive.unity"},locationPathName=root,target=BuildTarget.WebGL,options=BuildOptions.None});
  File.WriteAllText(Path.Combine(Application.dataPath,"../../Evidence/build-result.txt"),"UTC "+DateTime.UtcNow.ToString("o")+"\nUnity "+Application.unityVersion+"\nResult "+result.summary.result+"\nBytes "+result.summary.totalSize+"\nDuration "+result.summary.totalTime+"\n");
  if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Web build failed: "+result.summary.result);
  File.Copy("WebShell/index.html",Path.Combine(root,"index.html"),true);
  Directory.CreateDirectory("../Evidence/V102");
  File.Copy("../Evidence/build-result.txt","../Evidence/V102/build-result.txt",true);
  using(var sha=System.Security.Cryptography.SHA256.Create()){
   var manifest=new System.Text.StringBuilder("V10 source/build SHA-256 / "+DateTime.UtcNow.ToString("o")+"\n");
   foreach(string file in Directory.GetFiles("Assets/Scripts","*.cs").Concat(Directory.GetFiles("Assets/Resources/Voyage").Where(f=>!f.EndsWith(".meta"))).Concat(Directory.GetFiles(Path.Combine(root,"Build"))).OrderBy(f=>f))
    manifest.AppendLine(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(file))).Replace("-","").ToLowerInvariant()+"  "+file);
   File.WriteAllText("../Evidence/V102/build-manifest.txt",manifest.ToString());
  }
 }
 static void Assert(bool condition,string name){if(!condition)throw new Exception("RULE TEST FAILED: "+name);}
 public static void Verify(){
  var book=JsonUtility.FromJson<LevelBook>(File.ReadAllText("Assets/Resources/Data/levels.json"));var report=new System.Text.StringBuilder();report.AppendLine("V2 actual C# slide rules / "+DateTime.UtcNow.ToString("o"));
  Assert(book.levels.Length==9,"nine levels");var rewards=new HashSet<int>();
  foreach(var l in book.levels){
   var p=new Puzzle(l);string initial=p.Key();var steps=new List<string>();
   foreach(var r in l.rewards)rewards.Add(r);
   Assert(l.animals.All(a=>l.gates.Any(g=>g.target==a.id)),"every animal has identity exit");
   for(int n=0;n<40&&p.Remaining>0;n++){
    var a=p.FirstSolutionAction(100000);Assert(a!=null,"solvable: "+l.id+" / "+p.Key());
    string before=p.Key();var preview=p.Trace(a[0],a[1],a[2],a[3]);Assert(before==p.Key(),"trace is read only");
    var result=p.Slide(a[0],a[1],a[2],a[3]);Assert(result.valid&&p.positions.SequenceEqual(preview.after),"trace matches commit");Assert(p.positions.Where(v=>!v.gone).Distinct().Count()==p.Remaining,"unique occupancy");
    string after=p.Key();Assert(p.Undo()&&p.Key()==before,"whole gesture undo restores exits and group");p.Slide(a[0],a[1],a[2],a[3]);Assert(p.Key()==after,"deterministic replay");
    steps.Add(string.Join(",",a));
   }
   Assert(p.Remaining==0,"cleared "+l.id);report.AppendLine("Quest "+l.id+": PASS / shortest "+steps.Count+" gestures / "+string.Join(";",steps));
   for(int n=0;n<steps.Count;n++)Assert(p.Undo(),"all undo");Assert(p.Key()==initial,"all undo restores initial");
   Assert(!p.Slide(0,1,1,4).valid&&p.Key()==initial,"diagonal rejected");Assert(!p.Slide(-1,1,0,4).valid,"invalid selection");Assert(!p.Slide(0,1,0,0).valid,"zero drag");
   for(int a=0;a<l.animals.Length;a++)for(int d=0;d<4;d++){
    int dx=new[]{-1,0,0,1}[d],dy=new[]{0,-1,1,0}[d];var one=p.Preview(a,dx,dy);var many=p.Trace(a,dx,dy,99);Assert(one.valid==many.valid,"cannot tunnel through blocked first tile");Assert(p.Key()==initial,"probes immutable");}
   p.Reset();Assert(p.moves==0&&!p.CanUndo&&p.Key()==initial,"reset");
  }
  Assert(rewards.Count==12&&rewards.Min()==0&&rewards.Max()==11,"twelve rewards reachable");var pair=new Puzzle(book.levels[1]);Assert(pair.Group(0).Length==2,"sheep group");Assert(!pair.Trace(0,0,1,4).valid,"one blocked member blocks group");
  report.AppendLine("PASS: 9 solvable stages; 12 unlocks; legal intermediate tiles; immutable preview; atomic multi-tile undo; no overlap; species/identity exits; deterministic replay; reset; linked sheep.");Directory.CreateDirectory("../Evidence");File.WriteAllText("../Evidence/rules-v2.txt",report.ToString());Debug.Log(report.ToString());
 }
}

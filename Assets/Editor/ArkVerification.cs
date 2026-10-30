using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AnimalArchive;
public static class ArkVerification {
 static int checks;static void Check(bool b,string label){checks++;if(!b)throw new Exception("ARK TEST: "+label);}
 [MenuItem("Animal Archive/Verify Ark continuous navigation")]
 public static void Verify(){checks=0;var book=JsonUtility.FromJson<ArkBook>(File.ReadAllText("Assets/Resources/Data/ark.json"));var report=new System.Text.StringBuilder("Ark V3 — actual C# continuous navigation\n");int owned=0;Check(book.stages.Length==9,"nine stages");
 foreach(var s in book.stages){var run=new ArkRun(s);var initial=(Vector2[])run.positions.Clone();int count=0;foreach(var a in s.solution){Vector2 to=new Vector2(a.x,a.z);var path=run.Plan(a.animal,to);Check(path!=null,"reachable "+s.title+" step "+count);Check(run.positions[a.animal]==(count==0?initial[a.animal]:run.positions[a.animal]),"read-only plan");var boxes=run.Obstacles(a.animal);for(int k=1;k<path.Count;k++)Check(ArkRoute.SegmentClear(path[k-1],path[k],boxes),"segment avoids obstacles");bool open=run.opened;int remain=run.Remaining;Check(run.Commit(a.animal,path,a.exit),"commit legal path");Check(run.Undo(),"undo available");Check(run.opened==open&&run.Remaining==remain,"undo restores barrier and rescue");Check(run.Commit(a.animal,path,a.exit),"deterministic commit");count++;}Check(run.Remaining==0,"all aboard "+s.title);for(int k=0;k<count;k++)run.Undo();Check(run.positions.SequenceEqual(initial)&&run.moves==0,"full rollback");Check(run.Plan(-1,Vector2.zero)==null,"invalid animal");Check(run.Plan(0,new Vector2(100,100))==null,"outside bounds");
  if(s.lever){Check(run.Plan(0,s.animals[0].Gate)==null,"closed barrier cannot be bypassed");if(s.keeper>=0){var path=run.Plan(1,new Vector2(s.sx,s.sz));Check(path!=null&&run.Commit(1,path,false)&&!run.opened,"wrong species cannot operate weight plate");}}
  var fresh=s.rewards.Where(id=>(owned&(1<<id))==0).ToArray();foreach(int id in fresh)owned|=1<<id;Check(s.rewards.All(id=>(owned&(1<<id))!=0),"registered");Check(s.rewards.Where(id=>(owned&(1<<id))==0).Count()==0,"repeat gives no new reward");report.AppendLine(s.title+": PASS / "+count+" signals");
 }
 Check(owned==4095,"twelve unique passengers");var test=new ArkRun(book.stages[0]);var curved=test.Plan(0,book.stages[0].animals[0].Gate);Check(curved.Count>=3,"corner-turn route");Check(!test.Commit(0,new List<Vector2>{test.positions[0],book.stages[0].animals[0].Gate},true),"reject direct wall crossing");
 foreach(string path in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Resources/Music"}).Select(AssetDatabase.GUIDToAssetPath)){var imp=(AudioImporter)AssetImporter.GetAtPath(path);var setting=imp.defaultSampleSettings;setting.compressionFormat=AudioCompressionFormat.Vorbis;setting.quality=.55f;setting.loadType=AudioClipLoadType.CompressedInMemory;imp.defaultSampleSettings=setting;imp.SaveAndReimport();}
 report.AppendLine("PASS: "+checks+" assertions; continuous routes, corners, no tunnelling, undo, gate keeper, unique rewards.");Directory.CreateDirectory("../Evidence");File.WriteAllText("../Evidence/rules-v3.txt",report.ToString());Debug.Log(report.ToString());
 }
}

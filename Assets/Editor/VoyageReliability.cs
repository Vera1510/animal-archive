using System;
using System.Linq;
using UnityEngine;
using AnimalArchive.Voyage;
public static class VoyageReliability {
 public static int Run(VoyageConfig cfg){int n=0;Action<bool,string> check=(ok,name)=>{n++;if(!ok)throw new Exception("V4 reliability: "+name);};
  var slow=new VoyageModel(cfg);var regular=new VoyageModel(cfg);for(int k=0;k<100;k++)slow.AdvanceFrame(1);for(int k=0;k<1000;k++)regular.AdvanceFrame(.1f);
  check(Math.Abs(slow.state.elapsed-regular.state.elapsed)<.02f,"1fps preserves elapsed time");check(slow.state.total==regular.state.total&&slow.state.boarded==regular.state.boarded&&slow.state.nextId==regular.state.nextId,"1fps and 10fps yield same real-time lifecycle");
  float before=slow.state.elapsed;slow.AdvanceFrame(float.NaN);slow.AdvanceFrame(float.PositiveInfinity);check(slow.state.elapsed==before,"invalid frame times do not advance");
  var m=new VoyageModel(cfg);for(int k=0;k<150;k++)m.Tick(.1f);
  check(m.state.active.Count>0,"in-flight save fixture");var original=m.state.active.Select(p=>p.id).ToArray();var rewards=m.state.active.Select(p=>p.reward).ToArray();
  string encoded=VoyageSave.Encode(m.state);var copy=VoyageSave.Decode(cfg,encoded);check(copy!=null&&copy.active.Select(p=>p.id).SequenceEqual(original),"save retains active IDs");check(copy.active.Select(p=>p.reward).SequenceEqual(rewards),"save retains frozen rewards");
  check(VoyageSave.Decode(cfg,encoded.Replace("132941","999999"))!=null||!encoded.Contains("132941"),"unchanged save remains decodable");
  check(VoyageSave.Decode(cfg,encoded.Substring(0,encoded.Length/2))==null,"truncated save rejected");
  var e=JsonUtility.FromJson<SaveEnvelope>(encoded);e.body=e.body.Replace("\"available\":0","\"available\":9999");check(VoyageSave.Decode(cfg,JsonUtility.ToJson(e))==null,"checksum rejects altered balances");
  var resumed=new VoyageModel(cfg,copy);for(int k=0;k<1000;k++){m.Tick(.1f);resumed.Tick(.1f);m.arrivals.Clear();resumed.arrivals.Clear();m.events.Clear();resumed.events.Clear();}
  check(JsonUtility.ToJson(m.state)==JsonUtility.ToJson(resumed.state),"resume is deterministic, including RNG and active rewards");
  long total=m.state.total;int known=m.state.discovered;check(!m.Buy(99)&&!m.SetAuto(true,4),"early automation and invalid purchase rejected");m.Tick(0);m.Tick(float.NaN);m.Tick(float.PositiveInfinity);check(m.state.total==total&&m.state.discovered==known,"invalid ticks cannot credit");
  var active=m.state.active[0];int reward=active.reward;while(m.state.available<24)m.Tick(.2f);check(m.Buy(2)&&active.reward==reward,"value upgrade cannot rewrite active reward");
  check(VoyageModel.Validate(cfg,m.state,out _),"ledger valid after live upgrade");var bad=JsonUtility.FromJson<VoyageState>(JsonUtility.ToJson(m.state));bad.active[0].duration=float.PositiveInfinity;check(!VoyageModel.Validate(cfg,bad,out _),"infinite duration rejected");
  bad=JsonUtility.FromJson<VoyageState>(JsonUtility.ToJson(m.state));bad.active.Add(bad.active[0]);check(!VoyageModel.Validate(cfg,bad,out _),"duplicated in-flight ID rejected");
  var random=new VoyageModel(cfg);for(int k=0;k<20000;k++){random.Tick(.1f);if(k%37==0)random.Buy((k/37)%4);if(random.TwilightReady)random.LightLantern();if(random.AutoUnlocked)random.SetAuto(k%701<650,(k/701)%5);if(k%313==0){string snap=VoyageSave.Encode(random.state);var restored=VoyageSave.Decode(cfg,snap);check(restored!=null,"randomized snapshot "+k);random=new VoyageModel(cfg,restored);}random.arrivals.Clear();random.events.Clear();}
  check(random.state.available>=0&&random.state.total==random.state.available+random.state.spent,"mixed auto/manual spending conserves all balances");check(random.state.paidTail.Count<=cfg.maxActive,"settlement ledger stays bounded");
  int[] counts=new int[cfg.animals.Length];for(int k=0;k<100000;k++)counts[random.Choose()]++;for(int i=0;i<counts.Length;i++)check(Math.Abs(counts[i]/100000f-random.Probability(i))<.009f,"weighted distribution matches declared chance "+i);
  return n;
 }
}

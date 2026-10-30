using UnityEngine;
namespace AnimalArchive.Voyage {
 public partial class VoyageModel {
  public int populationLimit;
  public int ActiveLimit=>Mathf.Min(config.maxActive,populationLimit>0?Mathf.Max(3,populationLimit):config.maxActive);
  void CompactLedger(){state.ledgerV10=1;state.paidPrefix=state.boarded+state.lost;state.paidTail.Clear();}
 }
 public partial class VoyageState {public int ledgerV10;}
 public partial class VoyageGame {
  int populationBudget;float performanceClock,recoveryClock;
  void UpdatePopulationBudget(float dt){
   int ceiling=Application.isMobilePlatform||Mathf.Min(Screen.width,Screen.height)<500?40:64;
   if(populationBudget==0)populationBudget=ceiling;
   populationBudget=Mathf.Min(populationBudget,ceiling);
   if(screen=="play"&&!paused&&!background&&story<0&&S.mode==0){
    performanceClock+=dt;recoveryClock+=dt;
    if(performanceClock>=5){performanceClock=0;if(fps<28){populationBudget=Mathf.Max(24,populationBudget-8);recoveryClock=0;}else if(fps>52&&recoveryClock>=20){populationBudget=Mathf.Min(ceiling,populationBudget+4);recoveryClock=0;}}
   }
   // Lowering the budget never deletes animals already present; new arrivals wait.
   model.populationLimit=populationBudget;
  }
 }
}

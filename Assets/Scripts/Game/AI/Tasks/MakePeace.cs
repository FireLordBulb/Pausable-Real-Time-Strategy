using System.Linq;
using Simulation;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "MakePeace", menuName = "ScriptableObjects/AI/Tasks/MakePeace")]
	public class MakePeace : Task {
		[Space]
		[SerializeField] private int playerRequiredAcceptance;
		[SerializeField] private float monthlyLandDemandDecrease;
		[SerializeField, Min(0.01f)] private float goldTransferChunkSize;
		
		private WarEnemy peaceTarget;
		private AIController peaceTargetAI;
		private DiplomaticStatus diplomaticStatus;
		private PeaceTreaty peaceTreaty;
		
		internal AIController PeaceTargetAI => peaceTargetAI;
		
		internal void Init(AIController controller, WarEnemy enemy, AIController enemyAI){
			Init(controller);
			peaceTarget = enemy; 
			peaceTargetAI = enemyAI; 
			diplomaticStatus = Country.GetDiplomaticStatus(peaceTarget.Country);
			peaceTreaty = Country.NewPeaceTreaty(peaceTarget.Country);
			peaceTreaty.DidTreatyInitiatorWin = true;
			peaceTreaty.IsWhitePeace = false;
		}
		protected override int CurrentPriority(){
			if (!diplomaticStatus.IsAtWar || !peaceTarget.Country.enabled){
				return defaultPriority;
			}
			peaceTreaty.AnnexedLands.Clear();
			int occupiedProvinces = Country.Occupations.Count(land => land.Owner == peaceTarget.Country);
			if (occupiedProvinces == 0){
				Land singleDemand = peaceTarget.ClosestProvinces.FirstOrDefault(land => land.Owner == peaceTarget.Country && !land.IsOccupied);
				if (singleDemand != null){
					peaceTreaty.AnnexedLands.Add(singleDemand);
				}
			} else foreach (Land land in peaceTarget.ClosestProvinces){
				// Demand all occupied lands or a single unoccupied land if none are occupied.
				if (land.Owner == peaceTarget.Country && land.Occupier == Country){
					peaceTreaty.AnnexedLands.Add(land);
				}
			}
			// As the war grows longer, gradually become more willing to not demand every single occupied province.
			float demandProportion = Mathf.Clamp01(1-monthlyLandDemandDecrease*peaceTarget.MonthsOfWar);
			int minAcceptableLands = Mathf.CeilToInt(peaceTreaty.AnnexedLands.Count*demandProportion);
			while (minAcceptableLands < peaceTreaty.AnnexedLands.Count && !WouldAccept()){
				peaceTreaty.AnnexedLands.RemoveAt(peaceTreaty.AnnexedLands.Count-1);
			}
			// Demand as much extra gold as would be accepted.
			float acceptableGold = 0;
			peaceTreaty.GoldTransfer = 0;
			while (acceptableGold <= peaceTarget.Country.Gold && WouldAccept()){
				acceptableGold = peaceTreaty.GoldTransfer;
				peaceTreaty.GoldTransfer += goldTransferChunkSize;
			}
			peaceTreaty.GoldTransfer = acceptableGold;
			return defaultPriority;
		}
		internal override bool CanBePerformed(){
			return diplomaticStatus.IsAtWar && WouldAccept();
		}
		private bool WouldAccept(){
			return peaceTargetAI.EvaluatePeaceOffer(peaceTreaty) > (peaceTargetAI.enabled ? 0 : playerRequiredAcceptance);
		}
		internal override void Perform(){
			Country.EndWar(peaceTarget.Country, peaceTreaty);
			AIController.OnWarEnd(Controller, peaceTargetAI);
		}
	}
}

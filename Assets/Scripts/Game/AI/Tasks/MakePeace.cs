using Simulation;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "MakePeace", menuName = "ScriptableObjects/AI/Tasks/MakePeace")]
	public class MakePeace : Task {
		[Space]
		[SerializeField, Min(0.01f)] private float goldTransferChunkSize;
		
		private Country peaceTarget;
		private AIController peaceTargetAI;
		private DiplomaticStatus diplomaticStatus;
		private PeaceTreaty peaceTreaty;
		
		internal AIController PeaceTargetAI => peaceTargetAI;
		
		internal void Init(AIController controller, AIController enemyAI){
			Init(controller);
			peaceTarget = enemyAI.Country; 
			peaceTargetAI = enemyAI; 
			diplomaticStatus = Country.GetDiplomaticStatus(peaceTarget);
			peaceTreaty = Country.NewPeaceTreaty(peaceTarget);
			peaceTreaty.DidTreatyInitiatorWin = true;
			peaceTreaty.IsWhitePeace = false;
		}
		protected override int CurrentPriority(){
			if (!diplomaticStatus.IsAtWar){
				return defaultPriority;
			}
			peaceTreaty.AnnexedLands.Clear();
			foreach (Land land in Controller.GetClosestProvinces(peaceTarget)){
				// Demand all occupied lands or a single unoccupied land if none are occupied.
				if (land.Owner == peaceTarget && (land.Occupier == Country || peaceTreaty.AnnexedLands.Count == 0 && !land.IsOccupied)){
					peaceTreaty.AnnexedLands.Add(land);
				}
			}
			// Demand as much extra gold as would be accepted.
			float acceptableGold = 0;
			peaceTreaty.GoldTransfer = 0;
			while (acceptableGold <= peaceTarget.Gold && peaceTargetAI.EvaluatePeaceOffer(peaceTreaty) > 0){
				acceptableGold = peaceTreaty.GoldTransfer;
				peaceTreaty.GoldTransfer += goldTransferChunkSize;
			}
			peaceTreaty.GoldTransfer = acceptableGold;
			return defaultPriority;
		}
		internal override bool CanBePerformed(){
			return diplomaticStatus.IsAtWar && peaceTargetAI.EvaluatePeaceOffer(peaceTreaty) > 0;
		} 
		internal override void Perform(){
			Country.EndWar(peaceTarget, peaceTreaty);
			AIController.OnWarEnd(Controller, peaceTargetAI);
		}
	}
}

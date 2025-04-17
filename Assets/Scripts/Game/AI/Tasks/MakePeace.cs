using Simulation;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "MakePeace", menuName = "ScriptableObjects/AI/Tasks/MakePeace")]
	public class MakePeace : Task {
		private Country peaceTarget;
		private AIController peaceTargetAI;
		private PeaceTreaty peaceTreaty;
		private bool hasWarEnded;
		
		internal AIController PeaceTargetAI => peaceTargetAI;
		
		internal void Init(AIController controller, AIController enemyAI){
			Init(controller);
			peaceTarget = enemyAI.Country; 
			peaceTargetAI = enemyAI;
			peaceTreaty = Country.NewPeaceTreaty(peaceTarget);
			peaceTreaty.DidTreatyInitiatorWin = true;
			peaceTreaty.IsWhitePeace = false;
		}
		protected override int CurrentPriority(){
			peaceTreaty.AnnexedLands.Clear();
			peaceTreaty.GoldTransfer = peaceTarget.Gold;
			return defaultPriority;
		}
		internal override bool CanBePerformed(){
			return !hasWarEnded && peaceTargetAI.EvaluatePeaceOffer(peaceTreaty) > 0;
		} 
		internal override void Perform(){
			Country.EndWar(peaceTarget, peaceTreaty);
			AIController.OnWarEnd(Controller, peaceTargetAI);
			hasWarEnded = true;
		}
	}
}

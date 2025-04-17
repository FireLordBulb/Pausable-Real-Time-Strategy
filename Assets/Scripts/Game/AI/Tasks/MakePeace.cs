using Simulation;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "MakePeace", menuName = "ScriptableObjects/AI/Tasks/MakePeace")]
	public class MakePeace : Task {
		[Space]
		private Country peaceTarget;
		private AIController peaceTargetAI;
		private PeaceTreaty peaceTreaty;

		internal AIController PeaceTargetAI => peaceTargetAI;
		
		internal void Init(AIController controller, AIController enemyAI){
			Init(controller);
			peaceTarget = enemyAI.Country; 
			peaceTargetAI = enemyAI;
			peaceTreaty = Country.NewPeaceTreaty(peaceTarget);
		}
		protected override int CurrentPriority(){
			return defaultPriority;
		}
		internal override bool CanBePerformed(){
			return peaceTargetAI.EvaluatePeaceOffer(peaceTreaty) > 0;
		} 
		internal override void Perform(){
			Country.EndWar(peaceTarget, peaceTreaty);
			AIController.OnWarEnd(Controller, peaceTargetAI);
		}
	}
}

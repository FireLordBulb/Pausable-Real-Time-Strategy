using System.Linq;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "DeclareWar", menuName = "ScriptableObjects/AI/Tasks/DeclareWar")]
	public class DeclareWar : Task {
		[SerializeField] private int neverDoPriority;
		[Space]
		[SerializeField] private float requiredRegimentsPerProvince;
		[SerializeField] private float maxRequiredRegiments;
		[Space]
		[SerializeField] private float maxRegimentProportion;
		[SerializeField] private float assumedMinRegimentsPerProvince;
		[SerializeField] private float enemyOfEnemyFactor;
		private Country warTarget;
		private AIController warTargetAI;
		
		protected override int CurrentPriority(){
			warTarget = null;
			if (Controller.WarEnemies.Count != 0){
				return neverDoPriority;
			}
			float regiments = Country.Regiments.Count;
			if (regiments < Mathf.Min(requiredRegimentsPerProvince*Country.ProvinceCount, maxRequiredRegiments)){
				return neverDoPriority;
			}
			float maxTargetRegiments = regiments*maxRegimentProportion;
			float weakestTargetFound = float.MaxValue;
			foreach (Country borderingCountry in Controller.BorderingCountries){
				if (!Country.GetDiplomaticStatus(borderingCountry).CanDeclareWar()){
					continue;
				}
				float borderingCountryRegiments = Mathf.Max(borderingCountry.Regiments.Count, borderingCountry.ProvinceCount*assumedMinRegimentsPerProvince);
				AIController borderingAI = borderingCountry.GetComponent<AIController>();
				float borderingCountryEnemiesRegiments = borderingAI.WarEnemies.Sum(country => country.Regiments.Count);
				borderingCountryRegiments -= borderingCountryEnemiesRegiments*enemyOfEnemyFactor;
				if (weakestTargetFound < borderingCountryRegiments || maxTargetRegiments < borderingCountryRegiments){
					continue;
				}
				weakestTargetFound = borderingCountryRegiments;
				warTarget = borderingCountry;
				warTargetAI = borderingAI;
			}
			return warTarget == null ? neverDoPriority : defaultPriority;
		}
		public override bool CanBePerformed(){
			return warTarget != null && Country.GetDiplomaticStatus(warTarget).CanDeclareWar();
		}
		public override void Perform(){
			Country.GetDiplomaticStatus(warTarget).DeclareWar();
			AIController.OnWarStart(Controller, warTargetAI);
		}
	}
}

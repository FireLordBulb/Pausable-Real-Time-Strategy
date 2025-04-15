using System.Collections.Generic;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI {
	[RequireComponent(typeof(Country))]
	public class AIController : MonoBehaviour {
		[SerializeField] private PeaceAcceptance peaceAcceptance;
		
		private Calendar calendar;
		private readonly List<Country> warEnemies = new();
		
		public Country Country {get; private set;}
		
		public void Init(){
			Country = GetComponent<Country>();
			calendar = Country.Map.Calendar;
			enabled = true;
		}
		private void OnEnable(){
			calendar.OnDayTick.AddListener(DayTick);
			calendar.OnMonthTick.AddListener(MonthTick);
			calendar.OnYearTick.AddListener(YearTick);
			foreach (Regiment regiment in Country.Regiments){
				regiment.GetComponent<RegimentBrain>().enabled = true;
			}
			foreach (Ship ship in Country.Ships){
				ship.GetComponent<ShipBrain>().enabled = true;
			}
		}
		private void OnDisable(){
			calendar.OnDayTick.RemoveListener(DayTick);
			calendar.OnMonthTick.RemoveListener(MonthTick);
			calendar.OnYearTick.RemoveListener(YearTick);
			foreach (Regiment regiment in Country.Regiments){
				regiment.GetComponent<RegimentBrain>().enabled = false;
			}
			foreach (Ship ship in Country.Ships){
				ship.GetComponent<ShipBrain>().enabled = false;
			}
		}

		private void DayTick(){
			
		}
		private void MonthTick(){
			
		}
		private void YearTick(){
			
		}

		public static void OnWarStart(AIController declarer, AIController target){
			declarer.OnWarStart(target);
			target.OnWarStart(declarer);
		}
		private void OnWarStart(AIController other){
			warEnemies.Add(other.Country);
			RegroupRegiments();
		}
		public static void OnWarEnd(AIController initiator, AIController receiver){
			initiator.OnWarEnd(receiver);
			receiver.OnWarEnd(initiator);
		}
		public void OnWarEnd(AIController other){
			warEnemies.Remove(other.Country);
			RegroupRegiments();
		}
		private void RegroupRegiments(){
			for (int i = 0; i < Country.Regiments.Count; i++){
				Regiment regiment = Country.Regiments[i];
				RegimentBrain brain = regiment.GetComponent<RegimentBrain>();
				Country enemy = warEnemies.Count == 0 ? null : warEnemies[i*warEnemies.Count/Country.Regiments.Count];
				brain.Tree.Blackboard.SetValue(brain.EnemyCountry, enemy);
			}
		}
		
		public int EvaluatePeaceOffer(PeaceTreaty treaty){
			return peaceAcceptance.EvaluatePeaceOffer(treaty);
		}
	}
}

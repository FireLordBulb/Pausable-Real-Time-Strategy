using System.Collections.Generic;
using System.Linq;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI {
	[RequireComponent(typeof(Country))]
	public class AIController : MonoBehaviour {
		[SerializeField] private PeaceAcceptance peaceAcceptance;
		
		private Calendar calendar;
		private readonly List<Country> warEnemies = new();
		private readonly Dictionary<Country, List<Province>> enemiesClosestProvinces = new();
		
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
		// Recalculate this occasionally since both sides could have gained/lost land in other wars.
		private void YearTick(){
			foreach (Country warEnemy in warEnemies){
				CalculateClosestProvinces(warEnemy);
			}
		}

		public static void OnWarStart(AIController declarer, AIController target){
			declarer.OnWarStart(target);
			target.OnWarStart(declarer);
		}
		private void OnWarStart(AIController other){
			Country enemy = other.Country;
			warEnemies.Add(enemy);
			enemiesClosestProvinces.Add(enemy, new List<Province>());
			CalculateClosestProvinces(enemy);
			RegroupRegiments();
		}
		public static void OnWarEnd(AIController initiator, AIController receiver){
			initiator.OnWarEnd(receiver);
			receiver.OnWarEnd(initiator);
		}
		public void OnWarEnd(AIController other){
			warEnemies.Remove(other.Country);
			enemiesClosestProvinces.Remove(other.Country);
			RegroupRegiments();
		}
		private void RegroupRegiments(){
			for (int i = 0; i < Country.Regiments.Count; i++){
				Regiment regiment = Country.Regiments[i];
				RegimentBrain brain = regiment.GetComponent<RegimentBrain>();
				if (warEnemies.Count == 0){
					brain.Tree.Blackboard.RemoveValue(brain.EnemyCountry);
				} else {
					Country enemy = warEnemies[i*warEnemies.Count/Country.Regiments.Count];
					brain.Tree.Blackboard.SetValue(brain.EnemyCountry, enemy);
				}
			}
		}
		// Heavy calculation, don't do often.
		private void CalculateClosestProvinces(Country enemy){
			const float speedIsIrrelevantForSorting = 1;
			LandLocation capital = Country.Capital.ArmyLocation;
			List<Province> closestProvinces = enemiesClosestProvinces[enemy];
			closestProvinces.Clear();
			Dictionary<Province, int> distances = new();
			foreach (Land land in enemy.Provinces){
				closestProvinces.Add(land.Province);
				List<ProvinceLink> path = Regiment.GetPath(capital, land.ArmyLocation, link => Regiment.LinkEvaluator(link, false, Country));
				distances[land.Province] = path.Sum(link => Regiment.GetTravelDays(link, speedIsIrrelevantForSorting));
			}
			closestProvinces.Sort((left, right) => distances[left]-distances[right]);
		}
		
		public int EvaluatePeaceOffer(PeaceTreaty treaty){
			return peaceAcceptance.EvaluatePeaceOffer(treaty);
		}

		internal IReadOnlyList<Province> GetClosestProvinces(Country country){
			return enemiesClosestProvinces[country];
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI {
	[RequireComponent(typeof(Country))]
	public class AIController : MonoBehaviour {
		[SerializeField] private PeaceAcceptance peaceAcceptance;
		[SerializeField] private Task[] monthlyTasks;
		[SerializeField] private Task[] yearlyTasks;
		[SerializeField] private int maxTasksPerTick;
		[SerializeField] private float maxStrengthMultiplier;
		
		private Calendar calendar;
		private Task[] allTasks;
		private readonly List<Country> warEnemies = new();
		private readonly Dictionary<Country, List<Land>> enemiesClosestProvinces = new();
		private readonly List<Land> borderProvinces = new();
		
		public Country Country {get; private set;}
		
		public void Init(){
			Country = GetComponent<Country>();
			Country.RegimentBuiltAddListener(_ => RegroupRegiments());
			calendar = Country.Map.Calendar;
			enabled = true;
			CalculateBorderProvinces();
			allTasks = new Task[monthlyTasks.Length+yearlyTasks.Length];
			InitTasks(monthlyTasks, 0);
			InitTasks(yearlyTasks, monthlyTasks.Length);
		}
		private void InitTasks(Task[] tasks, int offset){
			for (int i = 0; i < tasks.Length; i++){
				tasks[i] = Instantiate(tasks[i]);
				tasks[i].Init(this);
				allTasks[i+offset] = tasks[i];
			}
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
			UpdateTasks(monthlyTasks);
			SortTasks();
			PerformTasks();
		}
		// Recalculate this occasionally since both sides could have gained/lost land in other wars.
		private void YearTick(){
			foreach (Country warEnemy in warEnemies){
				CalculateClosestProvinces(warEnemy);
			}
			UpdateTasks(yearlyTasks);
		}
		private static void UpdateTasks(Task[] tasks){
			foreach (Task task in tasks){
				task.RecalculatePriority();
			}
		}
		private void SortTasks(){
			Array.Sort(allTasks);
		}
		private void PerformTasks(){
			int count = 0;
			int taskIndex = 0;
			while (taskIndex < allTasks.Length && count < maxTasksPerTick){
				while (!allTasks[taskIndex].CanBePerformed() && taskIndex < allTasks.Length-1){
					taskIndex++;
				}
				if (!allTasks[taskIndex].CanBePerformed()){
					break;
				}
				allTasks[taskIndex].Perform();
				allTasks[taskIndex].RecalculatePriority();
				// Shift the performed task down in the list based on its new priority without re-sorting the entire list.
				for (int shiftIndex = taskIndex+1; shiftIndex < allTasks.Length && allTasks[shiftIndex-1].CompareTo(allTasks[shiftIndex]) <= 0; shiftIndex++){
					(allTasks[shiftIndex-1], allTasks[shiftIndex]) = (allTasks[shiftIndex], allTasks[shiftIndex-1]);
				}
				count++;
			}
		}
		
		public static void OnWarStart(AIController declarer, AIController target){
			declarer.OnWarStart(target);
			target.OnWarStart(declarer);
		}
		private void OnWarStart(AIController other){
			Country enemy = other.Country;
			warEnemies.Add(enemy);
			enemiesClosestProvinces.Add(enemy, new List<Land>());
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
			CalculateBorderProvinces();
			RegroupRegiments();
		}
		private void CalculateBorderProvinces(){
			borderProvinces.Clear();
			foreach (Land province in Country.Provinces){
				if (province.Province.Links.Any(link => link.Target.IsLand && link.Target.Land.Owner != Country)){
					borderProvinces.Add(province);
				}
			}
		}
		private void RegroupRegiments(){
			for (int i = 0; i < Country.Regiments.Count; i++){
				Regiment regiment = Country.Regiments[i];
				RegimentBrain brain = regiment.GetComponent<RegimentBrain>();
				if (warEnemies.Count == 0){
					brain.Tree.Blackboard.RemoveValue(brain.EnemyCountry);
					if (borderProvinces.Count > 0){
						brain.Tree.Blackboard.SetValue(brain.Target, borderProvinces[i%borderProvinces.Count].Province);
					}
				} else {
					Country enemy = warEnemies[i*warEnemies.Count/Country.Regiments.Count];
					brain.Tree.Blackboard.SetValue(brain.EnemyCountry, enemy);
				}
			}
		}
		// Heavy calculation, don't do often.
		private void CalculateClosestProvinces(Country enemy){
			List<Land> closestProvinces = enemiesClosestProvinces[enemy];
			closestProvinces.Clear();
			Dictionary<Land, int> distances = new();
			AddLandDistances(enemy.Provinces, closestProvinces, distances);
			// Add the provinces of the own country so that armies will unoccupy provinces besieged by enemies. 
			AddLandDistances(Country.Provinces, closestProvinces, distances);
			
			closestProvinces.Sort((left, right) => distances[left]-distances[right]);
		}
		private void AddLandDistances(IEnumerable<Land> lands, List<Land> provinces, Dictionary<Land, int> distances){
			const float speedIsIrrelevantForSorting = 1;
			LandLocation capital = Country.Capital.ArmyLocation;
			foreach (Land land in lands){
				provinces.Add(land);
				List<ProvinceLink> path = Regiment.GetPath(capital, land.ArmyLocation, link => Regiment.LinkEvaluator(link, false, Country));
				if (path == null){
					distances[land] = int.MaxValue;
					continue;
				}
				distances[land] = path.Sum(link => Regiment.GetTravelDays(link, speedIsIrrelevantForSorting));
			}
		}
		
		public int EvaluatePeaceOffer(PeaceTreaty treaty){
			return peaceAcceptance.EvaluatePeaceOffer(treaty);
		}

		internal IReadOnlyList<Land> GetClosestProvinces(Country country){
			return enemiesClosestProvinces[country];
		}
		
		internal bool HasBesiegerAlready(Land land, Regiment regiment){
			foreach (Regiment presentRegiment in land.ArmyLocation.Units){
				// If the regiment itself is present in the siege and it's first in the enumeration order, it gets to stay.
				if (presentRegiment == regiment){
					return false;
				}
				if (!presentRegiment.IsMoving && presentRegiment.Owner == Country){
					return true;
				}
			}
			return false;
		}
		internal bool ShouldAvoidArmyAt(Province province, Regiment regiment){
			IReadOnlyList<Regiment> unitsAtLocation = province.Land.ArmyLocation.Units;
			if (unitsAtLocation.All(unit => unit.Owner == Country)){
				return false;
			}
			Regiment[] enemyRegiments = unitsAtLocation.Where(unit => unit.Owner != Country).ToArray();
			return maxStrengthMultiplier <= RelativeStrength(regiment, enemyRegiments);
		}
		internal static float RelativeStrength(Regiment attacker, params Regiment[] defenders){
			float defenderStrength = defenders.Sum(RegimentStrength);
			float attackerStrength = RegimentStrength(attacker);

			Land battleLand = defenders[0].Province.Land;
			// Attacker gets the defender advantage if they control the province the defender is in.
			if (battleLand.Controller == attacker.Owner){
				attackerStrength *= 1+battleLand.Terrain.DefenderAdvantage;
			// Otherwise the defender gets the defender advantage as expected.
			} else {
				defenderStrength *= 1+battleLand.Terrain.DefenderAdvantage;
			}
			return defenderStrength/attackerStrength;
		}
		internal static float RegimentStrength(Regiment regiment){
			return regiment.IsBuilt ? (regiment.CurrentManpower+regiment.DemoralizedManpower)*(regiment.AttackPower+regiment.Toughness) : 0;
		}
	}
}

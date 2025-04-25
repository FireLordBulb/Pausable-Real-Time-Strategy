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
		[SerializeField] private MakePeace makePeace;
		[SerializeField] private int maxTasksPerTick;
		[SerializeField] private float maxStrengthMultiplier;
		
		private Calendar calendar;
		private readonly List<Task> allTasks = new();
		private readonly List<Task> monthlyTaskList = new();
		private readonly List<Task> yearlyTaskList = new();
		private readonly Queue<(Task task, bool isAdded)> monthlyTaskChanges = new();
		
		private readonly List<WarEnemy> warEnemies = new();
		private readonly List<WarEnemy> overseasWarEnemies = new();
		private readonly List<Land> borderProvinces = new();
		private readonly List<Harbor> harbors = new();
		private readonly HashSet<Country> borderingCountries = new();

		private readonly Dictionary<Regiment, RegimentBrain> regimentBrains = new();
		private readonly Dictionary<Ship, ShipBrain> shipBrains = new();
		
		internal IReadOnlyList<WarEnemy> WarEnemies => warEnemies;
		public IEnumerable<Country> BorderingCountries => borderingCountries;
		
		public Country Country {get; private set;}
		public bool HasCoast {get; private set;}
		
		public void Init(){
			Country = GetComponent<Country>();
			Country.RegimentBuiltAddListener(regiment => {
				if (regiment == null){
					return;
				}
				regimentBrains.Add(regiment, regiment.GetComponent<RegimentBrain>());
				RegroupRegiments();
				regiment.enabled = enabled;
			});
			Country.ShipBuiltAddListener(ship => {
				if (ship == null){
					return;
				}
				shipBrains.Add(ship, ship.GetComponent<ShipBrain>());
				RegroupShips();
				ship.enabled = enabled;
			});
			calendar = Country.Map.Calendar;
			enabled = true;
			CalculateBorderProvinces();
			InitTasks(monthlyTasks, monthlyTaskList);
			InitTasks(yearlyTasks, yearlyTaskList);
		}
		private void InitTasks(Task[] taskArray, List<Task> taskList){
			foreach (Task task in taskArray){
				Task taskInstance = Instantiate(task);
				taskInstance.Init(this);
				taskList.Add(taskInstance);
				allTasks.Add(taskInstance);
			}
		}
		private void OnEnable(){
			calendar.OnMonthTick.AddListener(MonthTick);
			calendar.OnYearTick.AddListener(YearTick);
			RefreshBrainDictionaries();
			foreach (Regiment regiment in Country.Regiments){
				if (regiment.IsBuilt){
					GetBrain(regiment).enabled = true;
				}
			}
			foreach (Ship ship in Country.Ships){
				if (ship.IsBuilt){
					GetBrain(ship).enabled = true;
				}
			}
		}
		private void OnDisable(){
			calendar.OnMonthTick.RemoveListener(MonthTick);
			calendar.OnYearTick.RemoveListener(YearTick);
			RefreshBrainDictionaries();
			foreach (Regiment regiment in Country.Regiments){
				if (regiment.IsBuilt){
					GetBrain(regiment).enabled = false;
				}
			}
			foreach (Ship ship in Country.Ships){
				if (ship.IsBuilt){
					GetBrain(ship).enabled = false;
				}
			}
		}
		
		private void MonthTick(){
			if (!enabled){
				return;
			}
			foreach (WarEnemy warEnemy in warEnemies){
				warEnemy.TickMonth();
			}
			while (monthlyTaskChanges.Count > 0){
				(Task peaceNegotiations, bool isAdded) = monthlyTaskChanges.Dequeue();
				if (isAdded){
					allTasks.Add(peaceNegotiations);
					monthlyTaskList.Add(peaceNegotiations);
				} else {
					allTasks.Remove(peaceNegotiations);
					monthlyTaskList.Remove(peaceNegotiations);
				}
			}
			UpdateTasks(monthlyTaskList);
			SortTasks();
			PerformTasks();
		}
		// Recalculate this occasionally since both sides could have gained/lost land in other wars.
		private void YearTick(){
			if (!enabled){
				return;
			}
			RefreshBrainDictionaries();
			CalculateBorderProvinces();
			overseasWarEnemies.Clear();
			foreach (WarEnemy warEnemy in warEnemies){
				CalculateClosestProvinces(warEnemy);
			}
			UpdateTasks(yearlyTaskList);
		}
		private void RefreshBrainDictionaries(){
			foreach (Regiment key in regimentBrains.Keys.ToArray()){
				if (key == null){
					regimentBrains.Remove(key);
				}
			}
			foreach (Ship key in shipBrains.Keys.ToArray()){
				if (key == null){
					shipBrains.Remove(key);
				}
			}
		}
		private static void UpdateTasks(List<Task> tasks){
			foreach (Task task in tasks){
				task.RecalculatePriority();
			}
		}
		private void SortTasks(){
			allTasks.Sort();
		}
		private void PerformTasks(){
			int count = 0;
			int taskIndex = 0;
			while (taskIndex < allTasks.Count && count < maxTasksPerTick){
				while (!allTasks[taskIndex].CanBePerformed() && taskIndex < allTasks.Count-1){
					taskIndex++;
				}
				if (!allTasks[taskIndex].CanBePerformed()){
					break;
				}
				allTasks[taskIndex].Perform();
				allTasks[taskIndex].RecalculatePriority();
				// Shift the performed task down in the list based on its new priority without re-sorting the entire list.
				for (int shiftIndex = taskIndex+1; shiftIndex < allTasks.Count && allTasks[shiftIndex-1].CompareTo(allTasks[shiftIndex]) <= 0; shiftIndex++){
					(allTasks[shiftIndex-1], allTasks[shiftIndex]) = (allTasks[shiftIndex], allTasks[shiftIndex-1]);
				}
				count++;
			}
		}
		
		public static void OnWarStart(AIController declarer, AIController target){
			// If you declare war on a country at the tick it gets full annexed, ignore the declaration.
			if (!target.enabled){
				return;
			}
			declarer.OnWarStart(target);
			target.OnWarStart(declarer);
		}
		private void OnWarStart(AIController other){
			WarEnemy enemy = new(this, other.Country);
			warEnemies.Add(enemy);
			CalculateClosestProvinces(enemy);
			RegroupUnits();
			MakePeace peaceNegotiations = Instantiate(makePeace);
			peaceNegotiations.Init(this);
			peaceNegotiations.Init(this, enemy, other);
			monthlyTaskChanges.Enqueue((peaceNegotiations, true));
		}
		public static void OnWarEnd(AIController initiator, AIController receiver){
			initiator.OnWarEnd(receiver);
			receiver.OnWarEnd(initiator);
		}
		public void OnWarEnd(AIController other){
			warEnemies.RemoveAll(enemy => enemy.Country == other.Country);
			overseasWarEnemies.RemoveAll(enemy => enemy.Country == other.Country);
			Task peaceNegotiations = allTasks.Find(task => task is MakePeace peaceNegotiations && peaceNegotiations.PeaceTargetAI == other);
			monthlyTaskChanges.Enqueue((peaceNegotiations, false));
			if (!Country.enabled){
				enabled = false;
				foreach (WarEnemy warEnemy in warEnemies.ToArray()){
					Country.EndWar(warEnemy.Country, Country.NewPeaceTreaty(warEnemy.Country));
				}
				return;
			}
			CalculateBorderProvinces();
			RegroupUnits();
		}
		private void CalculateBorderProvinces(){
			borderProvinces.Clear();
			harbors.Clear();
			borderingCountries.Clear();
			HasCoast = false;
			foreach (Land province in Country.Provinces){
				bool isBorderProvince = false;
				HasCoast |= province.Province.IsCoast;
				foreach (ProvinceLink link in province.Province.Links){
					if (link is ShallowsLink shallowsLink){
						harbors.Add(shallowsLink.Harbor);
						// Countries that are a single sea tile away count as bordering.
						foreach (ProvinceLink seaLink in link.Target.Links){
							if (seaLink is CoastLink coastLink && coastLink.Land.Owner != Country){
								borderingCountries.Add(coastLink.Land.Owner);
							}
						}
						continue;
					}
					if (link.Target.Land.Owner == Country){
						continue;
					}
					borderingCountries.Add(link.Target.Land.Owner);
					if (isBorderProvince){
						continue;
					}
					borderProvinces.Add(province);
					isBorderProvince = true;
				}
			}
		}
		// Heavy calculation, don't do often.
		private void CalculateClosestProvinces(WarEnemy enemy){
			enemy.ClearProvinceData();
			Dictionary<Land, int> distances = new();
			AddLandDistances(enemy.Country.Provinces, distances, enemy);
			// Add the provinces of the own country so that armies will unoccupy provinces besieged by enemies. 
			AddLandDistances(Country.Provinces, distances, enemy);
			
			enemy.ClosestProvinces.Sort((left, right) => distances[left]-distances[right]);
			enemy.GroupOverseasProvinces();
		}
		private void AddLandDistances(IEnumerable<Land> lands, Dictionary<Land, int> distances, WarEnemy enemy){
			const float speedIsIrrelevantForSorting = 1;
			if (Country.ProvinceCount == 0){
				return;
			}
			LandLocation capital = Country.Capital.ArmyLocation;
			bool hasOverseasProvinces = false;
			foreach (Land land in lands){
				enemy.ClosestProvinces.Add(land);
				List<ProvinceLink> path = Regiment.GetPath(capital, land.ArmyLocation, link => Regiment.LinkEvaluator(link, false, Country));
				if (path == null){
					distances[land] = int.MaxValue;
					if (land.Province.IsCoast){
						if (!hasOverseasProvinces){
							overseasWarEnemies.Add(enemy);
						}
						enemy.AddOverseasProvince(land);
						hasOverseasProvinces = true;
					}
					continue;
				}
				distances[land] = path.Sum(link => Regiment.GetTravelDays(link, speedIsIrrelevantForSorting));
			}
		}
		private void RegroupUnits(){
			RegroupRegiments();
			RegroupShips();
		}
		private void RegroupRegiments(){
			for (int i = 0; i < Country.Regiments.Count; i++){
				Regiment regiment = Country.Regiments[i];
				if (!regiment.IsBuilt){
					continue;
				}
				RegimentBrain brain = GetBrain(regiment);
				if (warEnemies.Count == 0){
					brain.Tree.Blackboard.RemoveValue(brain.EnemyCountry);
					if (borderProvinces.Count > 0){
						brain.Tree.Blackboard.SetValue(brain.Target, borderProvinces[i%borderProvinces.Count].Province);
					}
				} else if (regiment.Location is not TransportDeck || IsNoLongerAtWar(brain)){
					WarEnemy enemy = warEnemies[i*warEnemies.Count/Country.Regiments.Count];
					brain.Tree.Blackboard.SetValue(brain.EnemyCountry, enemy);
				}
			}
		}
		private void RegroupShips(){
			for (int i = 0; i < Country.Ships.Count; i++){
				Ship ship = Country.Ships[i];
				if (!ship.IsBuilt){
					continue;
				}
				ShipBrain brain = GetBrain(ship);
				if (overseasWarEnemies.Count == 0){
					brain.Tree.Blackboard.RemoveValue(brain.EnemyCountry);
					if (harbors.Count > 0){
						brain.Tree.Blackboard.SetValue(brain.Target, harbors[i%harbors.Count]);
					}
					// Transports with armies are locked to keeping that enemy when the war still rages.
				} else if (ship is not Transport transport || transport.Deck.Units.Count == 0 || IsNoLongerAtWar(brain)){
					WarEnemy enemy = overseasWarEnemies[i*overseasWarEnemies.Count/Country.Ships.Count];
					brain.Tree.Blackboard.SetValue(brain.EnemyCountry, enemy);
				}
			}
		}
		private bool IsNoLongerAtWar<TUnit>(MilitaryUnitBrain<TUnit> brain) where TUnit : Unit<TUnit>{
			WarEnemy enemy = brain.Tree.Blackboard.GetValue<WarEnemy>(brain.EnemyCountry, null);
			return enemy == null || !enemy.Country.GetDiplomaticStatus(Country).IsAtWar;
		}
		
		public int EvaluatePeaceOffer(PeaceTreaty treaty){
			return peaceAcceptance.EvaluatePeaceOffer(treaty);
		}

		internal RegimentBrain GetBrain(Regiment regiment){
			return regimentBrains[regiment];
		}
		internal ShipBrain GetBrain(Ship ship){
			return shipBrains[ship];
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

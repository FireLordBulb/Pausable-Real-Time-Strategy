using System;
using System.Linq;
using System.Threading.Tasks;
using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player {
	public class DebugConsole : MonoBehaviour {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		// ReSharper disable thrice InconsistentNaming
		private const int YYYY_MM_DD = 3;
		private const int MM_DD = 2;
		private const int YYYY = 1;
		
		[SerializeField] private TextMeshProUGUI consoleText;
		[SerializeField] private TMP_InputField inputField;
		[SerializeField] private bool doUseAutoCommands;
		[SerializeField] private string[] autoCommands;

		private static GameObject InputFieldGameObject;
		public static bool IsKeyboardBusy(){
			return InputFieldGameObject == EventSystem.current.currentSelectedGameObject;
		}
		
		private void Awake(){
			inputField.onSubmit.AddListener(message => {
				inputField.ActivateInputField();
				inputField.text = "";
				message = message.Trim();
				if (message.Length != 0){
					consoleText.text += $"\n{message}";
					RunCommand(message);
				}
			});
			inputField.onValueChanged.AddListener(message => inputField.text = message.TrimEnd('\n').TrimEnd('\v'));
			InputFieldGameObject = inputField.gameObject;
		}
		private void Start(){
			gameObject.SetActive(false);
			if (!doUseAutoCommands){
				return;
			}
			foreach (string command in autoCommands){
				RunCommand(command);
			}
		}
		
		public void Enable(){
			gameObject.SetActive(true);
			inputField.ActivateInputField();
		}
		public void Disable(){
			gameObject.SetActive(false);
			EventSystem.current.SetSelectedGameObject(null);
		}
		
		private void RunCommand(string command){
			string[] words = command.Split(" ");
			switch(words[0].ToLower()){
				case "country_switching":
					UIStack.Instance.CanSwitchCountry = !UIStack.Instance.CanSwitchCountry;
					AddConsoleResponse($"Free country switching is now {(UIStack.Instance.CanSwitchCountry ? "Enabled" : "Disabled")}.");
					return;
				case "observe":
					UIStack.Instance.PlayAs(null);
					AddConsoleResponse("Activated observer mode. Deactivate by selecting a country.");
					return;
				case "play_as":
					if (words.Length == 1){
						AddConsoleResponse("Incomplete command: 'play_as' requires country name as argument.");
						return;
					}
					Country country = Country.Get(words[1]);
					if (country != null){
						UIStack.Instance.PlayAs(country);
						AddConsoleResponse($"Switching to {country.name}.");
					} else {
						AddConsoleResponse($"Command 'play_as' failed. No country has the name '{words[1]}'.");
					}
					return;
				case "own":
					if (UIStack.Instance.PlayerCountry == null){
						AddConsoleResponse("Command 'own' failed. Must be playing as a country to own a province.");
						return;
					}
					if (UIStack.Instance.SelectedProvince == null){
						AddConsoleResponse("Command 'own' failed. You must select a specific province to own.");
						return;
					}
					UIStack.Instance.SelectedProvince.Owner = UIStack.Instance.PlayerCountry;
					UIStack.Instance.RefreshSelected();
					AddConsoleResponse($"{UIStack.Instance.PlayerCountry.Name} now own the province {UIStack.Instance.SelectedProvince}.");
					return;
				case "rich":
					RunCommand("gold 999900");
					RunCommand("manpower 999999");
					RunCommand("sailors 999999");
					return;
				case "gold": {
					AddResource(words, "a float",
						s => (float.TryParse(s, out float value), value),
						gold => UIStack.Instance.PlayerCountry.GainResources(gold, 0, 0)
					);
					return;
				}
				case "manpower": {
					AddResource(words, "an int",
						s => (int.TryParse(s, out int value), value),
						manpower => UIStack.Instance.PlayerCountry.GainResources(0, manpower, 0)
					);
					return;
				}
				case "sailors": {
					AddResource(words, "an int",
						s => (int.TryParse(s, out int value), value),
						sailors => UIStack.Instance.PlayerCountry.GainResources(0, 0, sailors)
					);
					return;
				}
				case "skip_days":
				case "skip": {
					if (words.Length == 1){
						AddConsoleResponse("Incomplete command: 'skip_days' requires number of days as argument.");
						return;
					}
					if (int.TryParse(words[1], out int days)){
						Date date = Calendar.Instance.CurrentDate;
						date.day += days;
						date.Validate();
						SetDate(date);
					} else {
						AddConsoleResponse($"Command 'skip_days' failed. Couldn't parse {words[1]} to int.");
					}
					return;
				}
				case "date": {
					if (words.Length == 1){
						AddConsoleResponse("Incomplete command: 'date' requires a date formatted as YYYY-MM-DD or YYYY or MM-DD as argument.");
						return;
					}
					string[] numberStrings = words[1].TrimStart('-').Split('-');
					if (numberStrings.Length <= YYYY_MM_DD){
						int sign = words[1][0] == '-' ? -1 : +1;
						bool couldParse = ParseAndSetDate(numberStrings, sign);
						if (couldParse){
							return;
						}
					}
					AddConsoleResponse($"Command 'date' failed. Date must be formatted as YYYY-MM-DD or YYYY or MM-DD.");
					return;
				}
				case "regiment":
				case "reg": {
					CreateMilitaryUnit("regiment", words, province => {
						Simulation.Military.RegimentType type = UIStack.Instance.PlayerCountry.RegimentTypes.First();
						return (type.name, UIStack.Instance.PlayerCountry.TryStartRecuitingRegiment(type, province));
					}, "recruit a regiment", "recruiting");
					return;
				}
				case "ship": {
					CreateMilitaryUnit("ship", words, province => {
						Simulation.Military.ShipType type = UIStack.Instance.PlayerCountry.ShipTypes.First();
						return (type.name, UIStack.Instance.PlayerCountry.TryStartConstructingFleet(type, UILayer.GetHarbor(province)));
					}, "construct a ship", "constructing");
					return;
				}
			}
			AddConsoleResponse($"Invalid command: {words[0]}");
		}
		private void AddResource<T>(string[] words, string typeName, Func<string, (bool, T)> parser, Action<T> addfunction){
			if (UIStack.Instance.PlayerCountry == null){
				AddConsoleResponse($"Command '{words[0]}' failed. Must be playing as a country to add resources to it.");
				return;
			}
			if (words.Length == 1){
				AddConsoleResponse($"Incomplete command: '{words[0]}' requires {words[0]} amount as argument.");
				return;
			}
			(bool couldParse, T value) = parser(words[1]);
			if (couldParse){
				addfunction(value);
				UIStack.Instance.RefreshSelected();
				AddConsoleResponse($"Added {words[1]} {words[0]} to {UIStack.Instance.PlayerCountry.Name}.");
			} else {
				AddConsoleResponse($"Command '{words[0]}' failed. Couldn't parse {words[1]} to {typeName}.");
			}
		}
		private bool ParseAndSetDate(string[] numberStrings, int sign){
			int[] ints = new int[numberStrings.Length];
			for (int i = 0; i < numberStrings.Length; i++){
				if (!int.TryParse(numberStrings[i], out ints[i])){
					return false;
				}
			}
			ints[0] *= sign;
			switch(numberStrings.Length){
				case YYYY_MM_DD:
					SetDate(new Date(ints[0], ints[1]-1, ints[2]));
					break;
				case MM_DD:
					SetDate(new Date(Calendar.Instance.CurrentDate.year, ints[0]-1, ints[1]));
					break;
				case YYYY:
					SetDate(new Date(ints[0], Calendar.Instance.CurrentDate.month, Calendar.Instance.CurrentDate.day));
					break;
			}
			return true;
		}
		private async void SetDate(Date date){
			if (date < Calendar.Instance.CurrentDate){
				AddConsoleResponse($"Changing the date to the past will not modify the simulation.");
				Calendar.Instance.SetDate(date);
				UIStack.Instance.CalendarPanel.UpdateDate();
				AddConsoleResponse($"Set the date to {date}.");
				return;
			}
			if (date == Calendar.Instance.CurrentDate){
				AddConsoleResponse($"Desired date is already current date.");
				return;
			}
			AddConsoleResponse($"Will change the date to {date}. This may cause a lag spike.");
			// Wait a bit to let the lag spike warning render to the screen.
			await Task.Delay(100);
			UIStack.Instance.CalendarPanel.DisableUpdate();
			do {
				Calendar.Instance.ToNextDay();
			} while (Calendar.Instance.CurrentDate < date);
			UIStack.Instance.CalendarPanel.EnableUpdate();
			UIStack.Instance.CalendarPanel.UpdateDate();
			UIStack.Instance.RefreshSelected();
			AddConsoleResponse($"Successfully changed the date to {date}.");
		}
		private void CreateMilitaryUnit(string command, string[] words, Func<Province, (string, bool)> tryStartCreating, string setenceSegment, string creatingVerb){
			if (UIStack.Instance.PlayerCountry == null){
				AddConsoleResponse($"Command '{command}' failed. Must be playing as a country to {setenceSegment}.");
				return;
			}
			Province province;
			if (words.Length == 1){
				province = UIStack.Instance.PlayerCountry.Capital;
			} else {
				if (int.TryParse(words[1], out int index)){
					province = UIStack.Instance.PlayerCountry.Provinces.ElementAtOrDefault(index);
					if (province == null){
						AddConsoleResponse($"Command '{command}' failed. {words[1]} isn't smaller than {UIStack.Instance.PlayerCountry.Name}'s province count.");
						return;
					}
				} else {
					AddConsoleResponse($"Command '{command}' failed: Couldn't parse {words[1]} to an int province index.");
					return;
				}
			}
			(string typeName, bool didStartBuilding) = tryStartCreating(province);
			UIStack.Instance.RefreshSelected();
			AddConsoleResponse(didStartBuilding ? $"Started {creatingVerb} {typeName}." : $"Failed to start {creatingVerb} {typeName}.");
		}
		private void AddConsoleResponse(string response){
			consoleText.text += $"\n> {response}";
		}
#endif
	}
}

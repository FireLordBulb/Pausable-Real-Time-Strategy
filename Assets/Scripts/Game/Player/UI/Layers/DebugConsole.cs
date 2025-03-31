using System;
using System.Threading.Tasks;
using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player {
	public class DebugConsole : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI consoleText;
		[SerializeField] private TMP_InputField inputField;
		private void Awake(){
			gameObject.SetActive(false);
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
				case "skip_days": {
					if (words.Length == 1){
						AddConsoleResponse("Incomplete command: 'skip_days' requires number of days as argument.");
						return;
					}
					if (int.TryParse(words[1], out int value)){
						SetDate(new Date(Calendar.Instance.CurrentDate.year, Calendar.Instance.CurrentDate.month,
							Calendar.Instance.CurrentDate.day+value));
					} else {
						AddConsoleResponse($"Command 'skip_days' failed. Couldn't parse {words[1]} to int.");
					}
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
			AddConsoleResponse($"Successfully changed the date to {date}.");
		}
		private void AddConsoleResponse(string response){
			consoleText.text += $"\n> {response}";
		}
	}
}

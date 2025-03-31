using System;
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
				case "gold":
					AddResource(words,
						s => (float.TryParse(s, out float value), value),
						value => UIStack.Instance.PlayerCountry.GainResources(value, 0, 0),
					"a float");
					return;
				case "manpower":
					AddResource(words,
						s => (int.TryParse(s, out int value), value),
						value => UIStack.Instance.PlayerCountry.GainResources(0, value, 0),
					"an int");
					return;
				case "sailors":
					AddResource(words,
						s => (int.TryParse(s, out int value), value),
						value => UIStack.Instance.PlayerCountry.GainResources(0, 0, value),
					"an int");
					return;
			}
			AddConsoleResponse($"Invalid command: {words[0]}");
		}
		private void AddResource<T>(string[] words, Func<string, (bool, T)> parser, Action<T> addfunction, string typeName){
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
		private void AddConsoleResponse(string response){
			consoleText.text += $"\n> {response}";
		}
	}
}

using System;
using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class CountryPanel : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI countryName;
		[SerializeField] private Image flag;
		
		private Country linkedCountry;
		
		public void SetCountry(Country country, UIStack ui, Action action = null){
			if (linkedCountry == country){
				return;
			}
			linkedCountry = country;
			countryName.text = linkedCountry.Name;
			ui.Links.Add(countryName, linkedCountry, action);
			flag.material = new Material(flag.material){
				color = country.MapColor
			};
			ui.Links.Add(flag, linkedCountry, action);
		}
	}
}
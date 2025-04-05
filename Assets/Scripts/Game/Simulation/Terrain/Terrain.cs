using UnityEngine;

namespace Simulation {
	[CreateAssetMenu(fileName = "Terrain", menuName = "ScriptableObjects/Terrain")]
	public class Terrain : ScriptableObject {
		[SerializeField] private new string name;
		[SerializeField] private Material material;
		[SerializeField] private float developmentModifier;
		[SerializeField] private float moveSpeedModifier;
		[SerializeField] private float defenderAdvantage;
		[SerializeField] private int combatWidth;
		public string Name => name;
		public Material Material => material;
		public float DevelopmentModifier => developmentModifier;
		public float MoveSpeedModifier => moveSpeedModifier;
		public float DefenderAdvantage => defenderAdvantage;
		public int CombatWidth => combatWidth;
	}
}

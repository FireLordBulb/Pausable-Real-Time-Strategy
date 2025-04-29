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
		public float DevelopmentMultiplier => 1+developmentModifier;
		public float MoveSpeedMultiplier => 1+moveSpeedModifier;
		public float DefenderAdvantage => defenderAdvantage;
		public int CombatWidth => combatWidth;
	}
}

using UnityEngine;

[CreateAssetMenu(fileName = "Terrain", menuName = "ScriptableObjects/Terrain")]
public class Terrain : ScriptableObject {
	[SerializeField] private new string name;
	[SerializeField] private Material material;
	[SerializeField] private float developmentModifier;
	[SerializeField] private float defenderAdvantage;
	public string Name => name;
	public Material Material => material;
	public float DevelopmentModifier => developmentModifier;
	public float DefenderAdvantage => defenderAdvantage;
}
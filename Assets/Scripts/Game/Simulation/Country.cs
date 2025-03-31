using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMeshes;
using UnityEngine;

namespace Simulation {
	public class Country : MonoBehaviour {
		private static readonly Dictionary<string, Country> Countries = new();
		public static Country Get(string key){
			Countries.TryGetValue(key, out Country country);
			return country;
		}
#if UNITY_EDITOR
		public static void ClearCountryDictionary(){
			Countries.Clear();
		}
#endif
		
		private readonly HashSet<Province> provinces = new();
		
		[SerializeField] private MeshFilter borderMeshFilter;
		[SerializeField] private MeshRenderer borderMeshRenderer;
		[SerializeField] private float borderHalfWidth;
		[SerializeField] private float borderBrightnessFactor;
		
		private bool wasBorderChanged;
		
		public Color MapColor {get; private set;}
		public bool IsDirty {get; private set;}
		public int ProvinceCount {get; private set;}
		public float Gold {get; private set;}
		public int Manpower {get; private set;}
		public int Sailors {get; private set;}
		
		public IEnumerable<Province> Provinces => provinces;
		// TODO: Assign a specific province as capital from country data.
		public Province Capital => provinces.First();
		public string Name => gameObject.name;
		
		public void Init(CountryData data, MapGraph map){
			gameObject.name = data.Name;
			MapColor = data.MapColor;
			foreach (Color32 province in data.Provinces){
				map[province].Owner = this;
			}
			Color borderColor = MapColor*borderBrightnessFactor;
			borderColor.a = 1;
			borderMeshRenderer.material.color = borderColor;
			RegenerateBorder();
			Countries.Add(Name, this);
		}
		private void RegenerateBorder(){
			DestroyImmediate(borderMeshFilter.sharedMesh);
			MeshData borderMeshData = new($"{gameObject.name}BorderMesh");
			List<Vector2> borderVertices = new();
			
			// TODO: Only add the sections of vertices between outer border tri-points.
			Province province = provinces.First();
			int startSegment = 0;
			ProvinceLink link = null;
			AddAllButOneSegments();
			startSegment = (link.Target[province.ColorKey].SegmentIndex+1)%link.Target.outlineSegments.Count;
			province = link.Target;
			AddAllButOneSegments();
			
			// Completes incomplete loops
			/*for (int i = borderVertices.Count-2; i > 0; i--){
				borderVertices.Add(borderVertices[i]+Vector2.up*5);
			}*/
			
			PolygonOutline.GenerateMeshData(borderMeshData, borderVertices, borderHalfWidth, true);
			borderMeshFilter.mesh = borderMeshData.ToMesh();
			wasBorderChanged = false;

			void AddAllButOneSegments(){
				for (int i = 0; i < province.outlineSegments.Count; i++){
					int index = (i+startSegment+province.outlineSegments.Count)%province.outlineSegments.Count;
					int startIndex, endIndex;
					(startIndex, endIndex, link) = province.outlineSegments[index];
					if (i >= province.outlineSegments.Count-1){
						break;
					}
					for (int j = startIndex; j != endIndex; j = (j+1)%province.Vertices.Count){
						borderVertices.Add(province.MapPosition+province.Vertices[j]);
					}
				}
			}
		}
		
		private void Update(){
			if (wasBorderChanged){
				RegenerateBorder();
			}
		}
		
		public void GainResources(float gold, int manpower, int sailors){
			Gold += gold;
			Manpower += manpower;
			Sailors += sailors;
			IsDirty = true;
		}
		public void MarkClean(){
			IsDirty = false;
		}
		
		public bool GainProvince(Province province){
			return ChangeProvinceCount(provinces.Add(province), +1);
		}
		public bool LoseProvince(Province province){
			return ChangeProvinceCount(provinces.Remove(province), -1);

		}
		private bool ChangeProvinceCount(bool wasChanged, int change){
			if (!wasChanged){
				return false;
			}
			ProvinceCount += change;
			wasBorderChanged = true;
			IsDirty = true;
			return true;
		}
		
		public void OnSelect(){
			foreach (Province province in Provinces){
				province.OnSelect();
			}
		}
		public void OnDeselect(){
			foreach (Province province in Provinces){
				province.OnDeselect();
			}
		}
	}

	[Serializable]
	public class CountryData {
		[SerializeField] private string name;
		[SerializeField] private Color mapColor;
		[SerializeField] private Color32[] provinces;

		public string Name => name;
		public Color MapColor => mapColor;
		public IEnumerable<Color32> Provinces => provinces;
	}
}

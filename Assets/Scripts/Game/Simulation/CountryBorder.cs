using System.Collections.Generic;
using System.Linq;
using ProceduralMeshes;
using UnityEngine;

namespace Simulation {
	public static class CountryBorder {
		public static void Generate(MeshData meshData, Country country, HashSet<Province> unsearchedProvinces, float borderHalfWidth){
			HashSet<List<Province>> borderProvinceLoops = new();
			while (unsearchedProvinces.Count > 0){
				SearchForProvinceLoop(country, unsearchedProvinces, borderProvinceLoops);
			}
			foreach (List<Province> provinceLoop in borderProvinceLoops){
				List<Vector2> vertexLoop = ConvertToVertexLoop(provinceLoop);
				PolygonOutline.GenerateMeshData(meshData, vertexLoop, borderHalfWidth, true);
			}
		}
		
		// Finds what loop(s) of provinces constitute the outer border of the country.
		private static void SearchForProvinceLoop(Country country, HashSet<Province> unsearchedProvinces, HashSet<List<Province>> borderProvinceLoops){
			Province borderProvince = unsearchedProvinces.First();
			unsearchedProvinces.Remove(borderProvince);
			for (int segmentIndex = 0; segmentIndex < borderProvince.OutlineSegments.Count; segmentIndex++){
				(int _, int _, ProvinceLink link) = borderProvince.OutlineSegments[segmentIndex];
				if (link != null && link.Target.IsLand && link.Target.Land.Owner == country){
					continue;
				}
				AddLoopStartingAt(borderProvince, segmentIndex, country, unsearchedProvinces, borderProvinceLoops);
				break;
			}
		}
		private static void AddLoopStartingAt(Province borderProvince, int segmentIndex, Country country, HashSet<Province> unsearchedProvinces, HashSet<List<Province>> borderProvinceLoops){
			List<Province> provinceLoop = new(){borderProvince};
			ProvinceLink firstLink = null;
			while (true){
				segmentIndex = (segmentIndex+1)%borderProvince.OutlineSegments.Count;
				(_, _, ProvinceLink link) = borderProvince.OutlineSegments[segmentIndex];
				if (link == firstLink){
					if (provinceLoop.Count > 1){
						// Remove the last province because it's a duplicate of loopStartProvince.
						provinceLoop.RemoveAt(provinceLoop.Count-1);
					}
					break;
				}
				firstLink ??= link;
				if (link == null || link.Target.IsSea || link.Target.Land.Owner != country){
					continue;
				}
				segmentIndex = link.Target[borderProvince.ColorKey].SegmentIndex;
				borderProvince = link.Target;
				unsearchedProvinces.Remove(borderProvince);
				provinceLoop.Add(borderProvince);
			}
			borderProvinceLoops.Add(provinceLoop);
		}

		// Converts loops of provinces to loops of vertex positions.
		private static List<Vector2> ConvertToVertexLoop(List<Province> provinceLoop){
			List<Vector2> vertexLoop;
			if (provinceLoop.Count == 1){
				vertexLoop = new List<Vector2>(provinceLoop[0].Vertices);
				for (int i = 0; i < vertexLoop.Count; i++){
					vertexLoop[i] += provinceLoop[0].MapPosition;
				}
				return vertexLoop;
			}
			vertexLoop = new List<Vector2>();
			Province previousProvince = provinceLoop[^1];
			Province currentProvince = provinceLoop[0];
			for (int provinceIndex = 0; provinceIndex < provinceLoop.Count; provinceIndex++){
				Province nextProvince = provinceLoop[(provinceIndex+1)%provinceLoop.Count];
				int segmentIndex = (currentProvince[previousProvince.ColorKey].SegmentIndex+1)%currentProvince.OutlineSegments.Count;
				AddVerticesFromProvince(currentProvince, segmentIndex, nextProvince, vertexLoop);
				previousProvince = currentProvince;
				currentProvince = nextProvince;
			}
			return vertexLoop;
		}
		private static void AddVerticesFromProvince(Province currentProvince, int segmentIndex, Province nextProvince, List<Vector2> vertexLoop){
			while (true){
				(int vertexIndex, int endIndex, ProvinceLink link) = currentProvince.OutlineSegments[segmentIndex];
				if (link != null && link.Target == nextProvince){
					break;
				}
				for (; vertexIndex != endIndex; vertexIndex = (vertexIndex+1)%currentProvince.Vertices.Count){
					vertexLoop.Add(currentProvince.MapPosition+currentProvince.Vertices[vertexIndex]);
				}
				segmentIndex = (segmentIndex+1)%currentProvince.OutlineSegments.Count;
			}
		}
	}
}

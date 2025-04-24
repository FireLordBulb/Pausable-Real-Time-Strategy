using System.Collections.Generic;
using System.Linq;
using ProceduralMeshes;
using UnityEngine;

namespace Simulation {
	public static class CountryBorder {
		public static void Generate(MeshData meshData, Country country, HashSet<Province> unsearchedProvinces, float borderHalfWidth){
			HashSet<Province> unconnectedProvinces = new();
			HashSet<List<ProvinceLink>> provinceLoops = new();
			while (unsearchedProvinces.Count > 0){
				SearchForProvinceLoop(country, unsearchedProvinces, provinceLoops, unconnectedProvinces);
			}
			foreach (Province province in unconnectedProvinces){
				PolygonOutline.GenerateMeshData(meshData, GetVertexLoop(province), borderHalfWidth, true);
			}
			foreach (List<ProvinceLink> provinceLoop in provinceLoops){
				PolygonOutline.GenerateMeshData(meshData, GetVertexLoop(provinceLoop), borderHalfWidth, true);
			}
		}
		
		// Finds what loop(s) of provinceLinks constitute the outer border of the country.
		private static void SearchForProvinceLoop(Country country, HashSet<Province> unsearchedProvinces, HashSet<List<ProvinceLink>> provinceLoops, HashSet<Province> unconnectedProvinces){
			Province borderProvince = unsearchedProvinces.First();
			unsearchedProvinces.Remove(borderProvince);
			for (int segmentIndex = 0; segmentIndex < borderProvince.OutlineSegments.Count; segmentIndex++){
				(int _, int _, ProvinceLink link) = borderProvince.OutlineSegments[segmentIndex];
				if (link != null && link.Target.IsLand && link.Target.Land.Owner == country){
					continue;
				}
				AddLoopStartingAt(link, segmentIndex, country, unsearchedProvinces, provinceLoops, unconnectedProvinces);
				break;
			}
		}
		private static void AddLoopStartingAt(ProvinceLink firstLink, int segmentIndex, Country country, HashSet<Province> unsearchedProvinces, HashSet<List<ProvinceLink>> provinceLoops, HashSet<Province> unconnectedProvinces){
			List<ProvinceLink> provinceLoop = new();
			Province borderProvince = firstLink.Source;
			while (true){
				segmentIndex = (segmentIndex+1)%borderProvince.OutlineSegments.Count;
				(_, _, ProvinceLink link) = borderProvince.OutlineSegments[segmentIndex];
				if (link == firstLink){
					if (provinceLoop.Count == 0){
						unconnectedProvinces.Add(borderProvince);
					} else {
						provinceLoops.Add(provinceLoop);
					}
					break;
				}
				if (link.Target == null || link.Target.IsSea || link.Target.Land.Owner != country){
					continue;
				}
				segmentIndex = link.Reverse.SegmentIndex;
				borderProvince = link.Target;
				unsearchedProvinces.Remove(borderProvince);
				provinceLoop.Add(link);
			}
		}

		private static List<Vector2> GetVertexLoop(Province province){
			List<Vector2> vertexLoop = new(province.Vertices);
			for (int i = 0; i < vertexLoop.Count; i++){
				vertexLoop[i] += province.MapPosition;
			}
			return vertexLoop;
		}

		private static List<Vector2> GetVertexLoop(List<ProvinceLink> provinceLoop){
			List<Vector2> vertexLoop = new();
			ProvinceLink linkFromPrevious = provinceLoop[^1];
			foreach (ProvinceLink linkToNext in provinceLoop){
				AddVerticesFromProvince(linkFromPrevious, linkToNext, vertexLoop);
				linkFromPrevious = linkToNext;
			}
			return vertexLoop;
		}
		private static void AddVerticesFromProvince(ProvinceLink linkFromPrevious, ProvinceLink linkToNext, List<Vector2> vertexLoop){
			Province currentProvince = linkFromPrevious.Target;
			int segmentIndex = linkFromPrevious.Reverse.SegmentIndex;
			while (true){
				segmentIndex = (segmentIndex+1)%currentProvince.OutlineSegments.Count;
				(int vertexIndex, int endIndex, ProvinceLink link) = currentProvince.OutlineSegments[segmentIndex];
				if (link == linkToNext){
					break;
				}
				for (; vertexIndex != endIndex; vertexIndex = (vertexIndex+1)%currentProvince.Vertices.Count){
					vertexLoop.Add(currentProvince.MapPosition+currentProvince.Vertices[vertexIndex]);
				}
			}
		}
	}
}

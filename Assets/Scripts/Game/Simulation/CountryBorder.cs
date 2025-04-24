using System.Collections.Generic;
using System.Linq;
using ProceduralMeshes;
using UnityEngine;

namespace Simulation {
	public static class CountryBorder {
		public static void Generate(MeshData meshData, Country country, HashSet<Province> unsearchedProvinces, float borderHalfWidth){
			HashSet<List<ProvinceLink>> provinceLoops = new();
			while (unsearchedProvinces.Count > 0){
				SearchForProvinceLoop(country, unsearchedProvinces, provinceLoops);
			}
			foreach (List<ProvinceLink> provinceLoop in provinceLoops){
				List<Vector2> vertexLoop = ConvertToVertexLoop(provinceLoop);
				PolygonOutline.GenerateMeshData(meshData, vertexLoop, borderHalfWidth, true);
			}
		}
		
		// Finds what loop(s) of provinceLinks constitute the outer border of the country.
		private static void SearchForProvinceLoop(Country country, HashSet<Province> unsearchedProvinces, HashSet<List<ProvinceLink>> provinceLoops){
			Province borderProvince = unsearchedProvinces.First();
			unsearchedProvinces.Remove(borderProvince);
			for (int segmentIndex = 0; segmentIndex < borderProvince.OutlineSegments.Count; segmentIndex++){
				(int _, int _, ProvinceLink link) = borderProvince.OutlineSegments[segmentIndex];
				if (link != null && link.Target.IsLand && link.Target.Land.Owner == country){
					continue;
				}
				AddLoopStartingAt(link, segmentIndex, country, unsearchedProvinces, provinceLoops);
				break;
			}
		}
		private static void AddLoopStartingAt(ProvinceLink firstLink, int segmentIndex, Country country, HashSet<Province> unsearchedProvinces, HashSet<List<ProvinceLink>> provinceLoops){
			List<ProvinceLink> provinceLoop = new();
			Province borderProvince = firstLink.Source;
			while (true){
				segmentIndex = (segmentIndex+1)%borderProvince.OutlineSegments.Count;
				(_, _, ProvinceLink link) = borderProvince.OutlineSegments[segmentIndex];
				if (link == firstLink){
					if (provinceLoop.Count == 0){
						provinceLoop.Add(firstLink);
					}
					break;
				}
				if (link == null || link.Target.IsSea || link.Target.Land.Owner != country){
					continue;
				}
				segmentIndex = link.Reverse.SegmentIndex;
				borderProvince = link.Target;
				unsearchedProvinces.Remove(borderProvince);
				provinceLoop.Add(link);
			}
			provinceLoops.Add(provinceLoop);
		}

		// Converts loops of provinceLinks to loops of vertex positions.
		private static List<Vector2> ConvertToVertexLoop(List<ProvinceLink> provinceLoop){
			List<Vector2> vertexLoop;
			Debug.Log(provinceLoop.Count);
			if (provinceLoop.Count == 1){
				Province onlyProvince = provinceLoop[0].Source;
				vertexLoop = new List<Vector2>(onlyProvince.Vertices);
				for (int i = 0; i < vertexLoop.Count; i++){
					vertexLoop[i] += onlyProvince.MapPosition;
				}
				return vertexLoop;
			}
			vertexLoop = new List<Vector2>();
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

using Graphs;

public class ProvinceLink : ILink<Province, ProvinceLink> {
	public Province Source {get;}
	public Province Target {get;}
    
	public ProvinceLink(Province source, Province target){
		Source = source;
		Target = target;
	}
}
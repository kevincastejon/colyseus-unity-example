// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.40
// 

using Colyseus.Schema;

public class State : Schema {
	[Type(0, "map", typeof(MapSchema<PlayerData>))]
	public MapSchema<PlayerData> players = new MapSchema<PlayerData>();

	[Type(1, "ref", typeof(LightData))]
	public LightData lights = new LightData();

	[Type(2, "ref", typeof(DoorData))]
	public DoorData doors = new DoorData();

	[Type(3, "map", typeof(MapSchema<BallData>))]
	public MapSchema<BallData> balls = new MapSchema<BallData>();
}


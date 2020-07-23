// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.40
// 

using Colyseus.Schema;

public class EntityData : Schema {
	[Type(0, "ref", typeof(Vect3))]
	public Vect3 position = new Vect3();

	[Type(1, "ref", typeof(Quat))]
	public Quat rotation = new Quat();
}


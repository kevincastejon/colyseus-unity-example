// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.40
// 

using Colyseus.Schema;

public class PlayerData : EntityData {
	[Type(2, "string")]
	public string name = "";

	[Type(3, "uint8")]
	public uint color = 0;

	[Type(4, "uint8")]
	public uint emote = 0;
}


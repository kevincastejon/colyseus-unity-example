import {Schema, type, MapSchema} from '@colyseus/schema';

class Vect3 extends Schema {
  @type('float32')
  x: number = 0;

  @type('float32')
  y: number = 0;

  @type('float32')
  z: number = 0;
}
class Quat extends Schema {
  @type('float32')
  x: number = 0;

  @type('float32')
  y: number = 0;

  @type('float32')
  z: number = 0;

  @type('float32')
  w: number = 1;
}
class EntityData extends Schema {
  @type(Vect3)
  position = new Vect3();

  @type(Quat)
  rotation = new Quat();
}
class PlayerData extends EntityData {
  @type('string')
  name: string = null;

  @type('uint8')
  color: number = 0;

  @type('uint8')
  emote: number = 0;

  // Used only server-side for convenience, not synchronized with clients
  ownedBall : string = '';
}
class BallData extends EntityData {
  @type('string')
  owner: string = '';
}
class LightData extends Schema {
  @type('boolean')
  on: boolean = true;
}
class DoorData extends Schema {
  @type('boolean')
  open: boolean = false;
}
class State extends Schema {
  @type({map: PlayerData})
  players = new MapSchema<PlayerData>();

  @type(LightData)
  lights = new LightData();

  @type(DoorData)
  doors = new DoorData();

  @type({map: BallData})
  balls = new MapSchema<BallData>();
}
export {EntityData, PlayerData, BallData, LightData, DoorData, Vect3, Quat, State};

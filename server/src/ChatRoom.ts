import {Room, Client, generateId} from 'colyseus';
import {State, PlayerData, BallData, Vect3, Quat} from './Schemas';
import {World, Body, Vec3, Quaternion, Sphere, Plane, Material, ContactMaterial} from 'cannon-es';

// Some type definitions for TypeScript
class PlayerInput {
  position: Vect3;
  rotation: Quat;
}
class Area {
  minX: number;
  maxX: number;
  minZ: number;
  maxZ: number;
  minRY: number;
  maxRY: number;

  constructor(minX: number, maxX: number, minZ: number, maxZ: number, minRY: number=-Math.PI, maxRY: number=Math.PI) {
    this.minX=minX;
    this.maxX=maxX;
    this.minZ=minZ;
    this.maxZ=maxZ;
    this.minRY=minRY;
    this.maxRY=maxRY;
  }
}
class SphereMap {
    [name: string]: Body
};

// Setup some locations for interactions and physic world
const ballTakeDistance: number = 2;
const lightSwitchArea: Area = new Area(-21.5, -18.5, 6, 8, (Math.PI/180)*35, (Math.PI/180)*145);
const doorArea: Area = new Area(-38, -32, 1, 6.5);
const ballSpawnerArea: Area = new Area(-42, -40, 3, 5, (Math.PI/180)*75, (Math.PI/180)*125);
const ballRoomArea: Area = new Area(-50.8, -35.8, -5, 13);

export class ChatRoom extends Room {
  world: World;
  spheres: SphereMap;
  groundMaterial: Material;
  northWallMaterial: Material;
  southWallMaterial: Material;
  eastWallMaterial: Material;
  westWallMaterial: Material;

  // Room created
  onCreate(options: any) {
    this.spheres = {};
    // Create a CannonJS 3d physic world for the "balls area"
    this.world = new World();
    this.world.gravity.set(0, -9.82, 0); // m/s²

    // Create a floor for the "balls area"
    this.createFloor();

    // Create walls for the "balls area"
    this.createWalls();

    // configuring room
    this.setMetadata({name: options.name});
    this.setPatchRate(1000 / 20);
    this.setSimulationInterval((dt) => this.update(dt));

    // Setting inital state
    const state: State = new State();
    this.setState(state);

    // Listening to client's messages
    this.onMessage('input', (client, input)=>this.onPlayerMove(client, input));
    this.onMessage('emote', (client, emote)=>this.onPlayerEmote(client, emote));
    this.onMessage('interact', (client)=>this.onPlayerInteract(client));
  }

  // Player joined the game
  onJoin(client: Client, options: any) {
    console.log('player', client.id, 'connected');
    const newPlayer: PlayerData = new PlayerData();
    newPlayer.color=options.color;
    newPlayer.name=options.username;
    this.state.players[client.sessionId] = newPlayer;
  }

  // Player leaved the game
  onLeave(client: Client) {
    console.log('player', client.id, 'leaved');
    delete this.state.players[client.sessionId];
  }

  // Room is closing
  onDispose() {
    console.log('last player left, room closing...');
  }

  update(delta: number) {
    // Update 3D world and synchronize balls state from it
    this.world.step(delta/1000);
    this.refreshBallsSpacialization();
  }

  createFloor() {
    this.groundMaterial = new Material();
    const groundBody:Body = new Body({
      mass: 0,
      material: this.groundMaterial,
    });
    const groundShape: Plane = new Plane();
    groundBody.addShape(groundShape);
    groundBody.quaternion.setFromAxisAngle(new Vec3(1, 0, 0), -Math.PI/2);
    this.world.addBody(groundBody);
  }

  createWalls() {
    // Create north wall
    this.northWallMaterial = new Material();
    const northWallBody: Body = new Body({
      mass: 0,
      material: this.northWallMaterial,
      position: new Vec3(0, 0, ballRoomArea.minZ),
    });
    const northWallShape: Plane = new Plane();
    northWallBody.addShape(northWallShape);
    this.world.addBody(northWallBody);
    // Create south wall
    this.southWallMaterial = new Material();
    const southWallBody: Body = new Body({
      mass: 0,
      material: this.southWallMaterial,
      position: new Vec3(0, 0, ballRoomArea.maxZ),
    });
    const southWallShape: Plane = new Plane();
    southWallBody.addShape(southWallShape);
    southWallBody.quaternion.setFromAxisAngle(new Vec3(0, 1, 0), Math.PI);
    this.world.addBody(southWallBody);
    // Create east wall
    this.eastWallMaterial = new Material();
    const eastWallBody: Body = new Body({
      mass: 0,
      material: this.eastWallMaterial,
      position: new Vec3(ballRoomArea.minX, 0, 0),
    });
    const eastWallShape: Plane = new Plane();
    eastWallBody.addShape(eastWallShape);
    eastWallBody.quaternion.setFromAxisAngle(new Vec3(0, 1, 0), -3*(Math.PI/2));
    this.world.addBody(eastWallBody);
    // Create west wall
    this.westWallMaterial = new Material();
    const westWallBody: Body = new Body({
      mass: 0,
      material: this.westWallMaterial,
      position: new Vec3(ballRoomArea.maxX, 0, 0),
    });
    const westWallShape: Plane = new Plane();
    westWallBody.addShape(westWallShape);
    westWallBody.quaternion.setFromAxisAngle(new Vec3(0, 1, 0), -Math.PI/2);
    this.world.addBody(westWallBody);
  }

  // Player sent its position and rotation (see why not authoritative movements)
  onPlayerMove(client:Client, playerInput: PlayerInput) {
    const player: PlayerData = this.state.players[client.sessionId];
    if (playerInput.position.x != player.position.x ||
    playerInput.position.y != player.position.y ||
    playerInput.position.z != player.position.z
    ) {
      const pos: Vect3 = new Vect3();
      pos.x = playerInput.position.x;
      pos.y = playerInput.position.y;
      pos.z = playerInput.position.z;
      player.position = pos;
    }
    if (playerInput.rotation.x != player.rotation.x ||
    playerInput.rotation.y != player.rotation.y ||
    playerInput.rotation.z != player.rotation.z ||
    playerInput.rotation.w != player.rotation.w
    ) {
      const rot: Quat = new Quat();
      rot.x = playerInput.rotation.x;
      rot.y = playerInput.rotation.y;
      rot.z = playerInput.rotation.z;
      rot.w = playerInput.rotation.w;
      player.rotation = rot;
    }
    if (player.ownedBall && !this.isPlayerInArea(player, ballRoomArea)) {
      delete this.state.balls[player.ownedBall];
      delete this.spheres[player.ownedBall];
      player.ownedBall='';
    }
  }

  // Player change its emote
  onPlayerEmote(client: Client, emote: number) {
    const player: PlayerData = this.state.players[client.sessionId];
    player.emote = emote;
  }
  // Player pressed "interaction" input ("E" for keyboards "A" for gamepads)
  onPlayerInteract(client: Client) {
    let validation: boolean;

    //  Validate and execute interaction if validated
    //  if not validated it skips to the next possible interaction

    // Spawning ball interaction
    validation = this.trySpawnBall(client);
    if (validation) return;
    // Catching ball interaction
    validation = this.tryCatchBall(client);
    if (validation) return;
    // Operating lights interaction
    validation = this.tryOperateLights(client);
    if (validation) return;
    // Operating doors interaction
    validation = this.tryOperateDoors(client);
    if (validation) return;
    // Throwing ball interaction
    validation = this.tryThrowBall(client);
    if (validation) return;
  }
  tryCatchBall(client: Client) {
    const player: PlayerData = this.state.players[client.sessionId];
    // If player already got a ball then skip
    if (player.ownedBall !== '') {
      return false;
    }
    const reachableClosestBallId: string = this.getReachableClosestBallId(player);
    // If no reachable ball then skip
    if (!reachableClosestBallId) {
      return false;
    }
    // Player is catching a ball
    this.world.removeBody(this.spheres[reachableClosestBallId]);
    this.state.balls[reachableClosestBallId].owner = client.sessionId;
    player.ownedBall = reachableClosestBallId;
    return true;
  }
  tryOperateLights(client: Client) {
    const player: PlayerData = this.state.players[client.sessionId];
    // If player location/rotation is wrong then skip
    if (!this.isPlayerInArea(player, lightSwitchArea)) {
      return false;
    }
    // Player is operating lightswitch
    this.state.lights.on = !this.state.lights.on;
    return true;
  }
  tryOperateDoors(client: Client) {
    const player: PlayerData = this.state.players[client.sessionId];
    // If player location/rotation is wrong then skip
    if (!this.isPlayerInArea(player, doorArea)) {
      return false;
    }
    // Player is operating the doors
    this.state.doors.open = !this.state.doors.open;
    return true;
  }
  trySpawnBall(client: Client) {
    const player: PlayerData = this.state.players[client.sessionId];
    // If player location/rotation is wrong then skip
    if (!this.isPlayerInArea(player, ballSpawnerArea)) {
      return false;
    }
    // Player is spawning a ball
    // Create a CannonJS physic sphere object
    const radius: number = 0.20;
    const sphereMaterial: Material = new Material();
    const newBallId: string = generateId();
    this.spheres[newBallId] = new Body({
      mass: 5,
      position: new Vec3(-42.5+Math.random(), 2.5+Math.random(), 4+Math.random()), // Locate the new ball into the "balls area"
      shape: new Sphere(radius),
      material: sphereMaterial,
    });
    this.world.addBody(this.spheres[newBallId]);
    // Create ContactMaterial for ground
    const groundContMat = new ContactMaterial(this.groundMaterial, sphereMaterial, {friction: 1, restitution: 0.7});
    this.world.addContactMaterial(groundContMat);
    // Create ContactMaterial for walls
    const nWallContMat = new ContactMaterial(this.northWallMaterial, sphereMaterial, {friction: 1, restitution: 0.7});
    this.world.addContactMaterial(nWallContMat);
    const sWallContMat = new ContactMaterial(this.southWallMaterial, sphereMaterial, {friction: 1, restitution: 0.7});
    this.world.addContactMaterial(sWallContMat);
    const eWallContMat = new ContactMaterial(this.eastWallMaterial, sphereMaterial, {friction: 1, restitution: 0.7});
    this.world.addContactMaterial(eWallContMat);
    const wWallContMat = new ContactMaterial(this.westWallMaterial, sphereMaterial, {friction: 1, restitution: 0.7});
    this.world.addContactMaterial(wWallContMat);
    // Create the BallData object to synchonize room state with 3D world
    const newBallData: BallData = new BallData();
    newBallData.position.x = this.spheres[newBallId].position.x;
    newBallData.position.y = this.spheres[newBallId].position.y;
    newBallData.position.z = this.spheres[newBallId].position.z;
    newBallData.rotation.x = this.spheres[newBallId].quaternion.x;
    newBallData.rotation.y = this.spheres[newBallId].quaternion.y;
    newBallData.rotation.z = this.spheres[newBallId].quaternion.z;
    newBallData.rotation.w = this.spheres[newBallId].quaternion.w;
    this.state.balls[newBallId] = newBallData;
    return true;
  }
  tryThrowBall(client: Client) {
    const player: PlayerData = this.state.players[client.sessionId];
    // If player has no ball in hand then skip
    if (player.ownedBall === '') {
      return false;
    }
    // Player is throwing a ball
    const ballData: BallData = this.state.balls[player.ownedBall];
    const sphere: Body = this.spheres[player.ownedBall];
    sphere.position=new Vec3(player.position.x, player.position.y, player.position.z);
    sphere.quaternion = new Quaternion(player.rotation.x, player.rotation.y, player.rotation.z, player.rotation.w);
    this.world.addBody(sphere);
    const forward: Vec3 = new Vec3(0, 0, 1);
    const dir: Vec3 = new Vec3(0, 0, 0);
    sphere.quaternion.vmult(forward, dir);
    dir.scale(10, dir);
    dir.y=5;
    sphere.velocity = dir;
    ballData.owner = '';
    player.ownedBall = '';
    return true;
  }

  // Return the player's reachable free closest ball id
  getReachableClosestBallId(player: PlayerData) {
    let closestSphereId: string;
    let minDistance: number = Number.POSITIVE_INFINITY;
    for (const id in this.spheres) {
      if (this.spheres.hasOwnProperty(id)) {
        const sphere: Body = this.spheres[id];
        const ball: BallData = this.state.balls[id];
        if (ball.owner === '') {
          const currentDistance: number = sphere.position.distanceTo(new Vec3(player.position.x, player.position.y, player.position.z));
          if (currentDistance<minDistance) {
            minDistance = currentDistance;
            closestSphereId = id;
          }
        }
      }
    }
    // If the closest ball is not reachable then return null
    if (minDistance <= ballTakeDistance) {
      return closestSphereId;
    } else {
      return null;
    }
  }

  // Synchronize the 3D physic spheres objects with the balls state
  refreshBallsSpacialization() {
    for (const id in this.spheres) {
      if (this.spheres.hasOwnProperty(id)) {
        const sphere: Body = this.spheres[id];
        const balldata: BallData = this.state.balls[id];
        if (balldata.position.x != sphere.position.x ||
        balldata.position.y != sphere.position.y ||
        balldata.position.z != sphere.position.z
        ) {
          const ballPos: Vect3 = new Vect3();
          ballPos.x = sphere.position.x;
          ballPos.y = sphere.position.y;
          ballPos.z = sphere.position.z;
          balldata.position=ballPos;
        }
        if (balldata.rotation.x != sphere.quaternion.x ||
        balldata.rotation.y != sphere.quaternion.y ||
        balldata.rotation.z != sphere.quaternion.z ||
        balldata.rotation.w != sphere.quaternion.w
        ) {
          const ballRot: Quat = new Quat();
          ballRot.x = sphere.quaternion.x;
          ballRot.y = sphere.quaternion.y;
          ballRot.z = sphere.quaternion.z;
          ballRot.w = sphere.quaternion.w;
          balldata.rotation=ballRot;
        }
      }
    }
  }

  // Check if a player's location and rotation fits the "area" requirements
  isPlayerInArea(player:PlayerData, area: Area) {
    const quat: Quaternion = new Quaternion(player.rotation.x, player.rotation.y, player.rotation.z, player.rotation.w);
    const rotation: Vec3 = new Vec3(0, 0, 0);
    quat.toEuler(rotation);
    const xValid:boolean = player.position.x > area.minX && player.position.x < area.maxX;
    const zValid:boolean = player.position.z > area.minZ && player.position.z < area.maxZ;
    let ryValid:boolean=true;
    if (area.minRY != undefined) {
      ryValid = rotation.y > area.minRY && rotation.y < area.maxRY;
    }
    return (xValid && zValid && ryValid);
  }
}

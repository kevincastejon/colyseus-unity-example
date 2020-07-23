import {Room, Client, generateId} from 'colyseus';
import {State, PlayerData, Vect3, Quat} from './Schemas';
import {World, Body, Vec3, Quaternion, Sphere, Plane, Material, ContactMaterial, Transform, Mat3} from 'cannon-es';
const lightSwitchArea = {
  minX: -21.5,
  maxX: -18.5,
  minZ: 6,
  maxZ: 8,
  minRY: (Math.PI/180)*35,
  maxRY: (Math.PI/180)*145,
};
const doorArea = {
  minX: -38,
  maxX: -32,
  minZ: 1,
  maxZ: 6.5,
  minRY: -Math.PI,
  maxRY: Math.PI,
};
const ballRoomArea = {
  minX: -50.8,
  maxX: -35.8,
  minZ: -5,
  maxZ: 13,
  minRY: -Math.PI,
  maxRY: Math.PI,
};
export class ChatRoom extends Room {
  world:any;
  sphereBody:any;
  onCreate(options: any) {
    this.world = new World();
    this.world.gravity.set(0, -9.82, 0); // m/s²
    const radius = 0.20; // m
    const sphereMaterial = new Material();
    this.sphereBody = new Body({
      mass: 5, // kg
      position: new Vec3(-43, 4, 2.5), // m
      shape: new Sphere(radius),
      material: sphereMaterial,
    });
    this.world.addBody(this.sphereBody);
    // Create a plane
    const groundMaterial = new Material();
    const groundBody = new Body({
      mass: 0, // mass == 0 makes the body static
      material: groundMaterial,
    });
    const groundShape = new Plane();
    groundBody.addShape(groundShape);
    groundBody.quaternion.setFromAxisAngle(new Vec3(1, 0, 0), -Math.PI/2);
    this.world.addBody(groundBody);
    const matBallground = new ContactMaterial(groundMaterial, sphereMaterial, {friction: 1, restitution: 0.5});
    this.world.addContactMaterial(matBallground);
    this.setMetadata({
      name: options.name,
    });
    this.setPatchRate(1000 / 20);
    this.setSimulationInterval((dt) => this.update(dt));
    const state:State = new State();
    this.setState(state);
    this.refreshBallSpacialization();
    this.onMessage('input', (client, input)=>this.onPlayerMove(client, input));
    this.onMessage('emote', (client, emote)=>this.onPlayerEmote(client, emote));
    this.onMessage('chat', (client, msg)=>this.onPlayerChat(client, msg));
    this.onMessage('interact', (client)=>this.onPlayerInteract(client));
  }

  onJoin(client: Client, options: any) {
    console.log('player', client.id, 'connected');
    const newPlayer:PlayerData = new PlayerData();
    newPlayer.color=options.color;
    newPlayer.name=options.username;
    this.state.players[client.sessionId] = newPlayer;
    // this.sphereBody.applyLocalImpulse(new Vec3(10, 0, 0), new Vec3(0, 0, 0));
  }

  onLeave(client: Client, consented: boolean) {
    console.log('player', client.id, 'leaved');
    delete this.state.players[client.sessionId];
  }

  onDispose() {
    console.log('last player left, room closing...');
  }

  update(delta:number) {
    this.world.step(delta/1000);
    this.refreshBallSpacialization();
  }

  onPlayerMove(client:Client, playerInput) {
    const player = this.state.players[client.sessionId];
    if (playerInput.position.x != player.position.x ||
    playerInput.position.y != player.position.y ||
    playerInput.position.z != player.position.z
    ) {
      const pos = new Vect3();
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
      const rot = new Quat();
      rot.x = playerInput.rotation.x;
      rot.y = playerInput.rotation.y;
      rot.z = playerInput.rotation.z;
      rot.w = playerInput.rotation.w;
      player.rotation = rot;
    }
  }
  onPlayerEmote(client:Client, emote) {
    const player = this.state.players[client.sessionId];
    player.emote = emote;
  }
  onPlayerChat(client:Client, msg) {
    const player = this.state.players[client.sessionId];
    player.msg = msg;
  }
  onPlayerInteract(client:Client) {
    const player:PlayerData = this.state.players[client.sessionId];
    if (this.state.ball.owner === client.sessionId) {
      this.sphereBody.position=new Vec3(player.position.x, player.position.y, player.position.z);
      this.sphereBody.quaternion = new Quaternion(player.rotation.x, player.rotation.y, player.rotation.z, player.rotation.w);
      this.world.addBody(this.sphereBody);
      const forward = new Vec3(0, 0, 1);
      const dir = new Vec3(0, 0, 0);
      this.sphereBody.quaternion.vmult(forward, dir);
      dir.scale(10, dir);
      dir.y=5;
      this.sphereBody.velocity = dir;
      this.state.ball.owner = '';
    } else if (this.isPlayerInArea(player, lightSwitchArea)) {
      this.state.lights.on = !this.state.lights.on;
    } else if (this.isPlayerInArea(player, doorArea)) {
      this.state.doors.open = !this.state.doors.open;
    } else if (this.state.ball.owner === '' && this.isPlayerAround(player, this.sphereBody, 6)) {
      this.world.removeBody(this.sphereBody);
      this.state.ball.owner = client.sessionId;
    }
  }

  refreshBallSpacialization() {
    if (this.state.ball.position.x != this.sphereBody.position.x ||
    this.state.ball.position.y != this.sphereBody.position.y ||
    this.state.ball.position.z != this.sphereBody.position.z
    ) {
      const ballPos = new Vect3();
      ballPos.x = this.sphereBody.position.x;
      ballPos.y = this.sphereBody.position.y;
      ballPos.z = this.sphereBody.position.z;
      this.state.ball.position=ballPos;
    }
    if (this.state.ball.rotation.x != this.sphereBody.position.x ||
    this.state.ball.rotation.y != this.sphereBody.position.y ||
    this.state.ball.rotation.z != this.sphereBody.position.z ||
    this.state.ball.rotation.w != this.sphereBody.position.w
    ) {
      const ballRot = new Quat();
      ballRot.x = this.sphereBody.quaternion.x;
      ballRot.y = this.sphereBody.quaternion.y;
      ballRot.z = this.sphereBody.quaternion.z;
      ballRot.w = this.sphereBody.quaternion.w;
      this.state.ball.rotation=ballRot;
    }
    // console.log(this.sphereBody.position);
    // console.log(this.sphereBody.quaternion);
  }

  isPlayerInArea(player:PlayerData, area) {
    const quat = new Quaternion(player.rotation.x, player.rotation.y, player.rotation.z, player.rotation.w);
    const rotation = new Vec3(0, 0, 0);
    quat.toEuler(rotation);
    const xValid:boolean = player.position.x > area.minX && player.position.x < area.maxX;
    const zValid:boolean = player.position.z > area.minZ && player.position.z < area.maxZ;
    const ryValid:boolean = rotation.y > area.minRY && rotation.y < area.maxRY;
    return (xValid && zValid && ryValid);
  }

  isPlayerAround(player:PlayerData, body, radius:number) {
    // console.log(body.position.distanceTo(new Vec3(player.position.x, player.position.y, player.position.z)));
    return (body.position.distanceTo(new Vec3(player.position.x, player.position.y, player.position.z))<radius);
  }
}

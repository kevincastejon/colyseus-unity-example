# colyseus-unity-example
An example of using Colyseus.io nodejs server along with a Unity3D client

!! ISSUE !! Currently when the game is launched only the already created rooms will be visible on the room list, if a room is created by another player AFTER YOUR OWN INSTANCE OF THE GAME IS LAUNCHED it won't appear and will throw an error. Waiting for Endel's help to fix that ^^

## Overview
[Colyseus.io](https://colyseus.io/) is nodejs game server library made by [Endel](https://www.patreon.com/endel), it comes with many clients including a Unity3D one.

This repo is an example on how to use Colyseus server-side and Unity3d client-side. It synchronizes players positions/rotations into a small level, allows interactions like player's emotes, opening/closing doors, switching lights on/off, spawning/destroying/throwing balls and offers networked physics for these balls. Interactions are done by pressing the "E" key.

### Live Demo
[Try it online here](https://kevincastejon.github.io/colyseus-unity-example/)

### Non-autoritative movements
As Endel stated in [this presentation](https://docs.google.com/presentation/d/e/2PACX-1vSjJtmU-SIkng_bFQ5z1000M6nPSoAoQL54j0Y_Cbg7R5tRe9FXLKaBmcKbY_iyEpnMqQGDjx_335QJ/embed?start=false&loop=false&delayms=3000&slide=id.p), a game server should be fully autoritative.

In this example the player's movement are not, because it would have take to "convert" all the walls, floors, et other colliders from the Unity level design, into CannonJS bodies.

I had no time for that heavy work, so the players do not send their inputs, instead they will send their position/rotation, and the server will update it's own state.

Know that it's bad, hackers could travel all the game world at desired speed, wall passing etc...

Ps : I'm currently thinking of making a Unity plugin which would automatically converts scene's colliders into json or else.

### Autoritative interactions
The interactions (players pressing "E" key) are fully autoritative. It means that the players won't send a "I'm opening that door" message, rather they will send unique "I'm pressing E" message and the server will have to handle it by checking player's current position/rotation relative into the world and eventually executes some actions/modify it's state.

(eg: the player sent "interaction" message, its position is in front of the door, so the server will set the 'door' state to "opened" or "closed")

## Installation
```
git clone https://github.com/kevincastejon/colyseus-unity-example.git
cd colyseus-unity-example/server
npm i
npm start
```
Then open the Unity project or just launch the [build](https://github.com/kevincastejon/colyseus-unity-example/releases/download/0.1/colyseus-unity-example.rar)

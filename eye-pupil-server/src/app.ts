import WebSocket from 'ws';

import gazeApp from '@gaze/app';
import Subscriber from '@gaze/subscriber';
import { Pupil } from '@gaze/messages';

import { Vector2D, Vector3D, Circle, Ellipse, Sphere } from '@gaze/messages';

import options from '@gaze/options';


class ExVector2D {
    public x: number;
    public y: number;
    constructor(ref: Vector2D)  {
        this.x = ref.x;
        this.y = ref.y;
    }
}
class ExVector3D {
    public x: number;
    public y: number;
    public z: number;
    constructor(ref: Vector3D)  {
        this.x = ref.x;
        this.y = ref.y;
        this.z = ref.z;
    }
}
class ExCircle {
    public center: ExVector3D;
    public normal: ExVector3D;
    public radius: number;
    constructor(ref: Circle) {
        this.center = new ExVector3D(ref.center);
        this.normal = new ExVector3D(ref.normal);
        this.radius = ref.radius;
    }
}
class ExEllipse {
    public center: ExVector2D;
    public axes: ExVector2D;
    public angle: number;
    constructor(ref: Ellipse) {
        this.center = new ExVector2D(ref.center);
        this.axes = new ExVector2D(ref.axes);
        this.angle = ref.angle;
    }
}
  
class ExSphere {
    public center: ExVector3D;
    public radius: number
    constructor(ref: Sphere) {
        this.center = new ExVector3D(ref.center);
        this.radius = ref.radius;
    }
}

class ExPupil {
    public circle3d: ExCircle;
    public confidence: number;
    public timestamp: number;
    public diameter3d: number;
    public ellipse: ExEllipse;
    public normpos: ExVector2D;
    public diameter: number;
    public sphere: ExSphere;
    public projectedsphere: ExEllipse;
    public theta: number;
    public phi: number;
    public id: 0 | 1;
    constructor(ref: Pupil) {
        this.circle3d = new ExCircle(ref.circle_3d);
        this.confidence = ref.confidence;
        this.timestamp = ref.timestamp;
        this.diameter3d = ref.diameter_3d;
        this.ellipse = new ExEllipse(ref.ellipse);
        this.normpos = new ExVector2D(ref.norm_pos);
        this.diameter = ref.diameter;
        this.sphere = new ExSphere(ref.sphere);
        this.projectedsphere = new ExEllipse(ref.projected_sphere);
        this.theta = ref.theta;
        this.phi = ref.phi;
        this.id = ref.id;
    }
}

function simulatePupilDiameter() {
    return 4.5 + 0.1 * Math.random();
}

export default class App {
  private readonly server: WebSocket.Server;

  private static PORT = 51688;
  private static DATA_LOG_FREQ = 20

  private _dataLogCounter = 0;

  constructor() {
    this.server = new WebSocket.Server({ port: App.PORT });
    this.server.on( 'connection', (client, request) => {
      if (options.verbose) {
        console.log( `client connected, now ${this.server.clients.size} clients` );
      }

      client.on( 'close', (code, reason) => {
        if (options.verbose) {
          console.log( `connection from "${client.url}" closed ([${code}] ${reason})` );
        }
      });
      // client.on( 'message', data => {
      //   the request is in data.toString(), we do not need it in this project
      // });
    });
    this.server.on( 'error', err => {
      console.error( `error: ${err.message}` );
    });
    this.server.on( 'listen', () => {
      console.log( `Server is listening on port ${App.PORT}...` );
    });

    const subscribers = [
        Subscriber.create.pupil((pupil: Pupil) => this.onPupil( pupil ) ),
    ];

    gazeApp.start( subscribers );
  }

  private onPupil( pupil: Pupil ) {
    if (options.verbose) {
        if (this._dataLogCounter++ === App.DATA_LOG_FREQ) {
            this._dataLogCounter = 0;
            console.log(pupil.diameter, pupil.diameter_3d);
        }
    }

    const pupilSize = new ExPupil(pupil);

    this.server.clients.forEach( client => {
        client.send( JSON.stringify( pupilSize ) );
    });
  }

}

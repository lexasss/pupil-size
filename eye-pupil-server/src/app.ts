import WebSocket from 'ws';

import gazeApp from '@gaze/app';
import Subscriber from '@gaze/subscriber';
import { Pupil } from '@gaze/messages';

import options from '@gaze/options';


class PupilSize {
  constructor(
    public diam: number,
    public diam3d: number,
  ) { }
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

    if (options.simulated) {
      setInterval( () => this.simulate(), 10 );
    }
    else {
      const subscribers = [
        Subscriber.create.pupil((pupil: Pupil) => this.onPupil( pupil ) ),
      ];

      gazeApp.start( subscribers );
    }
  }

  private onPupil( pupil: Pupil ) {
    if (options.verbose) {
        if (this._dataLogCounter++ === App.DATA_LOG_FREQ) {
            this._dataLogCounter = 0;
            console.log(pupil.diameter, pupil.diameter_3d);
        }
    }

    const pupilSize = new PupilSize(
        pupil.diameter,
        pupil.diameter_3d,
    );

    this.server.clients.forEach( client => {
        client.send( JSON.stringify( pupilSize ) );
    });
  }

  private simulate() {
    var diam = simulatePupilDiameter();
    const pupilSize = new PupilSize(
        diam,
        diam * diam * Math.PI / 4,
    );

    this.server.clients.forEach( client => {
        client.send( JSON.stringify(pupilSize) );
    });
  }

}

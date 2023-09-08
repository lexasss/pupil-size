import commandLineArgs from 'command-line-args';

const CMD_OPT_DEFS = [
  { name: 'verbose', alias: 'v', type: Boolean, defaultValue: false, description: 'outputs debug information' },
  { name: 'help', alias: 'h', type: Boolean, defaultValue: false, description: 'prints this help info' },
  { name: 'simulated', alias: 's', type: Boolean, defaultValue: false, description: 'simulates input from Pupil Capture' },
]

const options = commandLineArgs( CMD_OPT_DEFS );

if (options.help) {
  console.log( `
  Usage: node index.js [parameters]

  parameters:
  `);

  CMD_OPT_DEFS.forEach( def => console.log( `    - ${def.name}: ${def.description}` ) );

  console.log( '\n' );
}

export default options;

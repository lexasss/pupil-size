# Server for pupil size events

## Requirements

Download and install the following applications:

- [Git](https://git-scm.com/)
- [NodeJS](https://nodejs.org/en/download/releases/) (NOTE: use version 12. Hint: use "NVM for Windows" to manage NodeJS versions)

## Install

Open a console in some folder. Run the following commands:

```bash
git clone https://github.com/lexasss/pupil-size-server.git
cd pupil-size-server
npm install
npm run build
```

## Dependencies

[ARVO Pupil Lab](https://github.com/tuni-eakr/pupil-labs) project must be cloned aside:

```bash
git clone https://github.com/tuni-eakr/pupil-labs.git
```

Modify `tsconfig.json` and `tslint.json` files of the current project and set the correct path to this dependency project.

## Run

Pupil Capture must be running before you run the application.

Run the server from a console as

```bash
npm run start
```

# A tool that displayes pupil size captured by Pupil Eye Tracker

## Note

Tested with
- Pupil Tracker model bought in year 2019
- Pupil Capture software installed in 2020

## Cloning

Open a console in some folder. Run the following commands:

```bash
git clone https://github.com/lexasss/pupil-size.git
```

## Requirements

Download and install the following applications:

- [Git](https://git-scm.com/)
- [NodeJS v12.x](https://nodejs.org/en/download/releases/) (Hint: use "NVM for Windows" to manage NodeJS versions)
- [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet)

Also, [ARVO Pupil Lab](https://github.com/tuni-eakr/pupil-labs) project must be cloned to the same folder that contains the PupilSize project folder:

```bash
git clone https://github.com/tuni-eakr/pupil-labs.git
```

## Building

### Build the server

**IMPORTANT!**
You need to set correct pathes to the ARVO Pupil Lab project in files `tsconfig.json` and `tslint.json`.
Also, you should edit the path to the server main JS file in file `package.json` and `prod-paths.js`.

``` bash
cd pupil-size\pupil-size-server
npm install
npm run build
````

### Build the display

Open the `display\Display` project in Visual Studio 2022 and build it in the Debug mode.


## Run

Pupil Capture must be running before you run the application.

Run both the server and the display by double-clicking on the `run.ps1` file in the project root folder

----

Note: you may need to enable running scripts in PowerShell. If so, follow this instruction:

- Start Windows PowerShell with the "Run as Administrator" option.
- Type `Set-ExecutionPolicy RemoteSigned` and press Enter

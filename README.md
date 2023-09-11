# A tool to monitor pupil size estimated by Pupil Core eye tracker

## Pupil Labs

The project was tested with
- [Pupil Core](https://pupil-labs.com/products/core/) model bought in year 2019
- [Pupil Capture](https://github.com/pupil-labs/pupil/releases/), v1.21.5 and the latest v3.5.


## Downloading the project

Open a console and run the following commands:

```bash
git clone https://github.com/lexasss/pupil-size.git
git clone https://github.com/tuni-eakr/pupil-labs.git
```

The latter command downloads [ARVO Pupil Lab](https://github.com/tuni-eakr/pupil-labs) project, as the current project depends on it.


## Requirements

Download and install the following tools:

- [NodeJS v12.x](https://nodejs.org/en/download/releases/) (Hint: use "NVM for Windows" to manage NodeJS versions)
- [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet)


## Building

### Server

**IMPORTANT!**
You need to set correct pathes to the ARVO Pupil Lab project in files `tsconfig.json` and `tslint.json` in `pupil-size-server` folder. If both `pupil-labs` and `pupil-size` are located in the same folder, then simply remove `../ARVO/` and you are good to go.

``` bash
cd pupil-size/pupil-size-server
npm install
npm run build
````

**IMPORTANT!**
After building the project, open the files `package.json` and `prod-paths.js` and remove `MIVI/` from the paths. Once done, you do not need to repeat this procedure after next build.

### Display

Open `display\Display.sln` solution in Visual Studio 2022 and build it in the `Debug` mode. If you prefer to build in `Release` mode, then edit the `run.ps1` file in the project root folder accordingly.

Compiled Display project can be downloaded from the `Release` section on the project GitHub page.


## Run

Pupil Capture with Pupil Remote enabled on port 50020 must be running before you start.

Run both the server and the display by double-clicking on the `run.ps1` file in the project root folder.

----

Note: you may need to enable running scripts in PowerShell. If so, follow this instruction:

- Start Windows PowerShell with the "Run as Administrator" option.
- Type `Set-ExecutionPolicy RemoteSigned` and press Enter

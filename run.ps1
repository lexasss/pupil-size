cd eye-pupil-server
cmd.exe /c 'start "PupilSize server" npm run start'
cd ..
Start-Sleep 2
& ".\display\bin\Debug\net6.0-windows\Display.exe"
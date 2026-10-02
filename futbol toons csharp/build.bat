@echo off
rem Compila Futbol Toons con el compilador de C# que ya trae Windows (.NET Framework 4)
cd /d "%~dp0"
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu /out:"Futbol Toons.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Core.dll *.cs
if errorlevel 1 (
  echo.
  echo Hubo errores al compilar.
  pause
  exit /b 1
)
echo Listo: "Futbol Toons.exe"

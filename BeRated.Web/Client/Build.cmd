@echo off
title TypeScript
setlocal enabledelayedexpansion
set output=Static\Script\Client.js
set main=Source\Main.ts
set sourceFiles=
for %%f in (Source\*.ts) do (
	if "%%f" neq "%main%" (
		set sourceFiles=!sourceFiles! "%%f"
	)
)
set sourceFiles=%sourceFiles% %main%
tsc --out %output% --removeComments %sourceFiles%
@echo on
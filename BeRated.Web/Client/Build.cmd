@echo off
title TypeScript
setlocal enabledelayedexpansion
set output=Static\Client.js
set main=TypeScript\Main.ts
set sourceFiles=
for %%f in (TypeScript\*.ts) do (
	if "%%f" neq "%main%" (
		set sourceFiles=!sourceFiles! "%%f"
	)
)
set sourceFiles=%sourceFiles% %main%
tsc --out %output% %sourceFiles%
@echo on
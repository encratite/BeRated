@echo off
setlocal enabledelayedexpansion
set sourceFiles=
for %%f in (TypeScript\*.ts) do (
	set sourceFiles=!sourceFiles! %%f
)
tsc --out Static\Client.js %sourceFiles%
@echo on
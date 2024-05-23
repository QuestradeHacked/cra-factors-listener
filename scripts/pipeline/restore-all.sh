#!/bin/bash

for projectName in *.csproj; do
	echo ========================================================
	echo Restoring .NET Core Project: $projectName
	echo ========================================================
	dotnet restore $projectName
done

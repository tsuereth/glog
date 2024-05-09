#!/usr/bin/env sh
set -e

GLOGGENERATOR=GlogGenerator/bin/Debug/net6.0/GlogGenerator

if [ ! -f "$GLOGGENERATOR" ]; then
    dotnet build GlogGenerator/GlogGenerator.csproj
fi

$GLOGGENERATOR host -i . -t GlogGenerator/templates/

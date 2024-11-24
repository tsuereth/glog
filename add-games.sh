#!/usr/bin/env sh
set -e

GLOGGENERATOR=GlogGenerator/bin/Debug/net8.0/GlogGenerator

if [ ! -f "$GLOGGENERATOR" ]; then
    dotnet build GlogGenerator/GlogGenerator.csproj
fi

IGDB_CLIENT_CREDSFILE=.igdb-client-credentials
if [ -f "$IGDB_CLIENT_CREDSFILE" ]; then
    IGDB_CLIENT_ID=$(cat "$IGDB_CLIENT_CREDSFILE" | cut -d ':' -f 1)
    IGDB_CLIENT_SECRET=$(cat "$IGDB_CLIENT_CREDSFILE" | cut -d ':' -f 2)
fi

$GLOGGENERATOR addgames --igdb-client-id "$IGDB_CLIENT_ID" --igdb-client-secret "$IGDB_CLIENT_SECRET" --add-igdb-game-ids $1

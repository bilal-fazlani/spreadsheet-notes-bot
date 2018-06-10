#!/usr/bin/env bash -xe

environment=${1:-test}

rm -rf ./publish
dotnet publish -o ./publish -c release
rm -rf ./publish/personal
heroku container:push -v web --app=ops-assistant-${environment}
heroku container:release web --app=ops-assistant-${environment}
rm -rf ./publish


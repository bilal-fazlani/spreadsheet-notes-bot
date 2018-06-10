#!/usr/bin/env bash -xe

rm -rf ./publish
dotnet publish -o ./publish -c release
rm -rf ./publish/personal
heroku container:push -v web --app=ops-assistant-test
heroku container:release web --app=ops-assistant-test
rm -rf ./publish


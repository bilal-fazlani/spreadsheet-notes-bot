FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine
COPY publish /app
WORKDIR /app
#ENV ASPNETCORE_ENVIRONMENT=Test
CMD ["dotnet", "SpreadsheetTextCapture.dll"]
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY ./futurum.openapi-to-plantuml-std-in-out ./futurum.openapi-to-plantuml-std-in-out
COPY ./futurum.openapi-to-plantuml ./futurum.openapi-to-plantuml

WORKDIR /app/futurum.openapi-to-plantuml-std-in-out
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app/futurum.openapi-to-plantuml-std-in-out
COPY --from=build-env /app/futurum.openapi-to-plantuml-std-in-out/out .
ENTRYPOINT ["dotnet", "Futurum.OpenApiToPlantumlStdInOut.dll"]
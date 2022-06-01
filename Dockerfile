FROM mcr.microsoft.com/dotnet/runtime:5.0-focal AS base
WORKDIR /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
# RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
# USER appuser

FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
WORKDIR /src
# Copy the source code to image and built the LPG simulation engine
COPY . .
RUN dotnet build "SimEngine2/SimEngine2.csproj"

FROM build AS publish
# Create self-contained executables (Todo: self-contained probably not necessary - best solution?)
RUN dotnet publish "SimEngine2/SimEngine2.csproj" -o /app/publish --configuration Release --self-contained true --runtime linux-x64 -f net5.0 --verbosity quiet 
RUN mv "/app/publish/profilegenerator-latest.db3" "/app/publish/profilegenerator.db3"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a folder for the input files
RUN mkdir /input

# Create a folder for the result files
RUN mkdir /results

# Todo: overwrite/remove the output directory that might be specified in the calcspec.json file

ENTRYPOINT "./SimEngine2" "ProcessHouseJob" "-J" "/input/request.json"
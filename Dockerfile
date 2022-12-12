FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

# install jq for removing invalid parameters from the request json file
RUN apt-get update
RUN apt-get upgrade -y
RUN apt-get install -y jq

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
# Copy the source code to image and built the LPG simulation engine
COPY . .
RUN dotnet build "SimEngine2/SimEngine2.csproj"

FROM build AS publish
RUN dotnet publish "SimEngine2/SimEngine2.csproj" -o /app/publish --configuration Release --self-contained true --runtime linux-x64 -f net6.0 --verbosity quiet 
RUN mv "/app/publish/profilegenerator-latest.db3" "/app/publish/profilegenerator.db3"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a folder for the input files
RUN mkdir /input

# Create a folder for the result files
RUN mkdir /results

# Hint: The command line documentation of the LPG (shown with -? argument) is interactive, so for that to work in docker 
# the "docker run" command has to be executed with the -t option (does not apply for other LPG commands such as ProcessHouseJob)

# First remove //-style line comments, then remove/overwrite invalid path parameters from the request.json, then start the calculation
ENTRYPOINT grep -v "//" "/input/request.json" | jq 'del(.PathToDatabase) | .CalcSpec.OutputDirectory="/results"' > "/input/converted_request.json" &&  \
    "./SimEngine2" "ProcessHouseJob" "-J" "/input/converted_request.json"
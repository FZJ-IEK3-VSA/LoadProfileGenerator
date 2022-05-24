# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/runtime:5.0

WORKDIR /lpg

# Install wget
RUN apt-get update && apt-get upgrade
RUN apt-get install -y wget
#RUN rm -rf /var/lib/apt/lists/*

# Download the LPG for Linux
RUN wget https://www.loadprofilegenerator.de/setup/LPG10.8.0_linux.zip

RUN apt-get install -y unzip
RUN unzip LPG10.8.0_linux.zip

# Make simengine2 executable
RUN chmod +x simengine2
#CMD ls -l simengine2

#CMD ./simengine2 -?
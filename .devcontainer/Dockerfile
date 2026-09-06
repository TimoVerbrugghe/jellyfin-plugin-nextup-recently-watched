FROM mcr.microsoft.com/devcontainers/dotnet:1-9.0-noble

USER root

RUN apt-get update \
    && apt-get install --yes --no-install-recommends docker.io dotnet-sdk-10.0 \
    && cp -a /usr/lib/dotnet/. /usr/share/dotnet/ \
    && ln -sf /usr/share/dotnet/dotnet /usr/local/bin/dotnet \
    && rm -rf /var/lib/apt/lists/*

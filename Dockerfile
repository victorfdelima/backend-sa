FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["WebApi/WebApi.csproj", "WebApi/"]
RUN dotnet restore "WebApi/WebApi.csproj"
COPY . .
WORKDIR "/src/WebApi"
RUN dotnet build "WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApi/WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y curl gnupg
RUN curl -L https://packagecloud.io/rabbitmq/rabbitmq-server/gpgkey | apt-key add -
RUN echo "deb https://packagecloud.io/rabbitmq/rabbitmq-server/debian buster main" > /etc/apt/sources.list.d/rabbitmq.list
RUN apt-get update && apt-get install -y rabbitmq-server
RUN rabbitmq-plugins enable rabbitmq_management

EXPOSE 44363
EXPOSE 5672
EXPOSE 15672

CMD service rabbitmq-server start && dotnet WebApi.dll

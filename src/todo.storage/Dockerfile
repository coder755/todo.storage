FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ARG dbn
ENV TODO_DB_ID=$dbn
ARG dbpw
ENV TODO_DB_PW=$dbpw

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["todo.storage/todo.storage.csproj", "todo.storage/"]
RUN dotnet restore "todo.storage/todo.storage.csproj"
COPY . .
WORKDIR "/src/todo.storage"
RUN dotnet build "todo.storage.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "todo.storage.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "todo.storage.dll"]

ENV ASPNETCORE_HTTP_PORTS=80
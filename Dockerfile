# ---------- Этап 1 ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
 
# Сначала только .csproj — слой с restore кэшируется, пока не меняются зависимости
COPY *.csproj ./
RUN dotnet restore
 
# Затем остальной код и публикация
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false
 
# ---------- Этап 2 ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
 
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "wt-lab7-7postgres-efcore-kleshchenok.dll"]
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/AcademicManager.Web/AcademicManager.Web.csproj", "src/AcademicManager.Web/"]
COPY ["src/AcademicManager.Application/AcademicManager.Application.csproj", "src/AcademicManager.Application/"]
COPY ["src/AcademicManager.Infrastructure/AcademicManager.Infrastructure.csproj", "src/AcademicManager.Infrastructure/"]
COPY ["src/AcademicManager.Domain/AcademicManager.Domain.csproj", "src/AcademicManager.Domain/"]

RUN dotnet restore "src/AcademicManager.Web/AcademicManager.Web.csproj"

COPY . .
WORKDIR "/src/src/AcademicManager.Web"
RUN dotnet build "AcademicManager.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AcademicManager.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "AcademicManager.Web.dll"]

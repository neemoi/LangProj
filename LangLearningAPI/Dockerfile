FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY LangLearningAPI/LangLearningAPI/LangLearningAPI.csproj LangLearningAPI/LangLearningAPI/
COPY LangLearningAPI/Application/Application.csproj LangLearningAPI/Application/
COPY LangLearningAPI/Domain/Domain.csproj LangLearningAPI/Domain/
COPY LangLearningAPI/Persistance/Persistance.csproj LangLearningAPI/Persistance/
COPY LangLearningAPI/Common/Common.csproj LangLearningAPI/Common/

RUN dotnet restore "LangLearningAPI/LangLearningAPI/LangLearningAPI.csproj"

COPY LangLearningAPI/LangLearningAPI/ LangLearningAPI/LangLearningAPI/
COPY LangLearningAPI/Application/ LangLearningAPI/Application/
COPY LangLearningAPI/Domain/ LangLearningAPI/Domain/
COPY LangLearningAPI/Persistance/ LangLearningAPI/Persistance/
COPY LangLearningAPI/Common/ LangLearningAPI/Common/

WORKDIR "/src/LangLearningAPI"
RUN dotnet build "LangLearningAPI/LangLearningAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LangLearningAPI/LangLearningAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y bash

ENTRYPOINT ["bash", "-c", "dotnet LangLearningAPI.dll migrate-and-run || true && dotnet LangLearningAPI.dll"]

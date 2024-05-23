FROM mcr.microsoft.com/dotnet/sdk:6.0 AS restore
WORKDIR /app/restore
COPY nuget.config .
COPY scripts/pipeline/restore-all.sh .
RUN chmod +x restore-all.sh 
COPY src/*/*.csproj ./
RUN ./restore-all.sh

FROM restore as build
WORKDIR /app
COPY src/ ./src/
COPY *.sln .
RUN dotnet build -c Release

FROM build as test
WORKDIR /app
COPY scripts/pipeline/run_test.sh .
ENTRYPOINT ["sh", "run_test.sh"]

FROM build as publish 
WORKDIR /app/src/CRA.FactorsListener.Core
RUN dotnet publish -o /app/publish -c Release --no-restore --no-build

FROM gcr.io/qt-shared-services-3w/dotnet:6.0 AS runtime
WORKDIR /app
COPY --chown=dotnet:dotnet --from=publish /app/publish .
USER dotnet
CMD [ "dotnet", "CRA.FactorsListener.Core.dll" ]
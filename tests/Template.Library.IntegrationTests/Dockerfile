FROM mcr.microsoft.com/dotnet/sdk:10.0

HEALTHCHECK NONE
USER 0

COPY ./.editorconfig ./
COPY ./Directory.Build.props ./
COPY ./src /src
COPY ./tests/.editorconfig /tests/
COPY ./tests/Directory.Build.targets /tests/
COPY ./tests/Template.Library.IntegrationTests /tests/Template.Library.IntegrationTests

WORKDIR /tests/Template.Library.IntegrationTests

RUN dotnet restore

RUN dotnet build

# bash allows to accept env variables with dots (e.g. Logging__LogLevel__System.Net.Http.HttpClient)
CMD ["bash", "-c", "dotnet test --settings ./Template.Library.IntegrationTests.runsettings --no-build --logger \"Console;Verbosity=normal\" | tee /artifacts/logs/Template.Library.IntegrationTests.log"]

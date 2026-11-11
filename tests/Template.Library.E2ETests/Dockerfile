FROM mcr.microsoft.com/dotnet/sdk:10.0

HEALTHCHECK NONE
USER 0

ARG TESTS_FILTER

COPY ./.editorconfig ./
COPY ./Directory.Build.props ./
COPY ./src /src
COPY ./tests/.editorconfig /tests/
COPY ./tests/Directory.Build.targets /tests/
COPY ./tests/Template.Library.E2ETests /tests/Template.Library.E2ETests

WORKDIR /tests/Template.Library.E2ETests

RUN dotnet restore

RUN dotnet build

# bash allows to accept env variables with dots (e.g. Logging__LogLevel__System.Net.Http.HttpClient)
CMD ["bash", "-c", "dotnet test --filter \"$TESTS_FILTER\" --settings ./Template.Library.E2ETests.runsettings --no-build --logger \"Console;Verbosity=normal\" | tee /artifacts/logs/Template.Library.E2ETests.log"]

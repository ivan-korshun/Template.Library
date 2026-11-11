FROM mcr.microsoft.com/dotnet/sdk:10.0

HEALTHCHECK NONE
USER 0

RUN dotnet tool install --global dotnet-ef --version 10.0.15
ENV PATH="${PATH}:/root/.dotnet/tools"

COPY ["src/Template.Library", "/src/Template.Library"]
COPY ["src/Template.Library.EntityFramework", "/src/Template.Library.EntityFramework"]
COPY ./Directory.Build.props ./src/Directory.Build.props
COPY ./.editorconfig ./

WORKDIR /src/Template.Library.EntityFramework

COPY ./src/Template.Library.EntityFramework/entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh

RUN dotnet restore

ENTRYPOINT ["/entrypoint.sh"]

FROM mcr.microsoft.com/mssql-tools

HEALTHCHECK NONE
USER 0

COPY ./scripts/LocalDB/init-db.sql /init-db.sql
COPY ./scripts/LocalDB/init-db-login.sql /init-db-login.sql
COPY ./scripts/LocalDB/init-db-user.sql /init-db-user.sql
COPY ./scripts/LocalDB/entrypoint.sh /entrypoint.sh

ENV PATH=$PATH:/opt/mssql-tools/bin

WORKDIR /

CMD ["/bin/bash", "./entrypoint.sh"]

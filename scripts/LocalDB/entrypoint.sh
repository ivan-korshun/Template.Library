#!/bin/bash
set -e

sqlcmd -S $SERVER -U sa -P $SA_PASSWORD -i init-db.sql
sqlcmd -S $SERVER -U sa -P $SA_PASSWORD -d $DATABASE_DB_NAME -i init-db-login.sql
sqlcmd -S $SERVER -U sa -P $SA_PASSWORD -d $DATABASE_DB_NAME -i init-db-user.sql

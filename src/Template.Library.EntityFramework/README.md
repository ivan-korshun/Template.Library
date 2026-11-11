# Entity Framework

## Install CLI

Initial: `dotnet tool install --global dotnet-ef --version 10.0.15`

Update: `dotnet tool update --global dotnet-ef --version 10.0.15`

## Create Migration

To create a migration the following needs to be done:

1. Modify the schema code in `EntityTypeConfiguration` folder.
2. Execute `dotnet ef migrations add $YourMigrationName` from this folder

## Update Database

To apply all migrations to the database which are not applied yet, the following needs to be done:

1. Ensure that `appsettings.json` file has correct connection string.
2. Run `dotnet ef database update` from this folder.

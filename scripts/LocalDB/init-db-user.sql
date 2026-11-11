IF NOT EXISTS(SELECT principal_id FROM sys.database_principals WHERE name = '$(CONSUMER_LOGIN_NAME)')
BEGIN
  CREATE USER $(CONSUMER_LOGIN_NAME) FOR LOGIN $(CONSUMER_LOGIN_NAME)
  EXEC sp_addrolemember N'db_datareader', N'$(CONSUMER_LOGIN_NAME)'
	EXEC sp_addrolemember N'db_datawriter', N'$(CONSUMER_LOGIN_NAME)'
END

IF NOT EXISTS(SELECT principal_id FROM sys.database_principals WHERE name = '$(OWNER_LOGIN_NAME)')
BEGIN
    CREATE USER $(OWNER_LOGIN_NAME) FOR LOGIN $(OWNER_LOGIN_NAME)
    EXEC sp_addrolemember N'db_owner', N'$(OWNER_LOGIN_NAME)'
END

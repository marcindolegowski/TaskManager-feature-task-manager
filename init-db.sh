#!/bin/bash

echo "Waiting for SqlServer"
sleep 10s

# Wykonywanie skryptu db-init.sql
echo "Running script db-init.sql..."
/opt/mssql-tools/bin/sqlcmd -S localhost,1433 -U SA -P "P@ssw0rd123!" -d master -i /db-init.sql

echo "Script db-init.sql compleated."

#!/bin/bash
# Wait for SQL Server to start
echo "Waiting for SQL Server to start..."
sleep 10s

# Run the SQL script to create the database and tables
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "#1324aAa1324" -d master -i /usr/src/app/init.sql


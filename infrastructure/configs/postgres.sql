CREATE DATABASE platform_db;
ALTER SYSTEM SET max_connections = 1000;
CREATE ROLE readonly_user WITH LOGIN PASSWORD '12345678';
GRANT pg_read_all_data TO readonly_user;
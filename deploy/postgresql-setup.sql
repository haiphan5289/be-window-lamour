-- PostgreSQL setup script for Windows Server
-- Run as a PostgreSQL superuser (e.g., postgres)
-- Usage: psql -U postgres -f postgresql-setup.sql

-- 1. Create dedicated user for the application
CREATE USER lamour WITH PASSWORD 'CHANGE_ME_STRONG_PASSWORD';

-- 2. Create database
CREATE DATABASE lamour_db OWNER lamour;

-- 3. Grant privileges
GRANT ALL PRIVILEGES ON DATABASE lamour_db TO lamour;

-- 4. Connect to lamour_db and grant schema privileges
\c lamour_db
GRANT ALL ON SCHEMA public TO lamour;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO lamour;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO lamour;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO lamour;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO lamour;

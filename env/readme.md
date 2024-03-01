# ENVIRONMENT STARTUP

## environment variables including license value are stored in .env so that file is excluded from source

## Steps

1. copy .env.example to .env

2. Open the PowerShell command prompt with `ADMIN` access

3. Run ./compose-init.ps1

4. Run docker compose build

5. Run docker compose up --detach
# RXERP Database Export Instructions (Schema + Data)

Use these steps to export all tables and their data from your local MySQL into a shareable SQL file for teammates.

Tables covered:
- UserCredentials
- Employees
- Vendors
- VendorFiles

Requirements:
- MySQL client tools (mysqldump, mysql) installed and in your PATH
- Your local DB credentials (host, user, password) and database name (rxerp)

## 1) Export schema and data (single file)

```zsh
mysqldump \
  -h 127.0.0.1 -P 3306 \
  -u root -p \
  --databases rxerp \
  --tables UserCredentials Employees Vendors VendorFiles \
  --skip-lock-tables \
  > docs/rxerp-full-$(date +%F).sql
```

- This creates docs/rxerp-full-YYYY-MM-DD.sql containing CREATE TABLE statements and INSERTs.
- When prompted, enter your MySQL password.

## 2) Export data-only (no CREATE TABLE)

Useful when teammates already ran `docs/database-setup.sql` to create tables.

```zsh
today=$(date +%F)
mysqldump \
  -h 127.0.0.1 -P 3306 \
  -u root -p \
  rxerp UserCredentials Employees Vendors VendorFiles \
  --no-create-info --skip-lock-tables \
  > docs/rxerp-data-$today.sql
```

## 3) Export schema-only (no data)

```zsh
mysqldump \
  -h 127.0.0.1 -P 3306 \
  -u root -p \
  rxerp UserCredentials Employees Vendors VendorFiles \
  --no-data \
  > docs/rxerp-schema-$(date +%F).sql
```

## 4) Importing on a teammate machine

- Option A: Full file (schema + data)

```zsh
mysql -h 127.0.0.1 -P 3306 -u root -p < docs/rxerp-full-YYYY-MM-DD.sql
```

- Option B: Run our setup script, then import data-only

```zsh
mysql -h 127.0.0.1 -P 3306 -u root -p < docs/database-setup.sql
mysql -h 127.0.0.1 -P 3306 -u root -p rxerp < docs/rxerp-data-YYYY-MM-DD.sql
```

## 5) Notes and tips

- If your MySQL user requires a different host/user/port, update the command flags accordingly.
- To avoid including sensitive data, sanitize rows before export or use --where filters.
- Large data sets: consider adding --quick to mysqldump for memory efficiency.
- If you use SSL or different auth, add the appropriate flags (e.g., --ssl-mode=REQUIRED).

import os
import databases
import sqlalchemy

# L'injectem per defecte si no està definida.
DATABASE_URL = os.getenv("DATABASE_URL", "postgresql://tamarro_user:tamarro_password@localhost:5432/tamarro_db")

database = databases.Database(DATABASE_URL)
metadata = sqlalchemy.MetaData()
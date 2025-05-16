import os
import databases
import sqlalchemy

# Llegir la URL de la base de dades des de les variables d'entorn.
# Proporcionar un valor per defecte per a desenvolupament local si no està definida.
# En producció (dins de Docker Compose), la variable d'entorn DATABASE_URL serà injectada.
DATABASE_URL = os.getenv("DATABASE_URL", "postgresql://tamarro_user:tamarro_password@localhost:5432/tamarro_db")
# Nota: localhost s'utilitza aquí com a fallback; quan s'executi amb docker-compose,
# la variable DATABASE_URL del compose (amb host 'db') tindrà preferència.

database = databases.Database(DATABASE_URL)
metadata = sqlalchemy.MetaData()
import sqlalchemy
from .database import metadata # Importem els metadatos de database.py
from sqlalchemy.sql import func # Per a server_default=func.now()

# Taula de puntuacions per temps
puntuacions_temps = sqlalchemy.Table(
    "puntuacions_temps",
    metadata,
    sqlalchemy.Column("id", sqlalchemy.Integer, primary_key=True, index=True, autoincrement=True),
    sqlalchemy.Column("nom_usuari", sqlalchemy.String, index=True, nullable=False),
    sqlalchemy.Column("temps_jugat", sqlalchemy.Integer, nullable=False), # En segons
    sqlalchemy.Column(
        "data_partida",
        sqlalchemy.TIMESTAMP(timezone=True),
        server_default=func.now(), # Valor per defecte a nivell de BD
        nullable=False
    ),
)

# Taula de puntuacions per enemics
puntuacions_enemics = sqlalchemy.Table(
    "puntuacions_enemics",
    metadata,
    sqlalchemy.Column("id", sqlalchemy.Integer, primary_key=True, index=True, autoincrement=True),
    sqlalchemy.Column("nom_usuari", sqlalchemy.String, index=True, nullable=False),
    sqlalchemy.Column("enemics_derrotats", sqlalchemy.Integer, nullable=False),
    sqlalchemy.Column(
        "data_partida",
        sqlalchemy.TIMESTAMP(timezone=True),
        server_default=func.now(), # Valor per defecte a nivell de BD
        nullable=False
    ),
)

# Mantenim la referència original per compatibilitat
puntuacions = puntuacions_temps  # Per a imports existents
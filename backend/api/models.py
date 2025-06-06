import sqlalchemy
from .database import metadata # Importem els metadatos de database.py
from sqlalchemy.sql import func # Per a server_default=func.now()

puntuacions = sqlalchemy.Table(
    "puntuacions",
    metadata,
    sqlalchemy.Column("id", sqlalchemy.Integer, primary_key=True, index=True, autoincrement=True),
    sqlalchemy.Column("nom_usuari", sqlalchemy.String, index=True, nullable=False),
    sqlalchemy.Column("temps_jugat", sqlalchemy.Integer, nullable=False), # En segons
    sqlalchemy.Column("enemics_derrotats", sqlalchemy.Integer, nullable=False),
    sqlalchemy.Column(
        "data_partida",
        sqlalchemy.TIMESTAMP(timezone=True),
        server_default=func.now(), # Valor per defecte a nivell de BD
        nullable=False
    ),
)
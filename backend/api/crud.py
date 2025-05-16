from typing import List
from . import schemas, models
from .database import database # Importem la instància de connexió

async def crear_puntuacion(puntuacion: schemas.PuntuacionCreate) -> schemas.Puntuacion:
    """
    Crea un nou registre de puntuació a la base de dades.
    """
    query = models.puntuacions.insert().values(
        nom_usuari=puntuacion.nom_usuari,
        tiemps_jugat=puntuacion.tiemps_jugat,
        enemics_derrotats=puntuacion.enemics_derrotats
        # data_partida s'estableix per defecte per la BD
    )
    # Executa la consulta i obté l'ID de l'últim registre inserit
    last_record_id = await database.execute(query)

    # Retorna el registre complet creat (incloent id i data_partida per defecte)
    # Per fer això, necessitem fer una altra consulta per obtenir el registre acabat d'inserir
    created_query = models.puntuacions.select().where(models.puntuacions.c.id == last_record_id)
    created_puntuacion_db = await database.fetch_one(created_query)

    # Assegurem que created_puntuacion_db no és None abans de crear l'objecte Puntuacion
    if created_puntuacion_db is None:
        raise Exception("No s'ha pogut recuperar la puntuació després de la creació.") # O un error HTTP més específic

    return schemas.Puntuacion.model_validate(created_puntuacion_db) # Pydantic V2


async def obtenir_puntuacions_per_temps(limit: int = 10) -> List[schemas.Puntuacion]:
    """
    Obté les 'limit' millors puntuacions ordenades per temps_jugado (ASC)
    i després per enemics_derrotats (DESC) com a desempat.
    """
    query = (
        models.puntuacions.select()
        .order_by(models.puntuacions.c.tiemps_jugat.asc(), models.puntuacions.c.enemics_derrotats.desc())
        .limit(limit)
    )
    resultats_db = await database.fetch_all(query)
    return [schemas.Puntuacion.model_validate(row) for row in resultats_db] # Pydantic V2

async def obtenir_puntuacions_per_enemics(limit: int = 10) -> List[schemas.Puntuacion]:
    """
    Obté les 'limit' millors puntuacions ordenades per enemics_derrotats (DESC)
    i després per tiemps_jugat (ASC) com a desempat.
    """
    query = (
        models.puntuacions.select()
        .order_by(models.puntuacions.c.enemics_derrotats.desc(), models.puntuacions.c.tiemps_jugat.asc())
        .limit(limit)
    )
    resultats_db = await database.fetch_all(query)
    return [schemas.Puntuacion.model_validate(row) for row in resultats_db] # Pydantic V2
from typing import List
from . import schemas, models
from .database import database # Importem la instància de connexió

async def crear_puntuacio(puntuacio: schemas.PuntuacioCreate) -> schemas.Puntuacio:
    """
    Crea un nou registre de puntuació a la base de dades.
    """
    query = models.puntuacions.insert().values(
        nom_usuari=puntuacio.nom_usuari,
        temps_jugat=puntuacio.temps_jugat,
        enemics_derrotats=puntuacio.enemics_derrotats
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

    return schemas.Puntuacio.model_validate(created_puntuacion_db) # Pydantic V2


async def obtenir_puntuacions_per_temps(limit: int = 10) -> List[schemas.Puntuacio]:
    """
    Obté les 'limit' millors puntuacions ordenades per temps_jugado (ASC)
    i després per enemics_derrotats (DESC) com a desempat.
    """
    query = (
        models.puntuacions.select()
        .order_by(models.puntuacions.c.temps_jugat.asc(), models.puntuacions.c.enemics_derrotats.desc())
        .limit(limit)
    )
    resultats_db = await database.fetch_all(query)
    return [schemas.Puntuacio.model_validate(row) for row in resultats_db] # Pydantic V2

async def obtenir_puntuacions_per_enemics(limit: int = 10) -> List[schemas.Puntuacio]:
    """
    Obté les 'limit' millors puntuacions ordenades per enemics_derrotats (DESC)
    i després per temps_jugat (ASC) com a desempat.
    """
    query = (
        models.puntuacions.select()
        .order_by(models.puntuacions.c.enemics_derrotats.desc(), models.puntuacions.c.temps_jugat.asc())
        .limit(limit)
    )
    resultats_db = await database.fetch_all(query)
    return [schemas.Puntuacio.model_validate(row) for row in resultats_db] # Pydantic V2

async def eliminar_puntuacio(id: int) -> bool:
    """
    Elimina una puntuació de la base de dades segons el seu ID.
    Retorna True si s'ha eliminat amb èxit, False si no s'ha trobat.
    """
    try:
        query = models.puntuacions.delete().where(models.puntuacions.c.id == id)
        files_afectades = await database.execute(query)
        
        # Gestionar el cas on files_afectades podria ser None
        if files_afectades is None:
            # Si la BD no retorna el nombre de files afectades, comprovem manualment si existeix
            check_query = models.puntuacions.select().where(models.puntuacions.c.id == id)
            exists = await database.fetch_one(check_query)
            return exists is None  # Si ja no existeix, l'operació va ser exitosa
        
        # Si tenim un valor numèric, comprovem si es va eliminar algun registre
        return files_afectades > 0
        
    except Exception as e:
        # Registra l'error per a diagnòstic (podria enviar-se a un sistema de logging)
        print(f"Error al eliminar puntuació: {str(e)}")
        return False
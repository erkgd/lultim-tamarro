from typing import List, Dict, Any
from datetime import datetime
from . import schemas, models
from .database import database # Importem la instància de connexió
import sqlalchemy

async def crear_puntuacio(puntuacio: schemas.PuntuacioCreate) -> schemas.Puntuacio:
    """
    Crea un nou registre de puntuació a ambdues taules de manera sincronitzada.
    """
    # Generem una data única per a ambdues insercions
    data_partida = datetime.now()
    
    async with database.transaction():
        # Inserir en puntuacions_temps
        query_temps = models.puntuacions_temps.insert().values(
            nom_usuari=puntuacio.nom_usuari,
            temps_jugat=puntuacio.temps_jugat,
            data_partida=data_partida
        )
        temps_id = await database.execute(query_temps)
        
        # Inserir en puntuacions_enemics amb la mateixa data
        query_enemics = models.puntuacions_enemics.insert().values(
            nom_usuari=puntuacio.nom_usuari,
            enemics_derrotats=puntuacio.enemics_derrotats,
            data_partida=data_partida
        )
        await database.execute(query_enemics)
    
    # Obtenir el registre complet mitjançant JOIN
    puntuacio_completa = await _obtenir_puntuacio_per_id(temps_id)
    
    if puntuacio_completa is None:
        raise Exception("No s'ha pogut recuperar la puntuació després de la creació.")
    
    return schemas.Puntuacio.model_validate(puntuacio_completa)

async def _obtenir_puntuacio_per_id(id: int) -> Dict[str, Any] | None:
    """
    Funció auxiliar per obtenir una puntuació completa mitjançant JOIN.
    """
    query = sqlalchemy.select(
        models.puntuacions_temps.c.id,
        models.puntuacions_temps.c.nom_usuari,
        models.puntuacions_temps.c.temps_jugat,
        models.puntuacions_enemics.c.enemics_derrotats,
        models.puntuacions_temps.c.data_partida
    ).select_from(
        models.puntuacions_temps.join(
            models.puntuacions_enemics,
            sqlalchemy.and_(
                models.puntuacions_temps.c.nom_usuari == models.puntuacions_enemics.c.nom_usuari,
                models.puntuacions_temps.c.data_partida == models.puntuacions_enemics.c.data_partida
            )
        )
    ).where(models.puntuacions_temps.c.id == id)
    
    return await database.fetch_one(query)

async def _obtenir_totes_puntuacions() -> List[Dict[str, Any]]:
    """
    Funció auxiliar per obtenir totes les puntuacions mitjançant JOIN.
    """
    query = sqlalchemy.select(
        models.puntuacions_temps.c.id,
        models.puntuacions_temps.c.nom_usuari,
        models.puntuacions_temps.c.temps_jugat,
        models.puntuacions_enemics.c.enemics_derrotats,
        models.puntuacions_temps.c.data_partida
    ).select_from(
        models.puntuacions_temps.join(
            models.puntuacions_enemics,
            sqlalchemy.and_(
                models.puntuacions_temps.c.nom_usuari == models.puntuacions_enemics.c.nom_usuari,
                models.puntuacions_temps.c.data_partida == models.puntuacions_enemics.c.data_partida
            )
        )
    )
    
    return await database.fetch_all(query)

async def obtenir_puntuacions_per_temps(limit: int = 10) -> List[schemas.Puntuacio]:
    """
    Obté les 'limit' millors puntuacions ordenades per temps_jugat (ASC)
    i després per enemics_derrotats (DESC) com a desempat.
    """
    resultats_db = await _obtenir_totes_puntuacions()
    
    # Ordenar per temps_jugat ASC, després per enemics_derrotats DESC
    resultats_ordenats = sorted(
        resultats_db,
        key=lambda x: (x['temps_jugat'], -x['enemics_derrotats'])
    )
    
    # Limitar els resultats
    resultats_limitats = resultats_ordenats[:limit]
    
    return [schemas.Puntuacio.model_validate(row) for row in resultats_limitats]

async def obtenir_puntuacions_per_enemics(limit: int = 10) -> List[schemas.Puntuacio]:
    """
    Obté les 'limit' millors puntuacions ordenades per enemics_derrotats (DESC)
    i després per temps_jugat (ASC) com a desempat.
    """
    resultats_db = await _obtenir_totes_puntuacions()
    
    # Ordenar per enemics_derrotats DESC, després per temps_jugat ASC
    resultats_ordenats = sorted(
        resultats_db,
        key=lambda x: (-x['enemics_derrotats'], x['temps_jugat'])
    )
    
    # Limitar els resultats
    resultats_limitats = resultats_ordenats[:limit]
    
    return [schemas.Puntuacio.model_validate(row) for row in resultats_limitats]

async def eliminar_puntuacio(id: int) -> bool:
    """
    Elimina una puntuació de ambdues taules segons el seu ID.
    Retorna True si s'ha eliminat amb èxit, False si no s'ha trobat.
    """
    try:
        # Primer obtenim les dades de la puntuació per poder eliminar de ambdues taules
        puntuacio_data = await _obtenir_puntuacio_per_id(id)
        
        if puntuacio_data is None:
            return False
        
        async with database.transaction():
            # Eliminar de puntuacions_temps
            query_temps = models.puntuacions_temps.delete().where(models.puntuacions_temps.c.id == id)
            await database.execute(query_temps)
            
            # Eliminar de puntuacions_enemics usant nom_usuari i data_partida
            query_enemics = models.puntuacions_enemics.delete().where(
                sqlalchemy.and_(
                    models.puntuacions_enemics.c.nom_usuari == puntuacio_data['nom_usuari'],
                    models.puntuacions_enemics.c.data_partida == puntuacio_data['data_partida']
                )
            )
            await database.execute(query_enemics)
        
        return True
        
    except Exception as e:
        print(f"Error al eliminar puntuació: {str(e)}")
        return False
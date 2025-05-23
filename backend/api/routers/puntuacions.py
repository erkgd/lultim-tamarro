from fastapi import APIRouter, HTTPException, status, Query
from typing import List
from .. import schemas, crud # Importem des del nivell superior

router = APIRouter(
    prefix="/puntuacions", # Prefix per a totes les rutes d'aquest router
    tags=["Puntuacions"],   # Etiqueta per a la documentació Swagger UI
    responses={404: {"description": "No trobat"}} # Resposta per defecte per a 404
)

@router.post("/", response_model=schemas.Puntuacio, status_code=status.HTTP_201_CREATED, summary="Crear una nova puntuació")
async def crear_nova_puntuacio(puntuacio: schemas.PuntuacioCreate):
    """
    Crea un nou registre de puntuació amb les dades proporcionades.
    - nom_usuari: Nom del jugador (str)
    - temps_jugat: Temps en segons (int > 0)
    - enemics_derrotats: Enemics derrotats (int >= 0)
    Retorna la puntuació creada amb el seu ID i data de sessió.
    """
    try:
        return await crud.crear_puntuacio(puntuacio=puntuacio)
    except Exception as e: # Captura més genèrica, podria ser més específica
        # Log de l'error e
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Error intern del servidor en crear la puntuació: {str(e)}"
        )


@router.get("/temps", response_model=List[schemas.Puntuacio], summary="Obtenir top 10 puntuacions per temps")
async def llegir_puntuacions_per_temps(
    limit: int = Query(10, ge=1, le=100, description="Nombre màxim de puntuacions a retornar")
):
    """
    Retorna les millors puntuacions (top 'limit', per defecte 10)
    ordenades per menor temps jugat.
    En cas d'empat en temps, es desempatarà per major nombre d'enemics derrotats.
    """
    return await crud.obtenir_puntuacions_per_temps(limit=limit)

@router.get("/enemics", response_model=List[schemas.Puntuacio], summary="Obtenir top 10 puntuacions per enemics")
async def llegir_puntuacions_per_enemics(
    limit: int = Query(10, ge=1, le=100, description="Nombre màxim de puntuacions a retornar")
):
    """
    Retorna les millors puntuacions (top 'limit', per defecte 10)
    ordenades per major nombre d'enemics derrotats.
    En cas d'empat en enemics, es desempatarà per menor temps jugat.
    """
    return await crud.obtenir_puntuacions_per_enemics(limit=limit)

@router.delete("/borrar", response_model=schemas.PuntuacioEliminarResposta, status_code=200)
async def eliminar_puntuacio(request: schemas.PuntuacioEliminar):
    """
    Elimina una puntuació segons el seu ID enviat per JSON.
    
    Retorna:
    - Un missatge de confirmació si s'ha eliminat amb èxit
    - Error 404 si no s'ha trobat cap puntuació amb aquest ID
    """
    eliminada = await crud.eliminar_puntuacio(request.id)
    if not eliminada:
        raise HTTPException(
            status_code=404, 
            detail=f"No s'ha trobat cap puntuació amb ID {request.id}"
        )
    return {"missatge": f"Puntuació amb ID {request.id} eliminada amb èxit", "id": request.id}
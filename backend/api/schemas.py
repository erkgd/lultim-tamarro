from pydantic import BaseModel, Field
import datetime

# Model base amb camps comuns
class PuntuacioBase(BaseModel):
    nom_usuari: str = Field(..., min_length=1, max_length=50, description="Nom de l'usuari")
    temps_jugat: int = Field(..., gt=0, description="Temps total jugat en segons") # gt = greater than
    enemics_derrotats: int = Field(..., ge=4, description="Nombre d'enemics derrotats") # ge = greater than or equal

# Model per a la creació d'una puntuació (dades d'entrada per al POST)
class PuntuacioCreate(PuntuacioBase):
    pass

# Model per representar una puntuació llegida de la base de dades (dades de sortida)
class Puntuacio(PuntuacioBase):
    id: int = Field(..., description="Identificador únic de la puntuació")
    data_partida: datetime.datetime = Field(..., description="Data i hora de la sessió de joc")

    # Per a Pydantic V2 (recomanat)
    class Config:
        from_attributes = True # Permet crear instàncies des d'atributs d'objectes (ex: models SQLAlchemy)

class PuntuacioEliminar(BaseModel):
    id: int = Field(..., description="ID de la puntuació a eliminar", gt=0)

class PuntuacioEliminarResposta(BaseModel):
    missatge: str = Field(..., description="Missatge de confirmació")
    id: int = Field(..., description="ID de la puntuació eliminada")
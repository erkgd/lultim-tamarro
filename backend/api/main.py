from fastapi import FastAPI
from contextlib import asynccontextmanager
from .database import database # Importem la instància de la base de dades
from .routers import puntuacions as puntuacions_router # Importem el router

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Lògica a executar abans que l'aplicació comenci a acceptar peticions
    await database.connect()
    print("Connexió a la base de dades establerta.")
    yield
    # Lògica a executar després que l'aplicació hagi processat les peticions i estigui a punt de tancar-se
    await database.disconnect()
    print("Connexió a la base de dades tancada.")

app = FastAPI(
    title="API de puntuacions del Videojoc L'Últim Tamarro",
    description="Una API per gestionar les puntuacions i classificacions del videojoc.",
    version="1.0.0",
    lifespan=lifespan # Assignem el gestor de cicle de vida
)

# Incloure routers
app.include_router(puntuacions_router.router)

@app.get("/", tags=["Root"])
async def read_root():
    return {"message": "Benvingut a l'API de puntuacions del Videojoc L'Últim Tamarro!"}
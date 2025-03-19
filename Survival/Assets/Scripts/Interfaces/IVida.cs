using System;

public interface IVida
{
    int VidaActual { get; }
    int VidaMaxima { get; }
    bool EsViu();
    void IncrementarVida(int quantitat, string font);
    void DecrementarVida(int quantitat, string font);
    event Action QuanCanviVida;
}
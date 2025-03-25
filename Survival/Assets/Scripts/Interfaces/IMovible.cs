using UnityEngine;

public interface IMovible
{
    float Velocitat { get; }
    void Moure();
    void AturarMoviment();
}
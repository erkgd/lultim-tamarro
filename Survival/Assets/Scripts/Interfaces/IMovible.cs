using UnityEngine;

public interface IMovible
{
    float Velocitat { get; }
    void Moure(Vector3 direccio);
    void AturarMoviment();
}
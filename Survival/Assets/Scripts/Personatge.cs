using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Personatge : MonoBehaviour
{
    private SistemaVida sistemaVida;

    void Start()
    {
        sistemaVida = GetComponent<SistemaVida>();
    }

    private void OnTriggerEnter(Collider other)
    {

    }
}
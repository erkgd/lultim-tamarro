using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SistemaVida : MonoBehaviour
{
    // Associem un enemic al sistema de vida
    private Enemic enemicAssociat;

    void Awake()
    {
        // Inicialitzem el component Enemic

    }

    // Start is called before the first frame update
    void Start()
    {
        //referència al component Enemic associat
        enemicAssociat = GetComponent<Enemic>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool EsViu()
    {
        return enemicAssociat != null && enemicAssociat.vidaActual > 0;
    }

    //carreguem la dependencia per morir
    protected IEnumerator Morir()
    {
        // Implementación directa como corrutina
        return enemicAssociat.Morir();
    }

    public void DecrementarVida(int quantitat)
    {
        if (enemicAssociat != null)
        {
            if (quantitat <= 0 || !EsViu()) return;

        // Evitamos modificar la vida del enemigo si ya está muriendo
        if (enemicAssociat.animator.GetBool("senseVida"))
            return;

        enemicAssociat.vidaActual -= quantitat;
        
        // Activamos la animación de recibir daño si no estamos muriendo
        if (enemicAssociat.vidaActual > 0 && enemicAssociat.animator != null)
            enemicAssociat.animator.SetTrigger("TrRepMal");

        // Notificamos el cambio de vida
        NotificarCanviVida();

        // Si la vida llega a 0, iniciamos la muerte
        if (enemicAssociat.vidaActual <= 0)
        {
            enemicAssociat.vidaActual = 0;
            StartCoroutine(Morir());
        }
        }
    }
}

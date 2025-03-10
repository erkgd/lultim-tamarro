using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovimentEnemics : MonoBehaviour
{

    [Header("Patrulla")]
    [SerializeField] private Transform puntsMoviment;
    private int puntActual;

    [Header("Components")]
    NavMeshAgent enemic;

    void Start()
    {
        enemic = GetComponent<NavMeshAgent>();

    }

    void Update()
    {
        if (enemic.remainingDistance <= 0.2f)
        {
            puntActual++;
            if (puntActual >= puntsMoviment.childCount)
            {
                puntActual = 0;
            }

            enemic.SetDestination(puntsMoviment.GetChild(puntActual).position);


        }
    }
}

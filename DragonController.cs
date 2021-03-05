using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DragonController : MonoBehaviour
{
    //Controlador de estado
    public enum EnemyState
    {
        Idle,
        Run,
        Attack,
        Die
    }

    //Movimiento del enemigo
    [SerializeField] NavMeshAgent _agent = null;

    //Contiene el controlador de la animación
    [SerializeField] Animator _ac = null;

    //Estado
    EnemyState State;

    void Start()
    {
        if (_ac == null)
        {
            _ac = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        }
        if (_agent == null)
        {
            _agent = GetComponent<NavMeshAgent>() ?? GetComponentInChildren<NavMeshAgent>();
        }
        SetState(EnemyState.Idle);
    }

    void Update()
    {
        _agent.SetDestination(PlayerController.Instance.transform.position);
    }

    //Acción al acabar el ataque
    public void AttackEnd()
    {
        UnityEngine.Debug.Log("Acabé");
    }

    //Acción al hacer el daño
    public void AttackDamage()
    {
        UnityEngine.Debug.Log("hata");
    }

    void SetState(EnemyState newState)
    {
        if (State == newState)
        {
            return;
        }
        switch (newState)
        {
            case EnemyState.Idle:
                PlayIdle();
                break;
            case EnemyState.Run:
                PlayRun();
                break;
            case EnemyState.Die:
                PlayDie();
                break;
            case EnemyState.Attack:
                PlayAttack();
                break;
        }
        State = newState;

    }

    //Controladores de animaciones
    //Quieto
    public void PlayIdle()
    {
        _ac.CrossFade("Idle", 0.1f);
    }

    //Se mueve
    public void PlayRun()
    {
        _ac.CrossFade("Run", 0.1f);
    }

    //Muere
    public void PlayDie()
    {
        _ac.CrossFade("Attack", 0.1f);

    }

    //Ataca
    public void PlayAttack()
    {
        _ac.CrossFade("Die", 0.1f);
    }
}

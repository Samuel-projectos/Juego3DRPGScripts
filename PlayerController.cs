using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    //Controlador de estado
    public enum PlayerState
    {
        Idle,
        Run,
        Attack,
        Die
    }

    //Contienen la velocidad
    [SerializeField] float _speed= 3f;

    //Contiene el controlador de la animación
    [SerializeField] Animator _ac = null;

    //Lista de trail Renderers que contiene el juego
    [SerializeField] List<GameObject> _trailRenderers = new List<GameObject>();

    //Rango de ataque
    [SerializeField] float _attackRange = 1.5f;

    //Ángulo de ataque
    [SerializeField] float _attackAngle = 90f;

    //Controlador de la vida del jugador
    [SerializeField] PlayerHealth _health = null;

    //Condición de victoria, llegar a Victoria
    [SerializeField] Transform _victory = null;

    //Lo que mostrará si gana
    [SerializeField] GameObject _win = null;

    //Lo que mostrará si pierde
    [SerializeField] GameObject _lose = null;

    //Estado
    PlayerState State;

    //Crea una instancia del jugador
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if( _ac == null)
        {
            _ac = GetComponent<Animator>() ?? GetComponentInChildren<Animator>(); 
        }
        SetState(PlayerState.Idle);
        _health.CurrentHealth = _health.MaxHealth;
    }

    void Update()
    {
        //Controlaque si está muerto
        if(State == PlayerState.Die)
        {
            return;
        }
        //controla si ha llegado al objetivo
        if (Vector3.Distance(transform.position, _victory.position) < 2f)
        {
            _win.SetActive(true);
            SetState(PlayerState.Idle);
            _health.CurrentHealth = 0;
            return;
        }
        //Giro y movimiento del personaje jugador
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        var direction = new Vector3(h, 0f, v).normalized;

        //Rango de ataque
        if(State != PlayerState.Attack)
        {
            if (direction.sqrMagnitude > 0f)
            {
                transform.position = transform.position - direction * Time.deltaTime * _speed;
                transform.LookAt(transform.position - direction);
                SetState(PlayerState.Run);
            }
            else
            {
                SetState(PlayerState.Idle);
            }
        }

        //Ataque del juegador
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetState(PlayerState.Attack);
        }
    }

    //Es golpeado y comprueba si muere
    public void Hit(int hit)
    {
        _health.CurrentHealth -= hit;
        if (_health.CurrentHealth <= 0)
        {
            _health.CurrentHealth = 0;
            SetState(PlayerState.Die);
            StartCoroutine(delayedDeath());
        }
    }

    IEnumerator delayedDeath()
    {
        yield return new WaitForSeconds(2f);
        _lose.SetActive(true);
    }

    //Acción al acabar el ataque
    public void AttackEnd()
    {
        foreach (var trail in _trailRenderers)
        {
            trail.SetActive(false);
        }
        SetState(PlayerState.Idle);
    }

    //Acción al hacer el daño
    public void AttackDamage()
    {
        foreach(var enemy in SkeletonController.Instances)
        {
            var epos = enemy.transform.position;
            if((epos - transform.position).sqrMagnitude < _attackRange * _attackRange)
            {
                Vector3 toTarget = (epos - transform.position).normalized;
                var cos = Vector3.Dot(toTarget, transform.forward);
                var angle = Mathf.Cos(_attackAngle * 0.5f);
                if ( angle < cos )
                {
                    enemy.Hit(1);
                }
            }
        }
    }

    //Cambiar estados
    void SetState(PlayerState newState)
    {
        if (State == newState)
        {
            return;
        }
        switch (newState)
        {
            case PlayerState.Idle:
                PlayIdle();
                break;
            case PlayerState.Run:
                PlayRun();
                break;
            case PlayerState.Die:
                PlayDie();
                break;
            case PlayerState.Attack:
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
        _ac.CrossFade("Die", 0.1f);

    }

    //Ataca
    public void PlayAttack()
    {
        _ac.CrossFade("Attack", 0.1f);
    }

    //Activa el trail
    public void StartTrail()
    {
        foreach (var trail in _trailRenderers)
        {
            trail.SetActive(true);
        }
    }

    //Desactiva el trail
    public void StopTrail()
    {
        foreach (var trail in _trailRenderers)
        {
            trail.SetActive(false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SkeletonController : MonoBehaviour
{
    public static List<SkeletonController> Instances = new List<SkeletonController>();

    //Controlador de estado
    public enum EnemyState
    {
        Idle,
        Run,
        Chase,
        Attack,
        Die
    }

    //Comprueba si está vivo el enemigo
    public bool IsAlive { get { return _health > 0; } }

    //Contiene el controlador de la animación
    [SerializeField] Animator _ac = null;

    //Contiene el controlador de la Barra de vida del enemigo
    [SerializeField] Transform _healthBar = null;

    //Contiene el controlador de la rotación Barra de vida del enemigo
    [SerializeField] Transform _healthBarRot = null;

    //Movimiento del enemigo
    [SerializeField] NavMeshAgent _agent = null;

    //Para comunicarnos con la animación del enemigo
    [SerializeField] AnimationTarget _animTarget = null;

    //Vida del enemigo
    [SerializeField] int _health = 2;

    //Rango de vision
    [SerializeField] float _vision = 10f;

    //Vida actual del enemigo
    int _currentHealth = 0;

    //Lista de rangos
    [SerializeField] List<Transform> _path = new List<Transform>();
    int _currentPath = 0;

    //Vida del jugador
    [SerializeField] PlayerHealth _playerHealth = null;

    //Estado
    EnemyState State;

    //Crea instancias de los enemigos
    private void Awake()
    {
        Instances.Add(this);
    }

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

        _animTarget.OnAnimationEvent += OnAnomationEvent;

        _currentHealth = _health;
        _healthBar.transform.localScale = Vector3.one;
        _agent.SetDestination(_path[_currentPath].position);
        SetState(EnemyState.Run);
    }

    //Recive si debe ejecutar "AttackDamage" o "AttackEnd"
    private void OnAnomationEvent(string obj)
    {
        switch (obj)
        {
            case "AttackDamage":
                AttackDamage();
                break;
            case "AttactEnd":
                AttackEnd();
                break;
        }
    }

    void Update()
    {
        //Comprueba que no está muerto
        if (State == EnemyState.Die || State == EnemyState.Attack)
        {

            return;
        }

        var player = PlayerController.Instance;
        var pp = PlayerController.Instance.transform.position;
        var distance = (pp - transform.position).sqrMagnitude;
        
        //Comprueba si está cerca el jugador para atacar
        if (_playerHealth.CurrentHealth > 0 && distance < 1.5f * 1.5f)
        {
            SetState(EnemyState.Attack);
            transform.LookAt(pp);
        }
        //Comprueba si está cerca el jugador para seguir
        else if (_playerHealth.CurrentHealth > 0 && distance < _vision * _vision)
        {
            _agent.SetDestination(pp);
            transform.LookAt(pp);
            SetState(EnemyState.Chase);
        }
        //Camina por el mundo
        else if (Vector3.Distance(transform.position, _path[_currentPath].position) < 1f)
        {
            _currentPath++;
            if(_currentPath >= _path.Count)
            {
                _currentPath = 0;
            }
            _agent.SetDestination(_path[_currentPath].position);
        }

        //Cuando mata al jugador vuelve a su punto inicial
        if ((State == EnemyState.Attack || State == EnemyState.Chase ) && _playerHealth.CurrentHealth < 1)
        {
            SetState(EnemyState.Run);
            _agent.SetDestination(_path[_currentPath].position);
        }
    }

    //Vida del enemigo quieto
    void LateUpdate()
    {
        _healthBarRot.rotation = Quaternion.Euler(new Vector3(-40f, 180f, 0f));
    }

    //Es golpeado y comprueba si muere
    public void Hit(int hit)
    {
        _currentHealth -= hit;
        if(_currentHealth <= 0)
        {
            _currentHealth = 0;
            SetState(EnemyState.Die);
        }
        _healthBar.transform.localScale = new Vector3(_currentHealth / (float)_health, 1f, 1f);
    }

    //Acción al acabar el ataque
    public void AttackEnd()
    {
        UnityEngine.Debug.Log("Acabé");
    }

    //Acción al hacer el daño
    public void AttackDamage()
    {
        var player = PlayerController.Instance;
        var pp = PlayerController.Instance.transform.position;
        var distance = (pp - transform.position).sqrMagnitude;

        //Comprueba si está cerca el jugador para atacar
        if (_playerHealth.CurrentHealth > 0 && distance < 1.5f * 1.5f)
        {
            PlayerController.Instance.Hit(1);
        }
    }

    //Cambiar estados
    void SetState(EnemyState newState)
    {
        if (State == newState)
        {
            return;
        }
        switch (newState)
        {
            case EnemyState.Idle:
                _agent.isStopped = true;
                PlayIdle();
                break;
            case EnemyState.Run:
                _agent.isStopped = false;
                PlayRun();
                break;
            case EnemyState.Chase:
                _agent.isStopped = false;
                PlayRun();
                break;
            case EnemyState.Die:
                _agent.isStopped = true;
                PlayDie();
                break;
            case EnemyState.Attack:
                _agent.isStopped = true;
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
        GetComponentInChildren<Collider>().enabled = false;
        _healthBarRot.gameObject.SetActive(false);
        _ac.CrossFade("Die", 0.1f);
        StartCoroutine(KilledCoroutine());
    }

    //Ataca
    public void PlayAttack()
    {
        _ac.CrossFade("Attack", 0.1f);
    }

    //Muere y desaparece
    IEnumerator KilledCoroutine()
    {
        yield return new WaitForEndOfFrame();
        _agent.enabled = false;
        float duration = 2f;
        yield return new WaitForSeconds(1f);
        while(duration > 0f)
        {
            transform.position = transform.position - transform.up * 0.5f * Time.deltaTime;
            yield return null;
            duration -= Time.deltaTime;
        }
        Destroy(gameObject);
    }

    //Trazados
    //private void OnDrawGizmos()
    //{
        //Vista del enemigo
        //Gizmos.DrawWireSphere(transform.position, _vision);
        //for(int i = 1; i < _path.Count; i++)
        //{
        //    Gizmos.DrawLine(_path[i - 1].position, _path[i].position);
        //}

        //Recorrido del enemigo
        //if(_path.Count > 1)
        //{
        //    Gizmos.DrawLine(_path[_path.Count-1].position, _path[0].position);
        //}
    //}
}

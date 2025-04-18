using UnityEngine;
using System.Collections;

public class InimigoTorreta : Inimigo
{
    #region Declaration

    [Header("Referências de ação do inimigo")]
    [Space(10)]
    public GameObject projectile;
    public Transform fireAction;

    [Header("Valores de ação do inimigo torreta")]
    [Space(10)]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float nextFire;
    [SerializeField] Vector3 direction;

    [Header("Foco do alvo")]
    [SerializeField] private float tempoDeFoco = 3f;
    private float tempoRestanteDeFoco;
    private bool temAlvoFixo = false;

    [SerializeField] private AnimatorTorreta animator;

    #endregion

    private void Awake(){
        cc = GetComponent<CharacterController>();
        animator = GetComponentInChildren<AnimatorTorreta>();
    }

    void Start(){
        target = EncontrarPlayerMaisProximo();
        if (target != null)
        {
            temAlvoFixo = true;
            tempoRestanteDeFoco = tempoDeFoco;
        }
    }

    void FixedUpdate()
    {
        base.ChecagemDeZonas();
        AtualizarAlvo();
        Movimento();
        Atacar();
    }

    protected override void Movimento()
    {        
        if(_playerNoCampoDeVisao && target != null)
        {
            direction = target.position - transform.position;
            animator.Olhar(direction);
            direction.y = 0;
        }
    }

    public override void Atacar()
    {
        if(_playerNaZonaDeAtaque && target != null && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            GameObject newProjectile = Instantiate(projectile, fireAction.transform.position, targetRotation);
            newProjectile.GetComponent<Projectile>().owner = this.gameObject;
            base.Atacar();
        }
    }

    void AtualizarAlvo()
    {
        if (temAlvoFixo)
        {
            tempoRestanteDeFoco -= Time.deltaTime;
            if (tempoRestanteDeFoco <= 0f || target == null || !PlayerNaZona(target))
            {
                temAlvoFixo = false;
            }
        }

        if (!temAlvoFixo)
        {
            Transform novoAlvo = EncontrarPlayerMaisProximo();
            if (novoAlvo != null)
            {
                target = novoAlvo;
                temAlvoFixo = true;
                tempoRestanteDeFoco = tempoDeFoco;
            }
            else
            {
                target = null;
            }
        }
    }

    Transform EncontrarPlayerMaisProximo()
    {
        Transform maisProximo = null;
        float menorDistancia = Mathf.Infinity;

        foreach (var jogador in GameManager.instance.jogadores)
        {
            float distancia = Vector3.Distance(transform.position, jogador.transform.position);
            if (distancia <= zonaDeAtaque && distancia < menorDistancia)
            {
                menorDistancia = distancia;
                maisProximo = jogador.transform;
            }
        }

        return maisProximo;
    }

    bool PlayerNaZona(Transform jogador)
    {
        return Vector3.Distance(transform.position, jogador.position) <= zonaDeAtaque;
    }

    protected override void TomaDano(int valor)
    {
        base.TomaDano(valor);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, campoDeVisao);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, zonaDeAtaque);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///Classe genérica usada para a construção de inimigos, eles herdaram seus valores e metodos base daqui.
///Herdam caso necessário, a implementação de todos os métodos não é obrigatoria.
///</summary>
public abstract class Inimigo : MonoBehaviour
{
    #region Declarações

    [Header("Valores genéricos dos inimigos")]
    [Space(10)]
    [SerializeField] protected int vidas;
    [SerializeField] protected int dano;
    [SerializeField] protected float velocidade;

    [Header("Valores de zonas de percepção do player")]
    [Space(10)]
    [SerializeField] protected float campoDeVisao;
    [SerializeField] protected float zonaDeAtaque;

    [Header("Referências do player")]
    [Space(10)]
    public Transform target;
    public LayerMask playerLayer;
    public int playerVidas = 3;
    
    protected CharacterController cc;

    [HideInInspector] public bool _playerNoCampoDeVisao;
    [HideInInspector] public bool _playerNaZonaDeAtaque;

    #endregion

    //Métodos estão protegidos para serem usados apenas das classes ques os herdarem
    protected virtual void ChecagemDeZonas()
    {
        //Checa se o player está no campo de visão ou na zona de ataque, para a tomada de ações...
        _playerNoCampoDeVisao = Physics.CheckSphere(transform.position, campoDeVisao, playerLayer);
        _playerNaZonaDeAtaque = Physics.CheckSphere(transform.position, zonaDeAtaque, playerLayer);
    }

    protected virtual void TomaDano(int valor)
    {
        vidas -= valor;

        if(vidas <= 0)
        {
            Morte();
        }
    }

    protected virtual void Movimento()
    {
        if(target != null)
        {
            Vector3 movDirecao = (target.position - transform.position);
            movDirecao.y = 0; //Garante que fique sempre no plano horizontal
            cc.Move(movDirecao * velocidade * Time.deltaTime);
        }

        Debug.Log("inimigo se moveu");
    }

    public virtual void Atacar()
    {
        playerVidas -= 1;
        Debug.Log("vida do player -1");
    }

    protected void Morte()
    {
        Destroy(gameObject);
    }

}

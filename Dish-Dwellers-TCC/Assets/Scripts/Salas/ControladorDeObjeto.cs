using UnityEngine;

public class ControladorDeObjeto : IResetavel, SincronizaMetodo
{
    [Header("<color=green>Componentes : </color>")][Space(10)]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform respawnPos;

    [Space(15)][Header("Objeto controlado :")][Space(10)]
    private GameObject objeto;

    [Space(15)][Header("Configurações")][Space(10)]
    [SerializeField] private bool spawnNoInicio = false;


    private void Start(){
        if(spawnNoInicio) 
            Spawn();
    }

    public override void OnReset(){   
        Reiniciar();
    }

    /// <summary>
    /// Caso não exista nenhum objeto atribuido ao campo do objeto controlado, instancia um novo objeto com base no prefab.
    /// </summary>
    [Sincronizar]
    public void Spawn(){
        gameObject.Sincronizar();
        if(objeto == null) objeto = Instantiate(prefab, respawnPos.position, transform.rotation);

        Destrutivel destrutivel = objeto.GetComponent<Destrutivel>();
        destrutivel.OnDestruido.AddListener(Respawn);
    }


    /// <summary>
    /// Transporta o objeto controlado para o ponto de respawn atribuido no componente e ativa ele.
    /// </summary>
    [Sincronizar]
    public void Respawn(){
        gameObject.Sincronizar();
        objeto.transform.position = respawnPos.position;

        if(!objeto.activeInHierarchy)
            objeto.SetActive(true);
    }

    /// <summary>
    /// Destroi o objeto controlado e reinicia o sistema
    /// </summary>
    [Sincronizar]
    public void Reiniciar(){
        gameObject.Sincronizar();
        Destroy(objeto);
        objeto = null;

        if(spawnNoInicio) 
            Spawn();
    }

}

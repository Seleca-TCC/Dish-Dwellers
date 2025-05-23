using UnityEngine;
using UnityEngine.Animations;

public class Carregador: MonoBehaviour {
    public Transform carregarTransform;
    public float forcaArremesso = 5f, alturaArremesso = 1.5f;
    [Range(0, 1)] public float influenciaDaInerciaNoArremesso = 0.33f;

    [Header("Pegar automaticamente")]
    public bool carregarSeCairNaArea = false;
    Collider[] collidersNaArea = new Collider[10];

    [Tooltip("Necessário apenas para pegar automaticamente")]
    public LayerMask layerCarregavel;

    [HideInInspector] public Carregavel carregado;
    Carregavel ultimoCarregado;
    float tempoLimpaUltimoCarregado = 0.25f; // Impede jogador de soltar e pegar um carregavel no mesmo momento
    float timerLimparUltimoCarregado = 0;

    int carregandoParentSourceId = -1;
    public bool estaCarregando => carregado != null;

    public System.Action<Carregavel> OnCarregar, OnSoltar; // Chamado quando o carregador carrega ou solta um objeto

    void FixedUpdate() {
        if (timerLimparUltimoCarregado > 0) {
            timerLimparUltimoCarregado -= Time.fixedDeltaTime;
            if (timerLimparUltimoCarregado <= 0) {
                ultimoCarregado = null;
            }
        }

        ProcuraCarregavelNaArea();
    }

    /// <summary>
    /// Função chamada apenas se está habilitado para o carregador pegar automaticamente um objeto que caia na área (desativado por padrão)
    /// </summary>
    void ProcuraCarregavelNaArea() {
        if (!carregarSeCairNaArea || carregado != null) return;

        int quant = Physics.OverlapSphereNonAlloc(carregarTransform.position, 0.1f, collidersNaArea, layerCarregavel);
        if (quant == 0 || (quant == 1 && collidersNaArea[0].gameObject == gameObject)) return; // Na maioria dos casos, não haverá carregáveis

        float menorDistancia = Mathf.Infinity;
        Carregavel carregavelProximo = null;

        for (int i = 0; i < quant; i++) {
            Collider collider = collidersNaArea[i];
            if (collider == null || collider.gameObject == gameObject) continue; // Ignora o próprio jogador

            float distancia = Vector3.Distance(carregarTransform.position, collider.transform.position);
            if (distancia < menorDistancia) {
                Carregavel carregavel = collider.GetComponent<Carregavel>();
                if (carregavel != null && carregavel.PodeInteragir(this) && carregavel != ultimoCarregado) {
                    menorDistancia = distancia;
                    carregavelProximo = carregavel;
                }
            }
        }

        if (carregavelProximo == null) return;

        CarregarNoAutomatico(carregavelProximo);
    }

    [Sincronizar]
    public void CarregarNoAutomatico(Carregavel carregavel) {
        gameObject.Sincronizar();
        carregavel.Carregar(this);
    }

    /// <summary>
    /// Chamado automaticamente por um objeto Carregavel (ver método Carregar(Carregador carregador))
    /// Carrega um objeto. Se o objeto já estiver carregado, não faz nada.
    /// </summary>
    /// <param name="carregavel">Objeto a ser carregado</param>
    public bool Carregar(Carregavel carregavel) {
        if (carregado != null || carregavel == null || carregavel == ultimoCarregado) return false;
        
        carregado = carregavel;
        ultimoCarregado = carregavel;


        carregandoParentSourceId = carregavel.parentConstraint.AddSource(new ConstraintSource() {
            sourceTransform = carregarTransform,
            weight = 1f
        });
        carregavel.parentConstraint.SetTranslationOffset(0, Vector3.zero);
        carregavel.parentConstraint.constraintActive = true; // Ativa o ParentConstraint para seguir o carregador


        Rigidbody cargaRigidbody = carregavel.GetComponent<Rigidbody>();
        if (cargaRigidbody != null) {
            carregavel.HandleSendoCarregado();
        }

        OnCarregar?.Invoke(carregado);
        return true;
    }

    /// <summary>
    /// Solta o objeto carregado. Se não houver nenhum objeto carregado, não faz nada.
    /// Se o objeto possuir um Rigidbody, ele será arremessado na direção especificada.
    /// </summary>
    /// <param name="direcao">Direção que o objeto vai ser solto</param>
    /// <param name="velocidade">Velocidade base do carregador</param>
    /// <param name="movendo">Se o carregador estava se movendo ao soltar o objeto (se True, adiciona 'velocidade' a força final com base no valor influenciaDaInerciaNoArremesso)</param>
    public void Soltar(Vector3 direcao, float velocidade = 0, bool movendo = false) {
        OnSoltar?.Invoke(carregado);

        carregado.parentConstraint.constraintActive = false; // Desativa o ParentConstraint
        carregado.parentConstraint.RemoveSource(carregandoParentSourceId); // Remove a fonte do ParentConstraint


        Rigidbody cargaRigidbody = carregado.GetComponent<Rigidbody>();
        carregado.HandleSolto();

        if (cargaRigidbody != null) {
            cargaRigidbody.position = carregarTransform.position;

            if (movendo)
                cargaRigidbody.linearVelocity = influenciaDaInerciaNoArremesso * (direcao * velocidade);

            Vector3 arremeco = direcao;
            arremeco.y = alturaArremesso;
            cargaRigidbody.AddForce(arremeco * forcaArremesso, ForceMode.Impulse);
        }

        carregado = null;
        timerLimparUltimoCarregado = tempoLimpaUltimoCarregado;
    }

    /// <summary>
    /// Previsão da trajetória de arremesso de um objeto sem considerar colisões.
    /// </summary>
    /// <param name="rigidbody">Rigidbody do objeto a ser arremessado</param>
    /// <param name="direcao">Direção do arremesso</param>
    /// <param name="forca">Força do arremesso</param>
    /// <param name="velocidadeInicial">Velocidade inicial do arremesso</param>
    /// <returns>Retorna um vetor de posições da trajetória, sem considerar posiveis colisões no caminho.</returns>
    public Vector3[] PreverArremesso(Rigidbody rigidbody, Vector3 direcao, float forca, Vector3 velocidadeInicial) {
        if (rigidbody == null) return null;
        
        int quantidadeMaxPontos = 20;
        float tempo = 10 * Time.fixedDeltaTime; // Intervalos de tempo

        Vector3[] pontos = new Vector3[quantidadeMaxPontos];
        Vector3 posicao = rigidbody.position;
        Vector3 velocidade = direcao * forca + velocidadeInicial;
        Vector3 gravidade = 0.5f * Physics.gravity * tempo * tempo;

        for (int i = 0; i < quantidadeMaxPontos; i++) {
            posicao += velocidade * tempo + gravidade;
            velocidade += (Physics.gravity * tempo);
            pontos[i] = posicao;
        }

        return pontos;
    }
}

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimadorPLayer : MonoBehaviour
{
    private Animator animator;

    #region Parâmetros do animator
    public static readonly int Anda = Animator.StringToHash(nameof(Anda));
    public static readonly int Caindo = Animator.StringToHash(nameof(Caindo));
    public static readonly int Carrega = Animator.StringToHash(nameof(Carrega));
    public static readonly int Arremesso = Animator.StringToHash(nameof(Arremesso));
    public static readonly int Morre = Animator.StringToHash(nameof(Morre));
    public static readonly int Dano = Animator.StringToHash(nameof(Dano));
    #endregion


    private void Start(){
        animator = GetComponent<Animator>();
    }

    #region Métodos de animação

    /// <summary>
    /// Atribui a animação de andar correta ao jogador, baseada no Vetor de velocidade.
    /// </summary>
    /// <param name="velocidade"></param>
    public void Mover(Vector3 velocidade){

        if(velocidade.x > 0){ // Vira para a esquerda
            transform.localScale = Vector3.Scale(transform.localScale, Vector3.right);
        }
        else if(velocidade.x < 0){// Vira para a direita
            transform.localScale = Vector3.Scale(transform.localScale, Vector3.left);
        }
        if(velocidade.z > 0){// Vira de costas
            Debug.Log("Vira de costas");
        }
        else if(velocidade.z < 0){ // Vira para a frente
            Debug.Log("Vira de frente");
        }
    }

    /// <summary>
    /// Atribui a animação de andar correta ao jogador, baseada no Vetor de velocidade.
    /// </summary>
    /// <param name="velocidade"></param>
    public void Mover(Vector2 velocidade){
        if(velocidade.x > 0){ // Vira para a direita
            transform.localScale = Vector3.Scale(transform.localScale, Vector3.right);
        }
        else if(velocidade.x < 0){// Vira para a esquerda
            transform.localScale = Vector3.Scale(transform.localScale, Vector3.left);
        }
        if(velocidade.y > 0){// Vira de costas      
            Debug.Log("Vira de costas");
        }
        else if(velocidade.y < 0){ // Vira para a frente
            Debug.Log("Vira de frente");
        }

        animator.SetBool(Anda, true);
    }

    /// <summary>
    /// Toca a animação de carregar objeto do personagem. Permanece na pose de carregar enquanto AtirarObjeto não for chamado.
    /// </summary>
    public void Carregar(){
        animator.SetTrigger(Carrega);
    }

    /// <summary>
    /// Toca a animação de arremesso do personagem.
    /// </summary>
    public void AtirarObjeto(){
        animator.SetTrigger(Arremesso);
    }

    /// <summary>
    /// Toca a animação de morte do personagem.
    /// </summary>
    public void Morte(){
        animator.SetTrigger(Morre);
    }

    /// <summary>
    /// Toca a animação de receber dano do personagem.
    /// </summary>
    public void TomarDano(){
        animator.SetTrigger(Dano);
    }

    #endregion
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class Portal : IResetavel, SincronizaMetodo {

    [SerializeField] private bool finalDaDemo;
    [SerializeField] private GameObject canvasFinalDaDemo;

    List<Player> playersNoPortal = new List<Player>();
    [SerializeField] private Transform spawnDeSaida;

    void Start() {
        Sincronizavel sin = GetComponent<Sincronizavel>();
        if (sin == null) sin = gameObject.AddComponent<Sincronizavel>();
    }

    public override void OnReset() {
        Debug.Log("Resetando portal");
        playersNoPortal.Clear();
        Debug.Log("Portal resetado");
        Debug.Log(playersNoPortal.Count);
    }

    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player")){
            PlayerEntra(other.gameObject);
        }
    }

    [Sincronizar]
    public void PlayerEntra(GameObject playerObj) {
        Player player = playerObj.GetComponent<Player>();
        if(player == null) return; // Se não for um player, não faz nada.
        if (playersNoPortal.Contains(player)) return;

        bool prosseguir = gameObject.Sincronizar(playerObj);
        if (!prosseguir) return;

        if (player.playerInput != null)
            player.playerInput.currentActionMap["Cancelar"].performed += SairDoPortal;
        
        playerObj.gameObject.SetActive(false);
        playersNoPortal.Add(player);
        
        // Caso os dois players tenham entrado na porta, passa de sala.
        PassarDeSala();

        Debug.Log("Players no portal : " + playersNoPortal.Count);
    }

    public void PassarDeSala() {
        if (playersNoPortal.Count < 2) return;
        
        if (finalDaDemo) VaiParaOFim();
        else GameManager.instance.PassaDeSala();
    }


    public void SairDoPortal(InputAction.CallbackContext context) {
        SairDoPortal();
    }

    [Sincronizar]
    public void SairDoPortal(){
        if(playersNoPortal.Count == 1){
            gameObject.Sincronizar();
            

            Player player = playersNoPortal[0];

            player.transform.position = spawnDeSaida.position + Vector3.up * 0.5f;    
            player.gameObject.SetActive(true);
            playersNoPortal.Remove(player);

            Debug.Log("<color=red>Saiu do portal.");

            if (player.playerInput != null)
                player.playerInput.currentActionMap["Cancelar"].performed -= SairDoPortal;
        }
    }

    public string cenaDoFim = "Fim";

    public void VaiParaOFim() {
        GameManager.instance.ForcarCenaAguardando();
        SceneManager.LoadScene(cenaDoFim, LoadSceneMode.Single);
    }
}

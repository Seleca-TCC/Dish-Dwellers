using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class CameraController : MonoBehaviour {

    // Informações da câmera : 
    ModoDeJogo modoDeJogoConfigurado = ModoDeJogo.INDEFINIDO;
    public bool ativo = true; // Substitui o modo "INATIVO" previamente implementado.

    [Header("Cameras:")]
    [Space(10)]

    [SerializeField] private CinemachineCamera introCamera;
    [SerializeField] private CinemachineCamera[] ccameras = new CinemachineCamera[2];
    [SerializeField] private CinemachinePositionComposer[] positionComposers = new CinemachinePositionComposer[2];
    [SerializeField] private Camera[] cameras = new Camera[2];

    [Space(15)]

    [Header("<color=green>Configuração da Camera Dividida :")]
    [Space(10)]

    [SerializeField] private SplitFollowTarget splitFollowTarget;
    [SerializeField] private float distancia;
    [Range(0, 100)][SerializeField] private float camDistMin;
    [Range(0, 100)][SerializeField] private float camDistMax;
    [Range(0, 100)][SerializeField] private float threshhold;
    [Range(0, 100)][SerializeField] private float tolerancia;
    [Range(0, 100)][SerializeField] private float fovMin;
    [Range(0, 100)][SerializeField] private float fovMax;

    [Space(15)]

    [Header("Pós-processamento : ")]
    [Space(10)]

    [SerializeField] private Volume postProc;
    private DepthOfField dof;
    [SerializeField] private float dofMin = 15;
    [SerializeField] private float dofMax = 22;

    [Space(15)]

    [Header("Configuração de Transições :")]
    [Space(10)]

    public UnityEvent onTerminarIntro;
    [SerializeField] private float tempoDBlendNormal = 0.5f;
    [SerializeField] private float tempoDBlendIntro = 2.0f;
    private bool podeTrocarCamera = false;
    private bool splitScreen = false;

    [Space(15)]


    //Titizim coisas
    [SerializeField] private TextMeshProUGUI nomeSalaText;
    [SerializeField] private TextMeshProUGUI nomeStageText;
    [SerializeField] private float fadeDuracao = 1.5f;
    [SerializeField] private float displayDuracao = 2.0f;


    private void Start() {
        DeterminaModoDeCamera();
        if (introCamera != null)
            FazerIntroducao();
        else {
            podeTrocarCamera = true;
            ConfigurarCameras();
        }

        postProc.profile.TryGet<DepthOfField>(out dof);
    }

    private void OnValidate() {
        for (int i = 0; i < 2; i++) {
            if (ccameras[i] != null && positionComposers[i] == null) {
                positionComposers[i] = ccameras[i].GetComponent<CinemachinePositionComposer>();
            }
        }
    }

    private void DeterminaModoDeCamera() {
        if (!ativo) return;

        modoDeJogoConfigurado = GameManager.instance.modoDeJogo;

        switch (modoDeJogoConfigurado) {
            case ModoDeJogo.SINGLEPLAYER:
                if (introCamera) {
                    onTerminarIntro.AddListener(() => {
                        GameManager.instance.OnTrocarControle += TrocarCamera;
                        TrocarCamera(GameManager.instance.playerAtual);
                    });
                } else {
                    GameManager.instance.OnTrocarControle += TrocarCamera;
                }
                break;

            case ModoDeJogo.MULTIPLAYER_LOCAL:

                UsarCameraDividida();
                break;

                UsarSegundaCam();
                if (!introCamera)
                    cameras[0].rect = new Rect(-0.5f, 0.0f, 1, 1);
                break;

            case ModoDeJogo.MULTIPLAYER_ONLINE:
                TrocarCamera1();
                break;
        }

    }

    private void UsarCameraDividida() {
        splitFollowTarget.gameObject.SetActive(true);
        ccameras[0].Follow = splitFollowTarget.transform;
    }

    private void UsarSegundaCam() {
        cameras[1].gameObject.SetActive(true);
        ccameras[1].OutputChannel = OutputChannels.Channel02;

        foreach (CinemachineCamera cam in ccameras) {
            cam.Priority = 1;
        }

        splitScreen = true;
    }

    private void LateUpdate() {
        if (modoDeJogoConfigurado == ModoDeJogo.MULTIPLAYER_LOCAL) {
            Vector3 vetorDist = splitFollowTarget.CalcularDistancia();

            distancia = vetorDist.magnitude;

            if (distancia > threshhold) {
                ccameras[0].Lens.FieldOfView = Mathf.Lerp(fovMin, fovMax, (distancia - threshhold) / tolerancia); // FOV
                positionComposers[0].CameraDistance = Mathf.Lerp(camDistMin, camDistMax, (distancia - threshhold) / tolerancia); // distancia da camera
                dof.gaussianStart.Override(Mathf.Lerp(dofMin, dofMax, (distancia - threshhold) / tolerancia));
            }
            else {
                ccameras[0].Lens.FieldOfView = fovMin;
                positionComposers[0].CameraDistance = camDistMin;
                dof.gaussianStart.Override(dofMin);
            }
        }
    }

    void OnDisable() {
        switch (modoDeJogoConfigurado) {
            case ModoDeJogo.SINGLEPLAYER:
                GameManager.instance.OnTrocarControle -= TrocarCamera;
                break;

            case ModoDeJogo.MULTIPLAYER_LOCAL:
                break;

            case ModoDeJogo.MULTIPLAYER_ONLINE:
                break;
        }
    }

    #region Camera de introdução

    private void FazerIntroducao() {
        StartCoroutine(Introducao());
    }

    IEnumerator Introducao() {
        SetTempoDeBlend(tempoDBlendIntro);

        introCamera.Priority = 2;

        yield return new WaitForSeconds(1.0f);

        introCamera.Priority = 0;
        ccameras[0].Priority = 1;
        if (splitScreen) ccameras[1].Priority = 1;

        if (splitScreen) {
            float timer = 2f; // Timer utilizado pra fazer a transição de camera full screen para split screen.
            float interpolador; // coeficiente de interpolação
            float y; // Valor que vai ser aplicado ao componente y do rect da camera.

            while (timer > 0) {
                timer -= Time.deltaTime;
                interpolador = timer / 2;
                y = Mathf.Lerp(-0.5f, 0.05f, interpolador);

                Debug.Log(y);

                cameras[0].rect = new Rect(y, 0, 1, 1);
                cameras[1].rect = new Rect(1 + y, 0, 1, 1);

                yield return new WaitForFixedUpdate();
            }
            Debug.Log("Terminou");
            cameras[0].rect = new Rect(-0.5f, 0, 1, 1);
            cameras[1].rect = new Rect(0.5f, 0, 1, 1);
        } else {
            yield return new WaitForSeconds(2.1f);
        }


        SetTempoDeBlend(tempoDBlendNormal);

        podeTrocarCamera = true;
        onTerminarIntro?.Invoke();
        ConfigurarCameras();

        StartCoroutine(MostrarNomesStageSala());

    }


    /// <summary>
    /// Muda o tempo de blend da camera(s) a depender do modo de camera.
    /// </summary>
    /// <param name="duracao"></param>
    private void SetTempoDeBlend(float duracao) {
        CinemachineBrain brain;
        if (splitScreen) {
            foreach (Camera cam in cameras) {
                brain = cam.GetComponent<CinemachineBrain>();
                brain.DefaultBlend.Time = duracao;
            }
        } else {
            brain = cameras[0].GetComponent<CinemachineBrain>();
            brain.DefaultBlend.Time = duracao;
        }
    }

    #endregion

    #region Configuração inicial

    private void ConfigurarCameras() {
        List<Player> players = GameManager.instance.jogadores;

        if (modoDeJogoConfigurado == ModoDeJogo.MULTIPLAYER_ONLINE) {
            Player jogador = players[0].isLocalPlayer ? players[0] : players[1];
            Player outro_jogador = players[0].isLocalPlayer ? players[1] : players[0];

            ccameras[0].Follow = jogador.transform;
            ccameras[1].Follow = outro_jogador.transform;

            return;
        }

        for (int i = 0; i < players.Count; i++) {
            if (players[i].qualPlayer == QualPlayer.Player1) ccameras[0].Follow = players[i].transform;
            else ccameras[1].Follow = players[i].transform;

            if (modoDeJogoConfigurado == ModoDeJogo.MULTIPLAYER_LOCAL) ccameras[0].Follow = splitFollowTarget.transform;
        }
    }

    // Alterna entre cameras.
    public void TrocarCamera(QualPlayer player) {
        if (player == QualPlayer.Player1) TrocarCamera1();
        else TrocarCamera2();
    }

    public void TrocarCamera1() {
        if (!podeTrocarCamera) return;
        ccameras[0].Priority = 1;
        ccameras[1].Priority = 0;
    }

    void TrocarCamera1(InputAction.CallbackContext ctx) {
        TrocarCamera1();
    }

    public void TrocarCamera2() {
        if (!podeTrocarCamera) return;
        ccameras[1].Priority = 1;
        ccameras[0].Priority = 0;
    }

    void TrocarCamera2(InputAction.CallbackContext ctx) {
        TrocarCamera2();
    }

    #endregion

    #region Nomes do Level

    private void SetAlpha(TextMeshProUGUI text, float alpha) {
        if (text == null) return;
        Color c = text.color;
        text.color = new Color(c.r, c.g, c.b, alpha);
    }

    private IEnumerator MostrarNomesStageSala() {
        //Pra garantir que o texto vai começar invisivel pq tava bugando
        SetAlpha(nomeStageText, 0);
        SetAlpha(nomeSalaText, 0);

        try {
            nomeSalaText.text = GameManager.instance.salaAtual.nomeDaSala;
        }
        catch {
            
        }

        //Fade In 
            float elapsedTime = 0f;
        while (elapsedTime < fadeDuracao) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuracao);
            SetAlpha(nomeStageText, alpha);
            SetAlpha(nomeSalaText, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(displayDuracao);

        //Fade Out 
        elapsedTime = 0f;
        while (elapsedTime < fadeDuracao) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuracao));
            SetAlpha(nomeStageText, alpha);
            SetAlpha(nomeSalaText, alpha);
            yield return null;
        }

        //Pra garantir o alpha zero no final
        SetAlpha(nomeStageText, 0);
        SetAlpha(nomeSalaText, 0);
    }

    // Coroutine para o fade de texto - mudei isso aqui pq tava bugado mas vou manter a lógica aqui pra caso der merda dps e eu lembrar oq fiz 
    
    // private IEnumerator FadeText(TextMeshProUGUI textElement, float startAlpha, float endAlpha, float duracao) {
    //     float elapsedTime = 0;
    //     Color color = textElement.color;

    //     while (elapsedTime < duracao) {
    //         elapsedTime += Time.deltaTime;
    //         float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duracao);
    //         textElement.color = new Color(color.r, color.g, color.b, alpha);
    //         yield return null;
    //     }

    //     textElement.color = new Color(color.r, color.g, color.b, endAlpha);
    // }

    #endregion

}

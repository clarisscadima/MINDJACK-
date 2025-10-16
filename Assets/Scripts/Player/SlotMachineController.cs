using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class SlotMachineController : MonoBehaviour
{
    [Header("Slot Machine UI Elements")]
    public TextMeshProUGUI[] slotDisplays;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI fichasText;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI apuestaTypeText;
    public Button spinButton;
    public Button exitButton;
    public Button switchBetButton;

    [Header("Slot Settings - TEXTO EN LUGAR DE EMOJIS")]
    public string[] symbols = { "CHERRY", "HEART", "SKULL" };

    [Header("Configuración de Tiempo")]
    public float spinDuration = 2f;

    [Header("Costos")]
    public int costoTiradaFichas = 5;
    public int costoTiradaHP = 10;

    [Header("Límites y Enfriamiento")]
    public int tiradasPorSesion = 5;
    public float tiempoEnfriamiento = 60f;
    private int tiradasRealizadas = 0;
    private bool enEnfriamiento = false;
    private float tiempoRestanteEnfriamiento = 0f;

    // Sistema de selección de apuesta
    private enum TipoApuesta { Fichas, HP }
    private TipoApuesta apuestaActual = TipoApuesta.Fichas;

    [Header("Audio")]
    public AudioClip spinSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip cooldownSound;
    public AudioClip phoneRingSound;
    public AudioClip switchBetSound;

    [Header("Sistema de Enemigos")]
    public GameObject siluetaEnemigaPrefab;
    public Transform[] spawnPoints;

    [Header("Sistema de Teléfono")]
    public PhoneSystem phoneSystem;
    public bool usePhoneSystem = true;

    private bool isSpinning = false;
    private AudioSource audioSource;
    private PlayerResources playerResources;

    // Probabilidades
    private float[] probabilidades = { 0.30f, 0.15f, 0.25f, 0.20f, 0.10f };

    void Start()
    {
        // Obtener referencias
        audioSource = GetComponent<AudioSource>();
        playerResources = FindObjectOfType<PlayerResources>();

        // Configurar botones
        if (spinButton != null) spinButton.onClick.AddListener(StartSpin);
        if (switchBetButton != null) switchBetButton.onClick.AddListener(SwitchBetType);
        if (exitButton != null) exitButton.onClick.AddListener(ExitSlotMachine);

        // Inicializar
        UpdateBetUI();
        ResetSlotDisplays();
        UpdateUI();

        UnityEngine.Debug.Log("🎰 Tragamonedas listo - Sistema de texto activado");
    }

    void SwitchBetType()
    {
        if (isSpinning || enEnfriamiento) return;

        apuestaActual = apuestaActual == TipoApuesta.Fichas ? TipoApuesta.HP : TipoApuesta.Fichas;
        PlaySound(switchBetSound);
        UpdateBetUI();
    }

    void UpdateBetUI()
    {
        if (apuestaTypeText != null)
        {
            string texto = apuestaActual == TipoApuesta.Fichas ?
                $"<color=#FFD93D>Tipo: FICHAS ({costoTiradaFichas} FICHAS)</color>" :
                $"<color=#FF6B6B>Tipo: SALUD ({costoTiradaHP} HP)</color>";

            apuestaTypeText.text = texto;
        }

        if (switchBetButton != null)
        {
            TextMeshProUGUI buttonText = switchBetButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = apuestaActual == TipoApuesta.Fichas ?
                    "CAMBIAR A HP" : "CAMBIAR A FICHAS";
            }
        }

        if (spinButton != null)
        {
            TextMeshProUGUI spinText = spinButton.GetComponentInChildren<TextMeshProUGUI>();
            if (spinText != null)
            {
                string costo = apuestaActual == TipoApuesta.Fichas ?
                    $"{costoTiradaFichas} FICHAS" : $"{costoTiradaHP} HP";

                spinText.text = $"SPIN ({costo})";
            }
        }
    }

    void StartSpin()
    {
        if (!isSpinning && !enEnfriamiento && slotDisplays.Length == 3)
        {
            bool puedePagar = false;
            string mensajeError = "";

            if (apuestaActual == TipoApuesta.Fichas)
            {
                if (playerResources != null && playerResources.GetFichas() >= costoTiradaFichas)
                {
                    puedePagar = playerResources.ApostarFichas(costoTiradaFichas);
                }
                else
                {
                    mensajeError = $"<color=yellow>No tienes fichas suficientes ({costoTiradaFichas} requeridas)</color>";
                }
            }
            else
            {
                if (playerResources != null && playerResources.GetSalud() > costoTiradaHP)
                {
                    puedePagar = playerResources.ApostarSalud(costoTiradaHP);
                }
                else
                {
                    mensajeError = $"<color=red>No tienes HP suficiente ({costoTiradaHP} requeridos)</color>";
                }
            }

            if (puedePagar)
            {
                StartCoroutine(SpinAnimation());
            }
            else if (!string.IsNullOrEmpty(mensajeError) && resultText != null)
            {
                resultText.text = mensajeError;
            }
        }
        else if (enEnfriamiento && resultText != null)
        {
            resultText.text = $"<color=orange>En enfriamiento: {tiempoRestanteEnfriamiento:F0}s</color>";
        }
    }

    IEnumerator SpinAnimation()
    {
        isSpinning = true;
        spinButton.interactable = false;
        switchBetButton.interactable = false;

        // Sonido de giro
        PlaySound(spinSound);

        if (resultText != null)
        {
            string tipoApuesta = apuestaActual == TipoApuesta.Fichas ? "fichas" : "HP";
            resultText.text = $"<color=yellow>GIRANDO... (Apuesta: {tipoApuesta})</color>";
        }

        // Obtener combinación
        int[] finalResults = new int[3];
        int combinacion = ObtenerCombinacionSegunProbabilidad();

        // Asignar símbolos
        switch (combinacion)
        {
            case 0: finalResults[0] = 0; finalResults[1] = 0; finalResults[2] = 0; break;
            case 1: finalResults[0] = 1; finalResults[1] = 1; finalResults[2] = 1; break;
            case 2: finalResults[0] = 1; finalResults[1] = 1; finalResults[2] = 0; break;
            case 3: finalResults[0] = 2; finalResults[1] = 1; finalResults[2] = 0; break;
            case 4: finalResults[0] = 2; finalResults[1] = 2; finalResults[2] = 2; break;
        }

        // Animación de giro
        Coroutine[] spinningCoroutines = new Coroutine[3];
        for (int i = 0; i < 3; i++)
        {
            if (i < slotDisplays.Length && slotDisplays[i] != null)
            {
                int slotIndex = i;
                spinningCoroutines[i] = StartCoroutine(SpinSlotAnimation(slotIndex, finalResults));
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (spinningCoroutines[i] != null) yield return spinningCoroutines[i];
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // Aplicar resultado
        AplicarResultado(combinacion, finalResults);

        tiradasRealizadas++;
        if (tiradasRealizadas >= tiradasPorSesion)
        {
            IniciarEnfriamiento();
        }

        isSpinning = false;
        UpdateUI();
        UpdateBetUI();
    }

    IEnumerator SpinSlotAnimation(int slotIndex, int[] finalResults)
    {
        if (slotIndex >= slotDisplays.Length || slotDisplays[slotIndex] == null)
            yield break;

        TextMeshProUGUI slotDisplay = slotDisplays[slotIndex];
        float elapsedTime = 0f;

        // Giro rápido (75% del tiempo total)
        while (elapsedTime < spinDuration * 0.75f)
        {
            int randomSymbol = Random.Range(0, symbols.Length);
            slotDisplay.text = $"<size=48><b>{symbols[randomSymbol]}</b></size>";
            elapsedTime += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // Giro lento (25% del tiempo total)
        while (elapsedTime < spinDuration)
        {
            int randomSymbol = Random.Range(0, symbols.Length);
            slotDisplay.text = $"<size=48><b>{symbols[randomSymbol]}</b></size>";
            elapsedTime += 0.2f;
            yield return new WaitForSecondsRealtime(0.2f);
        }

        // Resultado final
        slotDisplay.text = $"<size=48><b>{symbols[finalResults[slotIndex]]}</b></size>";
    }

    int ObtenerCombinacionSegunProbabilidad()
    {
        float randomValue = Random.value;
        float acumulado = 0f;

        for (int i = 0; i < probabilidades.Length; i++)
        {
            acumulado += probabilidades[i];
            if (randomValue <= acumulado) return i;
        }

        return 0;
    }

    void AplicarResultado(int combinacion, int[] resultados)
    {
        string resultadoVisual = $"{symbols[resultados[0]]} {symbols[resultados[1]]} {symbols[resultados[2]]}";
        string mensaje = "";
        Color colorMensaje = Color.white;

        switch (combinacion)
        {
            case 0: // CHERRY CHERRY CHERRY (30%)
                mensaje = $"TRES CEREZAS!\n+50 HP";
                colorMensaje = Color.green;
                if (playerResources != null) playerResources.ModificarSalud(50);
                PlaySound(winSound);
                break;

            case 1: // HEART HEART HEART (15%)
                mensaje = $"JACKPOT CORAZONES!\n+100 HP +25 Cordura";
                colorMensaje = Color.red;
                if (playerResources != null)
                {
                    playerResources.ModificarSalud(100);
                    playerResources.ModificarCordura(25);
                }
                PlaySound(winSound);
                break;

            case 2: // HEART HEART CHERRY (25%)
                mensaje = $"DOS CORAZONES!\n+30 HP";
                colorMensaje = Color.yellow;
                if (playerResources != null) playerResources.ModificarSalud(30);
                PlaySound(winSound);
                break;

            case 3: // SKULL HEART CHERRY (20%)
                mensaje = $"CALAVERA MIXTA!\n+15 HP -10 Cordura";
                colorMensaje = Color.magenta;
                if (playerResources != null)
                {
                    playerResources.ModificarSalud(15);
                    playerResources.ModificarCordura(-10);
                }
                PlaySound(loseSound);
                break;

            case 4: // SKULL SKULL SKULL (10%)
                mensaje = $"TRES CALAVERAS!\n-20 HP + 2 ENEMIGOS";
                colorMensaje = Color.black;
                if (playerResources != null) playerResources.ModificarSalud(-20);
                SpawnEnemigos(2);
                PlaySound(loseSound);
                break;
        }

        if (resultText != null)
        {
            resultText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(colorMensaje)}>{resultadoVisual}\n{mensaje}</color>";
        }
    }

    void SpawnEnemigos(int cantidad)
    {
        if (siluetaEnemigaPrefab != null && spawnPoints != null && spawnPoints.Length > 0)
        {
            for (int i = 0; i < cantidad && i < spawnPoints.Length; i++)
            {
                Instantiate(siluetaEnemigaPrefab, spawnPoints[i].position, Quaternion.identity);
            }
        }
    }

    void IniciarEnfriamiento()
    {
        enEnfriamiento = true;
        tiempoRestanteEnfriamiento = tiempoEnfriamiento;
        StartCoroutine(CooldownCoroutine());

       
        if (usePhoneSystem && phoneSystem != null)
        {
            phoneSystem.StartPhoneCall();
        }
        else
        {
         
            PlaySound(phoneRingSound);
            if (resultText != null)
            {
                resultText.text = "<color=orange>TELEFONO SUENA... Puedes jugar al tragamonedas</color>";
            }
        }
    }

    IEnumerator CooldownCoroutine()
    {
        while (tiempoRestanteEnfriamiento > 0)
        {
            tiempoRestanteEnfriamiento -= Time.unscaledDeltaTime;
            UpdateUI();
            yield return null;
        }

        enEnfriamiento = false;
        tiradasRealizadas = 0;
        PlaySound(cooldownSound);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (fichasText != null && playerResources != null)
        {
            fichasText.text = $"<b>FICHAS: {playerResources.GetFichas()}</b>";
        }

        if (cooldownText != null)
        {
            if (enEnfriamiento)
            {
                cooldownText.text = $"<color=orange>Enfriamiento: {tiempoRestanteEnfriamiento:F0}s</color>";
            }
            else
            {
                cooldownText.text = $"<color=green>Tiradas: {tiradasRealizadas}/{tiradasPorSesion}</color>";
            }
        }

        if (spinButton != null) spinButton.interactable = !isSpinning && !enEnfriamiento;
        if (switchBetButton != null) switchBetButton.interactable = !isSpinning && !enEnfriamiento;
    }

    void ResetSlotDisplays()
    {
        for (int i = 0; i < slotDisplays.Length; i++)
        {
            if (slotDisplays[i] != null)
            {
                slotDisplays[i].text = $"<size=48><b><color=#666666>?</color></b></size>";
            }
        }

        if (resultText != null)
        {
            resultText.text = "<b><color=white>Selecciona tipo de apuesta y presiona SPIN!</color></b>";
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void ExitSlotMachine()
    {
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null)
        {
            inventory.ExitSlotMachine();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isSpinning && !enEnfriamiento)
        {
            StartSpin();
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !isSpinning && !enEnfriamiento)
        {
            SwitchBetType();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitSlotMachine();
        }
    }
}





//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;
//using System.Collections;
//using Random = UnityEngine.Random;

//public class SlotMachineController : MonoBehaviour
//{
//    [Header("Slot Machine UI Elements")]
//    public TextMeshProUGUI[] slotDisplays; // Slot1, Slot2, Slot3
//    public TextMeshProUGUI resultText;
//    public Button spinButton;
//    public Button exitButton;

//    [Header("Slot Settings")]
//    public string[] symbols = { "@", "*", "+", "#", "$", "&" };
//    public float spinDuration = 2f;

//    [Header("Audio")]
//    public AudioClip spinSound;
//    public AudioClip winSound;
//    public AudioClip loseSound;

//    private bool isSpinning = false;
//    private AudioSource audioSource;
//    private PlayerInventory playerInventory;

//    void Start()
//    {
//        // Obtener referencias
//        audioSource = GetComponent<AudioSource>();
//        playerInventory = FindObjectOfType<PlayerInventory>();

//        // Configurar botones
//        if (spinButton != null)
//        {
//            spinButton.onClick.AddListener(StartSpin);
//            spinButton.interactable = true;
//        }

//        if (exitButton != null)
//        {
//            exitButton.onClick.AddListener(ExitSlotMachine);
//        }

//        // Resetear displays
//        ResetSlotDisplays();

//        UnityEngine.Debug.Log("🎰 Tragamonedas listo - Presiona SPIN");
//    }

//    void StartSpin()
//    {
//        if (!isSpinning && slotDisplays.Length == 3)
//        {
//            StartCoroutine(SpinAnimation());
//        }
//    }

//    IEnumerator SpinAnimation()
//    {
//        isSpinning = true;
//        spinButton.interactable = false;

//        UnityEngine.Debug.Log("🌀 INICIANDO ANIMACIÓN DE GIRO...");

//        // Sonido de giro
//        if (audioSource != null && spinSound != null)
//        {
//            audioSource.PlayOneShot(spinSound);
//        }

//        // Resetear resultado
//        if (resultText != null)
//        {
//            resultText.text = "<color=yellow>GIRANDO...</color>";
//        }

//        // Resultados finales
//        int[] finalResults = new int[3];

//        // Iniciar giro de TODOS los slots simultáneamente
//        Coroutine[] spinningCoroutines = new Coroutine[3];
//        for (int i = 0; i < 3; i++)
//        {
//            // ✅ VERIFICAR que el slot existe antes de iniciar
//            if (i < slotDisplays.Length && slotDisplays[i] != null)
//            {
//                int slotIndex = i;
//                spinningCoroutines[i] = StartCoroutine(SpinSlotAnimation(slotIndex, finalResults));
//            }
//        }

//        // Esperar a que todos los slots terminen de girar
//        for (int i = 0; i < 3; i++)
//        {
//            if (spinningCoroutines[i] != null)
//            {
//                yield return spinningCoroutines[i];
//            }
//        }

//        // Pequeña pausa dramática antes del resultado
//        yield return new WaitForSecondsRealtime(0.5f);

//        // Mostrar resultado final
//        CheckResult(finalResults);

//        isSpinning = false;
//        spinButton.interactable = true;

//        UnityEngine.Debug.Log("✅ Animación completada");
//    }

//    IEnumerator SpinSlotAnimation(int slotIndex, int[] finalResults)
//    {
//        // ✅ VERIFICACIÓN DE SEGURIDAD - asegurar que el slot existe
//        if (slotIndex >= slotDisplays.Length || slotDisplays[slotIndex] == null)
//        {
//            UnityEngine.Debug.LogError($"❌ Slot {slotIndex} no válido");
//            yield break;
//        }

//        TextMeshProUGUI slotDisplay = slotDisplays[slotIndex];
//        float elapsedTime = 0f;

//        UnityEngine.Debug.Log($"🌀 Slot {slotIndex + 1}: Iniciando animación");

//        // FASE 1: GIRO RÁPIDO (1.5 segundos)
//        while (elapsedTime < spinDuration * 0.75f)
//        {
//            // Cambiar símbolo rápidamente
//            int randomSymbol = Random.Range(0, symbols.Length);
//            slotDisplay.text = $"<size=48><b>{symbols[randomSymbol]}</b></size>";

//            elapsedTime += 0.1f;
//            yield return new WaitForSecondsRealtime(0.1f);
//        }

//        // FASE 2: GIRO MÁS LENTO (0.5 segundos)
//        while (elapsedTime < spinDuration)
//        {
//            int randomSymbol = Random.Range(0, symbols.Length);
//            slotDisplay.text = $"<size=48><b>{symbols[randomSymbol]}</b></size>";

//            elapsedTime += 0.2f;
//            yield return new WaitForSecondsRealtime(0.2f);
//        }

//        // RESULTADO FINAL para este slot
//        finalResults[slotIndex] = Random.Range(0, symbols.Length);
//        slotDisplay.text = $"<size=48><b>{symbols[finalResults[slotIndex]]}</b></size>";

//        UnityEngine.Debug.Log($"✅ Slot {slotIndex + 1} resultado: {symbols[finalResults[slotIndex]]}");
//    }

//    void CheckResult(int[] results)
//    {
//        string resultMessage = "";
//        Color resultColor = Color.white;

//        string resultadoVisual = $"{symbols[results[0]]} {symbols[results[1]]} {symbols[results[2]]}";
//        UnityEngine.Debug.Log($"🎯 RESULTADO FINAL: {resultadoVisual}");

//        // Verificar combinaciones ganadoras (SIN EMOJIS PROBLEMÁTICOS)
//        if (results[0] == results[1] && results[1] == results[2])
//        {
//            // JACKPOT - 3 iguales
//            resultMessage = $"JACKPOT!!!\n{resultadoVisual}\n¡GANASTE EL PREMIO MAYOR!";
//            resultColor = Color.green;

//            if (audioSource != null && winSound != null)
//            {
//                audioSource.PlayOneShot(winSound);
//            }
//        }
//        else if (results[0] == results[1] || results[1] == results[2] || results[0] == results[2])
//        {
//            // 2 iguales
//            resultMessage = $"¡Bien!\n{resultadoVisual}\nDos símbolos iguales\n¡Premio pequeño!";
//            resultColor = Color.yellow;

//            if (audioSource != null && winSound != null)
//            {
//                audioSource.PlayOneShot(winSound);
//            }
//        }
//        else
//        {
//            // Sin premio
//            resultMessage = $"{resultadoVisual}\nSin premio esta vez\n¡Sigue intentando!";
//            resultColor = Color.red;

//            if (audioSource != null && loseSound != null)
//            {
//                audioSource.PlayOneShot(loseSound);
//            }
//        }

//        // Mostrar resultado
//        if (resultText != null)
//        {
//            resultText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(resultColor)}>{resultMessage}</color>";
//        }
//    }

//    void ResetSlotDisplays()
//    {
//        for (int i = 0; i < slotDisplays.Length; i++)
//        {
//            // ✅ CORREGIDO: Usar slotDisplays[i] en lugar de slotDisplay
//            if (slotDisplays[i] != null)
//            {
//                slotDisplays[i].text = $"<size=48><b><color=#666666>?</color></b></size>";
//            }
//        }

//        if (resultText != null)
//        {
//            resultText.text = "<b><color=white>Presiona SPIN para jugar!</color></b>";
//        }
//    }

//    void ExitSlotMachine()
//    {
//        UnityEngine.Debug.Log("Saliendo del tragamonedas");

//        if (playerInventory != null)
//        {
//            playerInventory.ExitSlotMachine();
//        }
//    }

//    void Update()
//    {
//        // Tecla Space también funciona como SPIN
//        if (Input.GetKeyDown(KeyCode.Space) && !isSpinning && slotDisplays.Length == 3)
//        {
//            StartSpin();
//        }

//        // Tecla Escape para salir
//        if (Input.GetKeyDown(KeyCode.Escape))
//        {
//            ExitSlotMachine();
//        }
//    }
//}
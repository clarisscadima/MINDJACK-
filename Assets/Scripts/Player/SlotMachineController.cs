using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Random = UnityEngine.Random;

public class SlotMachineController : MonoBehaviour
{
    [Header("Slot Machine UI Elements")]
    public TextMeshProUGUI[] slotDisplays; // Slot1, Slot2, Slot3
    public TextMeshProUGUI resultText;
    public Button spinButton;
    public Button exitButton;

    [Header("Slot Settings")]
    public string[] symbols = { "@", "*", "+", "#", "$", "&" };
    public float spinDuration = 2f;

    [Header("Audio")]
    public AudioClip spinSound;
    public AudioClip winSound;
    public AudioClip loseSound;

    private bool isSpinning = false;
    private AudioSource audioSource;
    private PlayerInventory playerInventory;

    void Start()
    {
        // Obtener referencias
        audioSource = GetComponent<AudioSource>();
        playerInventory = FindObjectOfType<PlayerInventory>();

        // Configurar botones
        if (spinButton != null)
        {
            spinButton.onClick.AddListener(StartSpin);
            spinButton.interactable = true;
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitSlotMachine);
        }

        // Resetear displays
        ResetSlotDisplays();

        UnityEngine.Debug.Log("🎰 Tragamonedas listo - Presiona SPIN");
    }

    void StartSpin()
    {
        if (!isSpinning && slotDisplays.Length == 3)
        {
            StartCoroutine(SpinAnimation());
        }
    }

    IEnumerator SpinAnimation()
    {
        isSpinning = true;
        spinButton.interactable = false;

        UnityEngine.Debug.Log("🌀 INICIANDO ANIMACIÓN DE GIRO...");

        // Sonido de giro
        if (audioSource != null && spinSound != null)
        {
            audioSource.PlayOneShot(spinSound);
        }

        // Resetear resultado
        if (resultText != null)
        {
            resultText.text = "<color=yellow>GIRANDO...</color>";
        }

        // Resultados finales
        int[] finalResults = new int[3];

        // Iniciar giro de TODOS los slots simultáneamente
        Coroutine[] spinningCoroutines = new Coroutine[3];
        for (int i = 0; i < 3; i++)
        {
            // ✅ VERIFICAR que el slot existe antes de iniciar
            if (i < slotDisplays.Length && slotDisplays[i] != null)
            {
                int slotIndex = i;
                spinningCoroutines[i] = StartCoroutine(SpinSlotAnimation(slotIndex, finalResults));
            }
        }

        // Esperar a que todos los slots terminen de girar
        for (int i = 0; i < 3; i++)
        {
            if (spinningCoroutines[i] != null)
            {
                yield return spinningCoroutines[i];
            }
        }

        // Pequeña pausa dramática antes del resultado
        yield return new WaitForSecondsRealtime(0.5f);

        // Mostrar resultado final
        CheckResult(finalResults);

        isSpinning = false;
        spinButton.interactable = true;

        UnityEngine.Debug.Log("✅ Animación completada");
    }

    IEnumerator SpinSlotAnimation(int slotIndex, int[] finalResults)
    {
        // ✅ VERIFICACIÓN DE SEGURIDAD - asegurar que el slot existe
        if (slotIndex >= slotDisplays.Length || slotDisplays[slotIndex] == null)
        {
            UnityEngine.Debug.LogError($"❌ Slot {slotIndex} no válido");
            yield break;
        }

        TextMeshProUGUI slotDisplay = slotDisplays[slotIndex];
        float elapsedTime = 0f;

        UnityEngine.Debug.Log($"🌀 Slot {slotIndex + 1}: Iniciando animación");

        // FASE 1: GIRO RÁPIDO (1.5 segundos)
        while (elapsedTime < spinDuration * 0.75f)
        {
            // Cambiar símbolo rápidamente
            int randomSymbol = Random.Range(0, symbols.Length);
            slotDisplay.text = $"<size=48><b>{symbols[randomSymbol]}</b></size>";

            elapsedTime += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // FASE 2: GIRO MÁS LENTO (0.5 segundos)
        while (elapsedTime < spinDuration)
        {
            int randomSymbol = Random.Range(0, symbols.Length);
            slotDisplay.text = $"<size=48><b>{symbols[randomSymbol]}</b></size>";

            elapsedTime += 0.2f;
            yield return new WaitForSecondsRealtime(0.2f);
        }

        // RESULTADO FINAL para este slot
        finalResults[slotIndex] = Random.Range(0, symbols.Length);
        slotDisplay.text = $"<size=48><b>{symbols[finalResults[slotIndex]]}</b></size>";

        UnityEngine.Debug.Log($"✅ Slot {slotIndex + 1} resultado: {symbols[finalResults[slotIndex]]}");
    }

    void CheckResult(int[] results)
    {
        string resultMessage = "";
        Color resultColor = Color.white;

        string resultadoVisual = $"{symbols[results[0]]} {symbols[results[1]]} {symbols[results[2]]}";
        UnityEngine.Debug.Log($"🎯 RESULTADO FINAL: {resultadoVisual}");

        // Verificar combinaciones ganadoras (SIN EMOJIS PROBLEMÁTICOS)
        if (results[0] == results[1] && results[1] == results[2])
        {
            // JACKPOT - 3 iguales
            resultMessage = $"JACKPOT!!!\n{resultadoVisual}\n¡GANASTE EL PREMIO MAYOR!";
            resultColor = Color.green;

            if (audioSource != null && winSound != null)
            {
                audioSource.PlayOneShot(winSound);
            }
        }
        else if (results[0] == results[1] || results[1] == results[2] || results[0] == results[2])
        {
            // 2 iguales
            resultMessage = $"¡Bien!\n{resultadoVisual}\nDos símbolos iguales\n¡Premio pequeño!";
            resultColor = Color.yellow;

            if (audioSource != null && winSound != null)
            {
                audioSource.PlayOneShot(winSound);
            }
        }
        else
        {
            // Sin premio
            resultMessage = $"{resultadoVisual}\nSin premio esta vez\n¡Sigue intentando!";
            resultColor = Color.red;

            if (audioSource != null && loseSound != null)
            {
                audioSource.PlayOneShot(loseSound);
            }
        }

        // Mostrar resultado
        if (resultText != null)
        {
            resultText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(resultColor)}>{resultMessage}</color>";
        }
    }

    void ResetSlotDisplays()
    {
        for (int i = 0; i < slotDisplays.Length; i++)
        {
            // ✅ CORREGIDO: Usar slotDisplays[i] en lugar de slotDisplay
            if (slotDisplays[i] != null)
            {
                slotDisplays[i].text = $"<size=48><b><color=#666666>?</color></b></size>";
            }
        }

        if (resultText != null)
        {
            resultText.text = "<b><color=white>Presiona SPIN para jugar!</color></b>";
        }
    }

    void ExitSlotMachine()
    {
        UnityEngine.Debug.Log("Saliendo del tragamonedas");

        if (playerInventory != null)
        {
            playerInventory.ExitSlotMachine();
        }
    }

    void Update()
    {
        // Tecla Space también funciona como SPIN
        if (Input.GetKeyDown(KeyCode.Space) && !isSpinning && slotDisplays.Length == 3)
        {
            StartSpin();
        }

        // Tecla Escape para salir
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitSlotMachine();
        }
    }
}
using UnityEngine;
using TMPro;
using System.Collections;

public class PhoneSystem : MonoBehaviour
{
    [Header("Referencias del Teléfono")]
    public GameObject phoneObject;
    public AudioClip ringSound;
    public AudioClip voiceMessage;

    [Header("Configuración de Vibración")]
    public float vibrationIntensity = 0.1f;
    public float vibrationSpeed = 30f;
    public float ringDuration = 10f;

    [Header("UI de Interacción")]
    public TextMeshProUGUI phonePromptText;
    public string promptMessage = "PRESIONA L PARA CONTESTAR";

    [Header("Mensaje de Voz")]
    [TextArea]
    public string voiceMessageText = "¡Puedes jugar al tragamonedas!";

    [Header("Configuración de Trigger")]
    public float interactionDistance = 3f;
    public Transform playerTransform;

    private bool isRinging = false;
    private bool isAnswered = false;
    private bool playerInRange = false;
    private Vector3 phoneOriginalPosition;
    private AudioSource audioSource;
    private Coroutine vibrationCoroutine;
    private Coroutine ringCoroutine;

    void Start()
    {
        // Obtener referencias
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Buscar jugador automáticamente si no está asignado
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        // Guardar posición original
        if (phoneObject != null)
            phoneOriginalPosition = phoneObject.transform.localPosition;

        // Ocultar prompt inicialmente
        if (phonePromptText != null)
            phonePromptText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Verificar distancia con el jugador
        CheckPlayerDistance();

        // Mostrar/ocultar prompt según distancia
        UpdatePromptVisibility();

        // Tecla para contestar solo si está en rango y sonando
        if (isRinging && !isAnswered && playerInRange && Input.GetKeyDown(KeyCode.L))
        {
            AnswerPhone();
        }
    }

    void CheckPlayerDistance()
    {
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            playerInRange = distance <= interactionDistance;
        }
    }

    void UpdatePromptVisibility()
    {
        if (phonePromptText != null)
        {
            // Mostrar solo si está sonando y el jugador está cerca
            bool shouldShow = isRinging && !isAnswered && playerInRange;

            if (shouldShow && !phonePromptText.gameObject.activeInHierarchy)
            {
                phonePromptText.text = $"<color=green>{promptMessage}</color>";
                phonePromptText.gameObject.SetActive(true);
            }
            else if (!shouldShow && phonePromptText.gameObject.activeInHierarchy)
            {
                phonePromptText.gameObject.SetActive(false);
            }
        }
    }

    // Método público para iniciar la llamada
    public void StartPhoneCall()
    {
        if (!isRinging && !isAnswered)
        {
            isRinging = true;
            UnityEngine.Debug.Log("📞 Teléfono sonando...");

            // Iniciar vibración y sonido
            if (vibrationCoroutine != null) StopCoroutine(vibrationCoroutine);
            if (ringCoroutine != null) StopCoroutine(ringCoroutine);

            vibrationCoroutine = StartCoroutine(VibrationRoutine());
            ringCoroutine = StartCoroutine(RingRoutine());
        }
    }

    IEnumerator VibrationRoutine()
    {
        float timer = 0f;

        while (isRinging && timer < ringDuration)
        {
            // Vibración del teléfono
            if (phoneObject != null)
            {
                float x = Mathf.Sin(Time.time * vibrationSpeed) * vibrationIntensity;
                float y = Mathf.Cos(Time.time * vibrationSpeed * 0.8f) * vibrationIntensity;
                phoneObject.transform.localPosition = phoneOriginalPosition + new Vector3(x, y, 0);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Si no se contestó, detener la llamada
        if (isRinging && !isAnswered)
        {
            StopPhoneCall();
        }
    }

    IEnumerator RingRoutine()
    {
        float timer = 0f;

        while (isRinging && timer < ringDuration)
        {
            // Reproducir sonido de llamada
            if (ringSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(ringSound);
            }

            yield return new WaitForSeconds(3f);
            timer += 3f;
        }
    }

    void AnswerPhone()
    {
        isAnswered = true;
        isRinging = false;

        UnityEngine.Debug.Log("✅ Teléfono contestado");

        // Detener vibración y sonido
        if (vibrationCoroutine != null)
        {
            StopCoroutine(vibrationCoroutine);
            vibrationCoroutine = null;
        }

        if (ringCoroutine != null)
        {
            StopCoroutine(ringCoroutine);
            ringCoroutine = null;
        }

        // Restaurar posición del teléfono
        if (phoneObject != null)
            phoneObject.transform.localPosition = phoneOriginalPosition;

        // Reproducir mensaje de voz
        if (voiceMessage != null && audioSource != null)
        {
            audioSource.PlayOneShot(voiceMessage);
        }

        // Mostrar mensaje de texto
        if (phonePromptText != null)
        {
            phonePromptText.text = $"<color=yellow>📞 {voiceMessageText}</color>";
            StartCoroutine(HidePromptAfterDelay(5f));
        }

        UnityEngine.Debug.Log($"📞 Mensaje: {voiceMessageText}");
    }

    void StopPhoneCall()
    {
        isRinging = false;

        UnityEngine.Debug.Log("❌ Llamada perdida");

        // Detener corrutinas
        if (vibrationCoroutine != null) StopCoroutine(vibrationCoroutine);
        if (ringCoroutine != null) StopCoroutine(ringCoroutine);

        // Restaurar posición del teléfono
        if (phoneObject != null)
            phoneObject.transform.localPosition = phoneOriginalPosition;

        // Ocultar prompt
        if (phonePromptText != null)
            phonePromptText.gameObject.SetActive(false);
    }

    IEnumerator HidePromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (phonePromptText != null)
            phonePromptText.gameObject.SetActive(false);
    }

    // Método para resetear el sistema
    public void ResetPhone()
    {
        isRinging = false;
        isAnswered = false;
        playerInRange = false;

        if (vibrationCoroutine != null) StopCoroutine(vibrationCoroutine);
        if (ringCoroutine != null) StopCoroutine(ringCoroutine);

        if (phoneObject != null)
            phoneObject.transform.localPosition = phoneOriginalPosition;

        if (phonePromptText != null)
            phonePromptText.gameObject.SetActive(false);
    }

    // ✅ QUITÉ EL OnGUI() PARA ELIMINAR LOS BOTONES NEGROS

    // Visualizar el área de interacción en el Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
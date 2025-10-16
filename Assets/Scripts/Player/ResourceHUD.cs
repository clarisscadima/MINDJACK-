using TMPro;
using UnityEngine;

public class ResourceHUD : MonoBehaviour
{
    [Header("Panel Principal")]
    public GameObject hudPanel;
    public bool siempreVisible = true;

    [Header("Elementos del HUD")]
    public TextMeshProUGUI saludText;
    public TextMeshProUGUI corduraText;
    public TextMeshProUGUI fichasText;
    public UnityEngine.UI.Image saludBar;
    public UnityEngine.UI.Image corduraBar;

    [Header("Efectos Visuales")]
    public GameObject lowHealthEffect;
    public GameObject lowSanityEffect;

    private PlayerResources playerResources;

    void Start()
    {
        playerResources = FindObjectOfType<PlayerResources>();

        if (playerResources != null)
        {
            // Conectar eventos
            playerResources.OnSaludChanged += OnSaludChanged;
            playerResources.OnCorduraChanged += OnCorduraChanged;
            playerResources.OnFichasChanged += OnFichasChanged;
        }

        // Configurar visibilidad
        if (hudPanel != null)
            hudPanel.SetActive(siempreVisible);

        Debug.Log("✅ HUD de recursos inicializado");
    }

    void OnSaludChanged(float nuevaSalud)
    {
        UpdateHUD();

        // Efectos de salud baja
        if (lowHealthEffect != null)
        {
            lowHealthEffect.SetActive(nuevaSalud < 30f);
        }
    }

    void OnCorduraChanged(float nuevaCordura)
    {
        UpdateHUD();

        // Efectos de cordura baja
        if (lowSanityEffect != null)
        {
            lowSanityEffect.SetActive(nuevaCordura < 25f);
        }
    }

    void OnFichasChanged(int nuevasFichas)
    {
        UpdateHUD();
    }

    void UpdateHUD()
    {
        if (playerResources == null) return;

        // Actualizar textos con colores y formatos
        if (saludText != null)
        {
            float salud = playerResources.GetSalud();
            string color = salud > 50 ? "#FF6B6B" : (salud > 20 ? "#FFA500" : "#FF0000");
            saludText.text = $"<color={color}>❤️ {salud:F0}/{playerResources.saludMaxima}</color>";
        }

        if (corduraText != null)
        {
            float cordura = playerResources.GetCordura();
            string color = cordura > 60 ? "#4ECDC4" : (cordura > 30 ? "#45B7D1" : "#96CEB4");
            corduraText.text = $"<color={color}>🧠 {cordura:F0}/{playerResources.corduraMaxima}</color>";
        }

        if (fichasText != null)
        {
            fichasText.text = $"<color=#FFD93D>💰 {playerResources.GetFichas()}</color>";
        }

        // Actualizar barras
        if (saludBar != null)
            saludBar.fillAmount = playerResources.GetSaludPorcentaje();

        if (corduraBar != null)
            corduraBar.fillAmount = playerResources.GetCorduraPorcentaje();
    }

    // Toggle visibilidad del HUD (útil para cutscenes)
    public void ToggleHUD(bool visible)
    {
        if (hudPanel != null)
            hudPanel.SetActive(visible);
    }

    void Update()
    {
        // Tecla para toggle rápido del HUD (opcional)
        if (Input.GetKeyDown(KeyCode.H))
        {
            siempreVisible = !siempreVisible;
            ToggleHUD(siempreVisible);
        }
    }

    void OnDestroy()
    {
        // Limpiar eventos
        if (playerResources != null)
        {
            playerResources.OnSaludChanged -= OnSaludChanged;
            playerResources.OnCorduraChanged -= OnCorduraChanged;
            playerResources.OnFichasChanged -= OnFichasChanged;
        }
    }
}
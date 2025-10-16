using TMPro;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    [Header("Recursos del Jugador")]
    public float saludMaxima = 100f;
    public float corduraMaxima = 100f;

    [SerializeField] private float saludActual;
    [SerializeField] private float corduraActual;
    [SerializeField] private int fichasActuales;

    [Header("Referencias UI")]
    public TextMeshProUGUI saludText;
    public TextMeshProUGUI corduraText;
    public TextMeshProUGUI fichasText;
    public UnityEngine.UI.Image saludBar;
    public UnityEngine.UI.Image corduraBar;

    // Eventos para cuando los recursos cambien
    public System.Action<float> OnSaludChanged;
    public System.Action<float> OnCorduraChanged;
    public System.Action<int> OnFichasChanged;

    void Start()
    {
        // Inicializar recursos
        saludActual = saludMaxima;
        corduraActual = corduraMaxima;
        fichasActuales = 0;

        // Actualizar UI inicial
        UpdateUI();

        Debug.Log("✅ Sistema de recursos del jugador inicializado");
    }

    #region MÉTODOS PÚBLICOS PARA MODIFICAR RECURSOS

    public void ModificarSalud(float cantidad)
    {
        saludActual = Mathf.Clamp(saludActual + cantidad, 0, saludMaxima);
        OnSaludChanged?.Invoke(saludActual);
        UpdateUI();

        Debug.Log($"❤️ Salud: {saludActual}/{saludMaxima} ({cantidad:+#;-#})");
    }

    public void ModificarCordura(float cantidad)
    {
        corduraActual = Mathf.Clamp(corduraActual + cantidad, 0, corduraMaxima);
        OnCorduraChanged?.Invoke(corduraActual);
        UpdateUI();

        Debug.Log($"🧠 Cordura: {corduraActual}/{corduraMaxima} ({cantidad:+#;-#})");
    }

    public void ModificarFichas(int cantidad)
    {
        fichasActuales = Mathf.Max(0, fichasActuales + cantidad);
        OnFichasChanged?.Invoke(fichasActuales);
        UpdateUI();

        Debug.Log($"💰 Fichas: {fichasActuales} ({cantidad:+#;-#})");
    }

    #endregion

    #region SISTEMA DE APUESTAS

    public bool ApostarSalud(float cantidad)
    {
        if (saludActual > cantidad)
        {
            ModificarSalud(-cantidad);
            Debug.Log($"🎯 Apostaste {cantidad} de Salud. Recompensas: Armas y curaciones");
            return true;
        }
        return false;
    }

    public bool ApostarCordura(float cantidad)
    {
        if (corduraActual > cantidad)
        {
            ModificarCordura(-cantidad);
            Debug.Log($"🎯 Apostaste {cantidad} de Cordura. Recompensas: Ventajas temporales");
            return true;
        }
        return false;
    }

    public bool ApostarFichas(int cantidad)
    {
        if (fichasActuales >= cantidad)
        {
            ModificarFichas(-cantidad);
            Debug.Log($"🎯 Apostaste {cantidad} Fichas. Recompensas: Pistas y mejoras");
            return true;
        }
        return false;
    }

    #endregion

    #region GETTERS

    public float GetSalud() => saludActual;
    public float GetCordura() => corduraActual;
    public int GetFichas() => fichasActuales;
    public float GetSaludPorcentaje() => saludActual / saludMaxima;
    public float GetCorduraPorcentaje() => corduraActual / corduraMaxima;

    #endregion

    void UpdateUI()
    {
        // Actualizar textos
        if (saludText != null)
            saludText.text = $"<color=#FF6B6B>❤️ {saludActual:F0}/{saludMaxima}</color>";

        if (corduraText != null)
            corduraText.text = $"<color=#4ECDC4>🧠 {corduraActual:F0}/{corduraMaxima}</color>";

        if (fichasText != null)
            fichasText.text = $"<color=#FFD93D>💰 {fichasActuales}</color>";

        // Actualizar barras de progreso
        if (saludBar != null)
            saludBar.fillAmount = GetSaludPorcentaje();

        if (corduraBar != null)
            corduraBar.fillAmount = GetCorduraPorcentaje();
    }

    // Método para debug rápido
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ModificarSalud(-10);
            ModificarCordura(-5);
            ModificarFichas(25);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            ApostarSalud(15);
            ApostarCordura(10);
            ApostarFichas(10);
        }
    }
}
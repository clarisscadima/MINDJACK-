using UnityEngine;

public class MaletaFichas : MonoBehaviour
{
    [Header("Configuración Maleta")]
    public int cantidadFichas = 5;
    public GameObject fichaPrefab; // Prefab separado para las fichas que salen
    public float fuerzaExplosion = 3f;

    [Header("Efectos")]
    public ParticleSystem efectoExplosion;
    public AudioClip sonidoApertura;

    private bool recolectada = false;

    void Start()
    {
        gameObject.tag = "Collectible";

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !recolectada)
        {
            RecolectarMaleta(other.transform);
        }
    }

    void RecolectarMaleta(Transform jugador)
    {
        recolectada = true;
        Debug.Log($"💼 Maleta recolectada! Soltando {cantidadFichas} fichas...");

        PlayerInventory inventario = jugador.GetComponent<PlayerInventory>();
        if (inventario != null)
        {
            inventario.AddCoins(cantidadFichas);
        }

        // Si hay prefab asignado, soltar fichas físicas
        if (fichaPrefab != null)
        {
            SoltarFichas();
        }

        PlayEfectos();
        OcultarMaleta();
        Destroy(gameObject, 3f);
    }

    void SoltarFichas()
    {
        for (int i = 0; i < cantidadFichas; i++)
        {
            Vector3 posicion = transform.position + new Vector3(
                UnityEngine.Random.Range(-0.3f, 0.3f),
                0.3f,
                UnityEngine.Random.Range(-0.3f, 0.3f)
            );

            GameObject ficha = Instantiate(fichaPrefab, posicion, UnityEngine.Random.rotation);

            Rigidbody rbFicha = ficha.GetComponent<Rigidbody>();
            if (rbFicha != null)
            {
                Vector3 direccion = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(0.5f, 1f),
                    UnityEngine.Random.Range(-1f, 1f)
                ).normalized;

                rbFicha.AddForce(direccion * fuerzaExplosion, ForceMode.Impulse);
                rbFicha.AddTorque(UnityEngine.Random.insideUnitSphere * fuerzaExplosion, ForceMode.Impulse);
            }
        }
    }

    void PlayEfectos()
    {
        if (efectoExplosion != null)
        {
            ParticleSystem explosion = Instantiate(efectoExplosion, transform.position, Quaternion.identity);
            explosion.Play();
            Destroy(explosion.gameObject, 2f);
        }

        if (sonidoApertura != null)
        {
            AudioSource.PlayClipAtPoint(sonidoApertura, transform.position);
        }
    }

    void OcultarMaleta()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;

        foreach (Transform child in transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null) childRenderer.enabled = false;
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
    }
}
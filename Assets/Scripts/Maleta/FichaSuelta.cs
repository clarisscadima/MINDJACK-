using UnityEngine;

public class FichaSuelta : MonoBehaviour
{
    [Header("Configuraci�n Ficha")]
    public int valorFicha = 1;
    public float tiempoVida = 2f;
    public float velocidadRotacion = 180f;

    [Header("Efectos")]
    public ParticleSystem efectoDestello;
    public AudioClip sonidoFicha;

    private bool recolectada = false;
    private float tiempoNacimiento;

    void Start()
    {
        tiempoNacimiento = Time.time;
        gameObject.tag = "Collectible";

        // Auto-destrucci�n despu�s del tiempo de vida
        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        // Rotaci�n continua
        transform.Rotate(0, velocidadRotacion * Time.deltaTime, 0);

        // Destello antes de desaparecer
        if (Time.time - tiempoNacimiento > tiempoVida - 0.5f && efectoDestello != null && !efectoDestello.isPlaying)
        {
            efectoDestello.Play();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !recolectada)
        {
            RecolectarFicha(other.GetComponent<PlayerInventory>());
        }
    }

    void RecolectarFicha(PlayerInventory inventario)
    {
        if (inventario != null && !recolectada)
        {
            recolectada = true;
            inventario.AddCoins(valorFicha);

            // Efecto de sonido
            if (sonidoFicha != null)
            {
                AudioSource.PlayClipAtPoint(sonidoFicha, transform.position);
            }

            // Efecto visual
            if (efectoDestello != null)
            {
                efectoDestello.transform.SetParent(null);
                efectoDestello.Play();
                Destroy(efectoDestello.gameObject, 2f);
            }

            Destroy(gameObject);
        }
    }
}
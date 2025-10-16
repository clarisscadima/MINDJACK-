using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName = "Parte";
    public float rotationSpeed = 50f;
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;

    private Vector3 startPos;
    private Rigidbody rb;
    private SphereCollider sphereCollider;

    void Start()
    {
        startPos = transform.position;

        // Configurar tag automáticamente
        gameObject.tag = "Collectible";

        // OBTENER O AGREGAR RIGIDBODY (sin duplicar)
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        // OBTENER O AGREGAR SPHERE COLLIDER (sin duplicar)
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
        }
        sphereCollider.isTrigger = true;
        sphereCollider.radius = 1.2f;

        // Desactivar otros colliders que no sean triggers
        Collider[] allColliders = GetComponents<Collider>();
        foreach (Collider collider in allColliders)
        {
            if (collider != sphereCollider && !collider.isTrigger)
            {
                collider.enabled = false;
            }
        }
    }

    void Update()
    {
        // Animación flotante y rotación
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.AddItem(itemName);
                Destroy(gameObject);
            }
        }
    }
}
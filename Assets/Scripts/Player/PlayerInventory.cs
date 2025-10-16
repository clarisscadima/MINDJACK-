using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public List<string> collectedItems = new List<string>();
    public int coins = 0; // ✅ NUEVO: Sistema de monedas

    [Header("UI References - TextMeshPro")]
    public TextMeshProUGUI inventoryText;
    public TextMeshProUGUI interactionText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI coinsText; // ✅ NUEVO: Texto de monedas

    [Header("Machine Parts Required")]
    public string[] requiredParts = { "Rueda", "Palanca", "Moneda", "Engranaje", "Manivela" };

    [Header("Slot Machine Reference")]
    public GameObject slotMachineUI;

    [Header("Machine Trigger Reference")]
    public GameObject machineTrigger;

    private bool nearMachine = false;
    private bool allPartsCollected = false;

    void Start()
    {
        UnityEngine.Debug.Log("=== INICIANDO PLAYER INVENTORY ===");

        // Configurar estado inicial
        if (inventoryText != null)
        {
            inventoryText.gameObject.SetActive(true);
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }

        if (coinsText != null) // ✅ NUEVO
        {
            coinsText.gameObject.SetActive(true);
        }

        if (slotMachineUI != null)
        {
            slotMachineUI.SetActive(false);
        }

        if (machineTrigger != null)
        {
            machineTrigger.SetActive(false);
        }

        UpdateInventoryUI();
        UpdateCoinsUI(); // ✅ NUEVO
    }

    void Update()
    {
        // Debug temporal - ver estado
        if (Input.GetKeyDown(KeyCode.P))
        {
            DebugCurrentState();
        }

        // Tecla O para activar tragamonedas
        if (Input.GetKeyDown(KeyCode.O) && nearMachine && allPartsCollected)
        {
            UnityEngine.Debug.Log("🎰 Presionando O...");
            ActivateSlotMachine();
        }

        UpdateInteractionText();
        UpdateInstructionText();
    }

    public void AddItem(string itemName)
    {
        string cleanItemName = itemName.Trim();

        if (!collectedItems.Contains(cleanItemName))
        {
            collectedItems.Add(cleanItemName);
            UnityEngine.Debug.Log($"✅ Item recogido: '{cleanItemName}'");
            UpdateInventoryUI();
            CheckIfAllPartsCollected();
        }
    }

    // ✅ NUEVO MÉTODO: Agregar monedas
    public void AddCoins(int amount)
    {
        coins += amount;
        UnityEngine.Debug.Log($"💰 +{amount} monedas! Total: {coins}");
        UpdateCoinsUI();
    }

    // ✅ NUEVO MÉTODO: Gastar monedas
    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            UnityEngine.Debug.Log($"💰 -{amount} monedas! Restantes: {coins}");
            UpdateCoinsUI();
            return true;
        }
        else
        {
            UnityEngine.Debug.Log($"❌ No tienes suficientes monedas. Necesitas: {amount}, Tienes: {coins}");
            return false;
        }
    }

    void UpdateCoinsUI() // ✅ NUEVO MÉTODO
    {
        if (coinsText != null)
        {
            coinsText.text = $"<b>FICHAS: {coins}</b>";
        }
    }

    void CheckIfAllPartsCollected()
    {
        bool hasAllParts = HasAllRequiredParts();

        if (hasAllParts && !allPartsCollected)
        {
            allPartsCollected = true;
            UnityEngine.Debug.Log("🎯 ¡TODAS LAS PARTES RECOGIDAS!");

            // Ocultar inventario
            if (inventoryText != null)
            {
                inventoryText.gameObject.SetActive(false);
            }

            // ✅ ACTIVAR MACHINETRIGGER 
            if (machineTrigger != null)
            {
                machineTrigger.SetActive(true);
                machineTrigger.tag = "Machine";
                UnityEngine.Debug.Log("✅ MachineTrigger ACTIVADO");
            }
            else
            {
                ActivateMachineTrigger();
            }

            // Mostrar instrucción
            if (instructionText != null)
            {
                instructionText.gameObject.SetActive(true);
            }
        }
    }

    void ActivateMachineTrigger()
    {
        UnityEngine.Debug.Log("🔍 Buscando MachineTrigger...");

        GameObject foundTrigger = GameObject.Find("MachineTrigger");
        if (foundTrigger != null)
        {
            UnityEngine.Debug.Log($"✅ Encontrado: {foundTrigger.name}");
            foundTrigger.SetActive(true);
            foundTrigger.tag = "Machine";
            UnityEngine.Debug.Log("🎯 MachineTrigger ACTIVADO");
            return;
        }

        UnityEngine.Debug.LogError("❌ NO SE PUDO ENCONTRAR EL MACHINETRIGGER");
    }

    public bool HasAllRequiredParts()
    {
        foreach (string part in requiredParts)
        {
            if (!collectedItems.Contains(part))
            {
                return false;
            }
        }
        return true;
    }

    void ActivateSlotMachine()
    {
        UnityEngine.Debug.Log($"🎰 Activando - allPartsCollected: {allPartsCollected}, slotMachineUI: {slotMachineUI != null}");

        if (allPartsCollected && slotMachineUI != null)
        {
            slotMachineUI.SetActive(true);
            UnityEngine.Debug.Log("✅ SlotMachineUI ACTIVADO");

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;

            if (inventoryText != null) inventoryText.gameObject.SetActive(false);
            if (interactionText != null) interactionText.gameObject.SetActive(false);
            if (instructionText != null) instructionText.gameObject.SetActive(false);
            if (coinsText != null) coinsText.gameObject.SetActive(false); // ✅ Ocultar monedas durante el juego
        }
        else
        {
            UnityEngine.Debug.LogError("❌ No se puede activar SlotMachineUI");
        }
    }

    public void ExitSlotMachine()
    {
        if (slotMachineUI != null)
        {
            slotMachineUI.SetActive(false);
            UnityEngine.Debug.Log("SlotMachineUI DESACTIVADO");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;

            if (instructionText != null && allPartsCollected)
            {
                instructionText.gameObject.SetActive(true);
            }

            if (coinsText != null) // ✅ Mostrar monedas otra vez
            {
                coinsText.gameObject.SetActive(true);
            }
        }
    }

    void UpdateInventoryUI()
    {
        if (inventoryText != null && inventoryText.gameObject.activeInHierarchy)
        {
            string inventoryContent = "<b>INVENTARIO:</b>\n\n";

            foreach (string item in collectedItems)
            {
                inventoryContent += $"- {item}\n";
            }

            int partsCollected = collectedItems.Count(item => requiredParts.Contains(item));
            inventoryContent += $"\n<b>Partes:</b> {partsCollected}/{requiredParts.Length}";

            inventoryText.text = inventoryContent;
        }
    }

    void UpdateInteractionText()
    {
        if (interactionText != null)
        {
            if (nearMachine && allPartsCollected)
            {
                interactionText.gameObject.SetActive(true);
                interactionText.text = "<color=green>¡PRESIONA O PARA JUGAR!</color>";
            }
            else if (nearMachine)
            {
                interactionText.gameObject.SetActive(true);
                int partsCollected = collectedItems.Count(item => requiredParts.Contains(item));
                interactionText.text = $"<color=yellow>Partes: {partsCollected}/{requiredParts.Length}</color>";
            }
            else
            {
                interactionText.gameObject.SetActive(false);
            }
        }
    }

    void UpdateInstructionText()
    {
        if (instructionText != null)
        {
            if (allPartsCollected && !nearMachine)
            {
                instructionText.gameObject.SetActive(true);
                instructionText.text = "<color=green>¡TODAS LAS PARTES! Ve al tragamonedas</color>";
            }
            else
            {
                instructionText.gameObject.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Machine"))
        {
            nearMachine = true;
            UnityEngine.Debug.Log("🔧 ENTRANDO a máquina");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Machine"))
        {
            nearMachine = false;
            UnityEngine.Debug.Log("👋 SALIENDO de máquina");
        }
    }

    void DebugCurrentState()
    {
        UnityEngine.Debug.Log("=== DEBUG ESTADO ===");
        UnityEngine.Debug.Log($"Items: {collectedItems.Count}");
        UnityEngine.Debug.Log($"Monedas: {coins}");
        UnityEngine.Debug.Log($"All Parts: {allPartsCollected}");
        UnityEngine.Debug.Log($"Near Machine: {nearMachine}");
    }
}
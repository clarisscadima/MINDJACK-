// TemporaryPhoneTester.cs - Elimínalo después de probar
using UnityEngine;

public class TemporaryPhoneTester : MonoBehaviour
{
    public PhoneSystem phoneSystem;

    void Update()
    {
        // Presiona P para probar el teléfono
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (phoneSystem != null)
                phoneSystem.StartPhoneCall();
        }

        // Presiona R para resetear
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (phoneSystem != null)
                phoneSystem.ResetPhone();
        }
    }
}
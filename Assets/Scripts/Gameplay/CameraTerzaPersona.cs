using UnityEngine;
using UnityEngine.InputSystem;

// Camera in terza persona che gira attorno al personaggio.
// A cosa serve: segue il giocatore; si ruota con il mouse o con la levetta destra del pad.
// Il cursore viene bloccato al centro: Esc lo libera, un clic nella finestra di gioco lo riblocca.
// Come montarlo: sulla Main Camera, trascinando il personaggio nel campo "Bersaglio".
public class CameraTerzaPersona : MonoBehaviour
{
    public Transform bersaglio;
    [SerializeField] float distanza = 6f;
    [SerializeField] float altezzaFuoco = 0.8f;
    [SerializeField] float sensibilitaMouse = 0.12f;
    [SerializeField] float sensibilitaPad = 150f;
    [SerializeField] float inclinazioneMinima = -20f;
    [SerializeField] float inclinazioneMassima = 60f;

    InputAction guardaMouse, guardaPad;
    float rotazioneOrizzontale;
    float inclinazione = 20f;

    void Awake()
    {
        guardaMouse = new InputAction("GuardaMouse", InputActionType.Value);
        guardaMouse.AddBinding("<Mouse>/delta");
        guardaPad = new InputAction("GuardaPad", InputActionType.Value);
        guardaPad.AddBinding("<Gamepad>/rightStick");

        if (bersaglio != null) rotazioneOrizzontale = bersaglio.eulerAngles.y;
    }

    void OnEnable()
    {
        guardaMouse.Enable();
        guardaPad.Enable();
        BloccaCursore(true);
    }

    void OnDisable()
    {
        guardaMouse.Disable();
        guardaPad.Disable();
        BloccaCursore(false);
    }

    void OnDestroy()
    {
        guardaMouse.Dispose();
        guardaPad.Dispose();
    }

    void LateUpdate()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) BloccaCursore(false);
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked) BloccaCursore(true);

        if (bersaglio == null) return;

        Vector2 mouse = Cursor.lockState == CursorLockMode.Locked ? guardaMouse.ReadValue<Vector2>() * sensibilitaMouse : Vector2.zero;
        Vector2 pad = guardaPad.ReadValue<Vector2>() * (sensibilitaPad * Time.deltaTime);

        rotazioneOrizzontale += mouse.x + pad.x;
        inclinazione = Mathf.Clamp(inclinazione - (mouse.y + pad.y), inclinazioneMinima, inclinazioneMassima);

        Quaternion rotazione = Quaternion.Euler(inclinazione, rotazioneOrizzontale, 0f);
        Vector3 fuoco = bersaglio.position + Vector3.up * altezzaFuoco;
        transform.SetPositionAndRotation(fuoco - rotazione * Vector3.forward * distanza, rotazione);
    }

    static void BloccaCursore(bool blocca)
    {
        Cursor.lockState = blocca ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !blocca;
    }
}

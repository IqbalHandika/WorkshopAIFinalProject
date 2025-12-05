using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class SkinPanelController : MonoBehaviour {
    [Header("Player")]
    public int playerIndex = 0; // 0 = P1, 1 = P2

    [Header("UI")]
    public Image previewImage;
    public TMP_Text nameText;
    public Button prevBtn;
    public Button nextBtn;
    public Button readyBtn;
    public TMP_Text hintText; // "Waiting..." / "Pick your skin"

    [Header("Gamepad Input")]
    public bool useGamepad = true;
    public bool useController2Map = false; // Set true untuk Player 2 dengan ShanWan gamepad
    
    int localIndex = 0;
    SkinSelectManager M;
    TMP_Text readyLabel;
    bool initialized = false;
    
    // Input tracking
    private ControllerInput inputActions;
    private float lastHorizontalInput = 0f;
    private float inputCooldown = 0.3f;
    private float lastInputTime = 0f;

    void Awake() {
        M = FindAnyObjectByType<SkinSelectManager>();
        if (M == null) {
            Debug.LogError("[SkinPanelController] SkinSelectManager tidak ditemukan di Scene.");
            enabled = false; // hentikan Update
            return;
        }
        if (readyBtn != null)
            readyLabel = readyBtn.GetComponentInChildren<TMP_Text>(true);
            
        // Setup gamepad input
        if (useGamepad) {
            inputActions = new ControllerInput();
            
            // Assign specific gamepad based on player index
            var gamepads = Gamepad.all;
            if (playerIndex < gamepads.Count) {
                if (playerIndex == 0) {
                    inputActions.devices = new[] { gamepads[0] };
                } else {
                    inputActions.devices = new[] { gamepads[playerIndex] };
                }
                Debug.Log($"[SkinPanel P{playerIndex + 1}] Assigned to Gamepad {playerIndex}: {gamepads[playerIndex].name}");
            }
            
            inputActions.Enable();
        }
    }
    
    void OnDestroy() {
        if (inputActions != null) {
            inputActions.Disable();
            inputActions.Dispose();
        }
    }

    void Start() {
        // Validasi UI refs
        if (previewImage == null || prevBtn == null || nextBtn == null || readyBtn == null || hintText == null) {
            Debug.LogError("[SkinPanelController] Ada reference UI yang belum di-assign di Inspector.", this);
            enabled = false;
            return;
        }

        // Validasi library
        if (M.library == null || M.library.shipSprites == null || M.library.shipSprites.Length == 0) {
            Debug.LogError("[SkinPanelController] Skin library kosong / belum diisi di SkinSelectManager.", M);
            enabled = false;
            return;
        }

        // Pastikan arrays di manager valid
        if (M.chosen == null || M.chosen.Length < 2) M.chosen = new int[] { -1, -1 };
        if (M.ready  == null || M.ready.Length  < 2) M.ready  = new bool[] { false, false };

        // Index awal
        localIndex = Mathf.Clamp(M.chosen[playerIndex] >= 0 ? M.chosen[playerIndex] : 0, 0, M.library.shipSprites.Length - 1);
        ApplyVisual(localIndex);

        // Wiring
        prevBtn.onClick.AddListener(() => Step(-1));
        nextBtn.onClick.AddListener(() => Step(+1));
        readyBtn.onClick.AddListener(ReadyUp);

        initialized = true;
        RefreshInteractivity();
    }

    void Update() {
        if (!initialized) return;
        
        // Gamepad input
        if (useGamepad && inputActions != null && playerIndex == M.currentTurn && !M.ready[playerIndex]) {
            HandleGamepadInput();
        }
        
        RefreshInteractivity();
    }
    
    void HandleGamepadInput() {
        // Check button presses based on which map is active
        bool leftPressed = false;
        bool rightPressed = false;
        bool selectPressed = false;
        
        if (useController2Map) {
            leftPressed = inputActions.Controller2.Movement.IsPressed();
            rightPressed = inputActions.Controller2.Movement1.IsPressed();
            selectPressed = inputActions.Controller2.Throttle.triggered;
        } else {
            leftPressed = inputActions.Controller.Movement.IsPressed();
            rightPressed = inputActions.Controller.Movement1.IsPressed();
            selectPressed = inputActions.Controller.Throttle.triggered;
        }
        
        // Handle left/right with cooldown
        if (Time.time - lastInputTime > inputCooldown) {
            if (leftPressed && !rightPressed) {
                Step(-1);
                lastInputTime = Time.time;
            } else if (rightPressed && !leftPressed) {
                Step(+1);
                lastInputTime = Time.time;
            }
        }
        
        // RB untuk select/ready
        if (selectPressed) {
            if (M.chosen[playerIndex] >= 0 && !M.ready[playerIndex]) {
                ReadyUp();
            }
        }
    }

    void Step(int delta) {
        if (!initialized) return;

        var lib = M.library;
        int n = lib.shipSprites.Length;
        int start = localIndex;

        // cari index berikutnya yg tidak diambil pemain lain (kecuali milik kita sendiri)
        for (int i = 0; i < n; i++) {
            int idx = (start + delta + n) % n;
            bool available = !M.IsTaken(idx) || M.chosen[playerIndex] == idx;
            if (available) { localIndex = idx; break; }
            start = idx;
        }

        ApplyVisual(localIndex);

        // simpan pilihan (hanya sukses kalau gilirannya, tapi TIDAK pindah turn)
        bool ok = M.TryPick(playerIndex, localIndex);

        if (hintText) hintText.text = ok ? "Skin selected." : (playerIndex == M.currentTurn ? "Pick a skin." : "Waiting for other player...");
        if (readyBtn) readyBtn.interactable = ok;
        if (!ok && readyLabel) readyLabel.text = "Select";
    }

    void ApplyVisual(int idx) {
        var lib = M.library;
        if (previewImage) previewImage.sprite = lib.shipSprites[idx];

        if (nameText != null) {
            if (lib.skinNames != null && lib.skinNames.Length == lib.shipSprites.Length)
                nameText.text = lib.skinNames[idx];
            else
                nameText.text = $"Skin {idx + 1}";
        }
    }

    void RefreshInteractivity() {
        if (M == null) return;

        bool myTurn = (playerIndex == M.currentTurn);
        if (prevBtn)  prevBtn.interactable  = myTurn && !M.ready[playerIndex];
        if (nextBtn)  nextBtn.interactable  = myTurn && !M.ready[playerIndex];

        if (hintText) hintText.text = myTurn ? "Pick your skin." : "Waiting for other player...";

        bool canSelect = myTurn && M.chosen[playerIndex] >= 0 && !M.ready[playerIndex];
        if (readyBtn) readyBtn.interactable = canSelect;

        if (M.ready[playerIndex]) {
            if (readyBtn) readyBtn.interactable = false;
            if (prevBtn) prevBtn.interactable = false;
            if (nextBtn) nextBtn.interactable = false;
            if (readyLabel) readyLabel.text = "READY";
        } else {
            if (readyLabel) readyLabel.text = canSelect ? "Select" : "Select";
        }
    }

    void ReadyUp() {
        if (!initialized) return;
        M.SetReady(playerIndex);
        if (hintText) hintText.text = "READY";
        if (prevBtn) prevBtn.interactable = false;
        if (nextBtn) nextBtn.interactable = false;
        if (readyBtn) readyBtn.interactable = false;
        if (readyLabel) readyLabel.text = "READY";
    }
}

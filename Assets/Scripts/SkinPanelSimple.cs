using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinPanelSimple : MonoBehaviour {
    [Header("Player")]
    public int playerIndex = 0;                 // 0 = P1, 1 = P2

    [Header("UI Refs")]
    public Image previewImage;                  // Image untuk preview
    public TMP_Text skinNumberText;             // Teks "Skin #"
    public Button prevBtn;
    public Button nextBtn;
    public Button selectBtn;                    // tombol yang akan berubah jadi READY

    SkinSelectManager M;
    TMP_Text selectLabel;                       // label di dalam tombol Select
    int curIndex = 0;

    void Awake() {
        M = FindAnyObjectByType<SkinSelectManager>();
        selectLabel = selectBtn.GetComponentInChildren<TMP_Text>(true);
    }

    void Start() {
        // default index; kalau sebelumnya sudah terset, pakai itu
        curIndex = (M.chosen[playerIndex] >= 0) ? M.chosen[playerIndex] : 0;
        ApplyVisual(curIndex);

        // wiring tombol
        prevBtn.onClick.AddListener(() => Step(-1));
        nextBtn.onClick.AddListener(() => Step(+1));
        selectBtn.onClick.AddListener(SelectCurrent);

        RefreshInteractable();
    }

    void Update() {
        // update siapa yang lagi turn
        RefreshInteractable();
    }

    void Step(int delta) {
        int n = M.shipSprites.Length;
        int probe = (curIndex + delta + n) % n;

        // cari skin berikutnya yang tidak diambil pemain lain
        for (int i = 0; i < n; i++) {
            bool available = !M.IsTaken(probe) || M.chosen[playerIndex] == probe;
            if (available) { curIndex = probe; break; }
            probe = (probe + (delta >= 0 ? 1 : -1) + n) % n;
        }

        ApplyVisual(curIndex);

        // coba set pilihan (akan sukses hanya jika gilirannya)
        bool ok = M.TryPick(playerIndex, curIndex);

        // aktifkan tombol Select jika berhasil memilih di gilirannya
        selectBtn.interactable = ok;
        if (!ok) selectLabel.text = "Select";
    }

    void ApplyVisual(int idx) {
        previewImage.sprite = M.shipSprites[idx];
        skinNumberText.text = $"Skin {idx + 1}";
    }

    void SelectCurrent() {
        // lock pilihan player ini & ubah tombol jadi READY
        M.SetReady(playerIndex);
        selectLabel.text = "READY";
        prevBtn.interactable = false;
        nextBtn.interactable = false;
        selectBtn.interactable = false;
    }

    void RefreshInteractable() {
        bool myTurn = (M.currentTurn == playerIndex);
        // hanya yang gilirannya boleh geser next/prev
        prevBtn.interactable = myTurn && !M.ready[playerIndex];
        nextBtn.interactable = myTurn && !M.ready[playerIndex];

        // tombol Select aktif kalau sudah memilih di gilirannya
        bool canSelect = myTurn && M.chosen[playerIndex] >= 0 && !M.ready[playerIndex];
        selectBtn.interactable = canSelect;

        // kalau panel ini sudah READY, pastikan label tetap “READY”
        if (M.ready[playerIndex]) selectLabel.text = "READY";
        else if (!canSelect)      selectLabel.text = "Select";
    }
}

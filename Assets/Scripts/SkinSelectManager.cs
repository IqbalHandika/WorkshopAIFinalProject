using UnityEngine;
using UnityEngine.SceneManagement;

public class SkinSelectManager : MonoBehaviour {
    public ShipSkinLibrary library;
    public Sprite[] shipSprites;          // drag semua skin ke sini (urut sesuai nomor)
    public string nextSceneName = "SampleScene"; // isi nama scene game kamu

    // state
    public int currentTurn = 0;           // 0 = P1, 1 = P2
    public int[] chosen = { -1, -1 };     // index skin terpilih
    public bool[] ready  = { false, false };

    public bool IsTaken(int idx) {
        return chosen[0] == idx || chosen[1] == idx;
    }

    public bool TryPick(int playerIndex, int idx) {
        if (playerIndex != currentTurn) return false;
        if (IsTaken(idx) && chosen[playerIndex] != idx) return false;
        chosen[playerIndex] = idx;
        return true;
    }

    public void SetReady(int playerIndex) {
        if (chosen[playerIndex] < 0) return;
        ready[playerIndex] = true;

        if (currentTurn == 0) currentTurn = 1;

        if (ready[0] && ready[1]) {
            // Simpan kalau perlu dipakai di scene game
            PlayerPrefs.SetInt("P1_SkinIndex", chosen[0]);
            PlayerPrefs.SetInt("P2_SkinIndex", chosen[1]);
            PlayerPrefs.Save();
            if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
        }
    }
}

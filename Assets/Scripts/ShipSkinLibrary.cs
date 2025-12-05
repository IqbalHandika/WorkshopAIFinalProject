using UnityEngine;

[CreateAssetMenu(menuName = "ShipSkinLibrary")]
public class ShipSkinLibrary : ScriptableObject {
    public Sprite[] shipSprites;   // urutkan sesuai ID (0..N-1)
    public string[] skinNames;     // opsional, biar ada nama di UI (panjang sama)
}

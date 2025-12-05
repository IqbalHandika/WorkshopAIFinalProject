using UnityEngine;

public class ApplyPlayerSkins : MonoBehaviour
{
    [Header("Lib skin yang sama dengan SkinSelector")]
    public ShipSkinLibrary library;

    [Header("Player Ships di scene game")]
    public SpriteRenderer player1Renderer;
    public SpriteRenderer player2Renderer;

    void Start()
    {
        // Ambil pilihan yang disave dari SkinSelector
        int p1Index = PlayerPrefs.GetInt("P1_SkinIndex", 0);
        int p2Index = PlayerPrefs.GetInt("P2_SkinIndex", 1);

        // Safety clamp biar gak keluar array
        p1Index = Mathf.Clamp(p1Index, 0, library.shipSprites.Length - 1);
        p2Index = Mathf.Clamp(p2Index, 0, library.shipSprites.Length - 1);

        // Apply sprite ke kapal yang ada di scene
        if (player1Renderer != null)
            player1Renderer.sprite = library.shipSprites[p1Index];

        if (player2Renderer != null)
            player2Renderer.sprite = library.shipSprites[p2Index];
    }
}

# Setup Guide - Player State UI & Health Bar

## ✅ Scripts yang Sudah Dibuat:

### 1. **Movement.cs** (UPDATED)
- Ditambahkan FSM (Finite State Machine) dengan 3 state:
  - `Idle` - Player diam (speed < threshold)
  - `Moving` - Player bergerak normal
  - `Boosting` - Player sedang boost
- Property baru:
  - `CurrentState` - Get current state
  - `GetStateString()` - Return "IDLE", "MOVING", atau "BOOSTING"
  - `GetStateColor()` - Return color (Gray/Green/Yellow)

### 2. **PlayerStateUI.cs** (NEW)
- Menampilkan state player di UI
- Support TextMeshPro dan Legacy Text
- Optional color indicator dengan Image component

### 3. **PlayerHealth.cs** (NEW)
- System health dengan damage/heal
- Auto-update health bar UI
- Color-coded health bar (Green → Yellow → Red)
- Events untuk integration dengan sistem lain

### 4. **GameUIManager.cs** (NEW)
- Master manager untuk semua UI di game scene
- Manage health bars & state displays untuk P1 dan P2
- Auto-assign references jika dicentang

---

## 🎮 Setup di Unity Editor:

### A. Setup Player GameObject:
1. Pilih **Player 1** GameObject
2. Pastikan ada component:
   - `KeyMovement2D` (Movement.cs)
   - `PlayerHealth` (PlayerHealth.cs)
3. Di Inspector `KeyMovement2D`:
   - Set `Player Name` = "Player 1"
   - Set keys: W/A/S/D + Shift
4. Ulangi untuk **Player 2** dengan keys berbeda (Arrow + RShift)

### B. Setup UI Canvas:

#### Hierarchy Structure:
```
Canvas
├── Player1Panel (Top Left)
│   ├── HealthSlider (Slider)
│   │   ├── Background (Image)
│   │   └── Fill Area
│   │       └── Fill (Image) ← Ini yang berubah warna!
│   ├── HealthText (TextMeshPro) → "100/100"
│   └── StateText (TextMeshPro) → "P1: IDLE"
│
└── Player2Panel (Top Right)
    ├── HealthSlider (Slider)
    │   ├── Background (Image)
    │   └── Fill Area
    │       └── Fill (Image) ← Ini yang berubah warna!
    ├── HealthText (TextMeshPro) → "100/100"
    └── StateText (TextMeshPro) → "P2: IDLE"
```

#### Setup Steps:

**1. Buat Canvas:**
   - Right-click Hierarchy → UI → Canvas
   - Canvas Scaler → UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080

**2. Player 1 Panel (Top Left):**
   - Right-click Canvas → Create Empty → Rename "Player1Panel"
   - Rect Transform:
     - Anchor: Top-Left
     - Pos X: 20, Pos Y: -20
     - Width: 300, Height: 100

**3. Health Bar (SLIDER):**
   - Right-click Player1Panel → UI → Slider
   - Rename "HealthSlider"
   - Slider Component:
     - Min Value: 0
     - Max Value: 1
     - Value: 1
     - Interactable: OFF (agar tidak bisa diklik)
   - Delete "Handle Slide Area" (tidak perlu handle)
   - Pilih child "Fill Area" → "Fill" → Ini adalah Image yang akan berubah warna
   - Simpan reference ke Fill Image ini untuk coloring

**4. Health Text:**
   - Right-click Player1Panel → UI → Text - TextMeshPro
   - Text: "100/100"
   - Font Size: 20
   - Position di bawah/samping slider

**5. State Text:**
   - Right-click Player1Panel → UI → Text - TextMeshPro
   - Text: "P1: IDLE"
   - Font Size: 24, Bold
   - Position di bawah Health Text

**6. Duplicate untuk Player 2:**
   - Duplicate Player1Panel → Rename "Player2Panel"
   - Anchor: Top-Right
   - Pos X: -20, Pos Y: -20

**7. Setup GameUIManager:**
   - Create Empty GameObject → Rename "GameUIManager"
   - Add Component → GameUIManager.cs
   - Drag references di Inspector:
     - Player 1 Health → Player1 GameObject (yang punya PlayerHealth component)
     - Player 1 Movement → Player1 GameObject (yang punya KeyMovement2D)
     - Player 1 Health Bar Slider → HealthSlider GameObject
     - Player 1 Slider Fill → Fill Image (child dari HealthSlider)
     - Player 1 Health Text → HealthText TextMeshPro
     - Player 1 State Text → StateText TextMeshPro
     - (Sama untuk Player 2)
   - **Settings:**
     - ✅ Auto Assign References = TRUE
     - ✅ Use White Text With Outline = TRUE (biar keliatan di background hijau!)
     - Text Outline Color = Black
     - Text Outline Width = 0.2

---

## 🎨 Styling Tips:

### Health Bar Slider Settings:
- **Min Value**: 0
- **Max Value**: 1 (karena menggunakan percentage 0-1)
- **Whole Numbers**: OFF
- **Interactable**: OFF (biar tidak bisa diklik)
- **Direction**: Left To Right
- **Delete Handle**: Tidak perlu handle untuk health bar

### Health Bar Colors:
- Healthy (>50%): Green `#00FF00`
- Mid Health (25-50%): Yellow `#FFFF00`
- Low Health (<25%): Red `#FF0000`

### State Colors (Optional - pakai Image indicator):
- Idle: Gray
- Moving: Green
- Boosting: Yellow

### Text Styling (PENTING untuk background hijau!):
**Auto Styling via GameUIManager:**
- `useWhiteTextWithOutline` = ✅ TRUE (default)
- Text Color: **White** (putih)
- Outline Color: **Black** (hitam)
- Outline Width: **0.2** (bisa disesuaikan)
- Font Style: **Bold**

**Manual Styling di TextMeshPro:**
1. Select Text GameObject
2. TextMeshProUGUI Component:
   - Vertex Color: White `#FFFFFF`
   - Font Style: **Bold**
   - Font Size: 20-28 (sesuai kebutuhan)
3. Scroll ke **Outline**:
   - Outline Color: Black `#000000`
   - Outline Thickness: **0.2** (atau 0.15-0.3 sesuai selera)

### Alternative Styling (jika outline kurang tebal):
**Shadow + Outline:**
1. Pakai outline seperti di atas
2. Tambah **Shadow** component (Right-click text → UI → Effects → Shadow)
3. Shadow:
   - Effect Color: Black
   - Effect Distance: (2, -2)
   - Use Graphic Alpha: OFF

### Font Recommendation:
- **Font**: Arial Bold, Roboto Bold, atau font tebal lainnya
- **Health Text**: Size 18-22, Bold
- **State Text**: Size 24-32, Bold + Outline
- **CPU State**: Size 20-24, Bold + Outline

---

## 🖼️ Text Outline Visual Example:

### Tanpa Outline (SUSAH DILIHAT di background hijau):
```
P1: MOVING  ← Text hijau di background hijau = tidak keliatan!
```

### Dengan Outline (JELAS TERLIHAT):
```
⬛⬛⬛⬛⬛⬛⬛
⬛⬜⬜⬜⬜⬛  ← Outline hitam bikin text putih keliatan jelas
⬛⬛⬛⬛⬛⬛⬛
P1: MOVING (White text + Black outline)
```

### Settings yang Recommended:
- **Text Color**: White `#FFFFFF`
- **Outline Color**: Black `#000000` 
- **Outline Width**: 
  - 0.15 = Tipis (cukup untuk font besar)
  - 0.2 = Standard (recommended)
  - 0.3 = Tebal (untuk font kecil)
- **Font Style**: Bold

### Quick Comparison:
| Setting | Visibility on Green BG |
|---------|----------------------|
| Green text, no outline | ❌ Jelek |
| White text, no outline | ⚠️ Lumayan |
| **White text + Black outline** | ✅ **PERFECT!** |
| White text + Black outline + Shadow | ✅✅ Extra clear! |

---

## 🧪 Testing:

### Quick Test - State Text Outline:
**JIKA STATE TEXT TIDAK ADA OUTLINE, LAKUKAN INI:**
1. **STOP Play Mode** (jika sedang play)
2. Select StateText GameObject di Hierarchy
3. Add Component → **TextOutlineHelper**
4. Settings akan auto default (putih + outline hitam)
5. Play mode → Outline langsung muncul!
6. ✅ Selesai!

**Atau pakai Context Menu (lebih cepat):**
1. Select StateText GameObject
2. Add Component → TextOutlineHelper
3. Klik kanan component → **"Apply Outline Now"**
4. Done! (tanpa perlu play)

### Test di Play Mode:
1. Play game
2. Tekan W/A/S/D → State berubah "MOVING" 
3. Tekan Shift → State berubah "BOOSTING"
4. Lepas semua → State "IDLE"
5. **CEK: Text putih dengan outline hitam tebal? ✅**

### Test Damage (di Console/Script):
```csharp
// Di GameUIManager atau script lain
FindObjectOfType<PlayerHealth>().TakeDamage(20f);
```

---

## 📝 Next Steps untuk Tugas Akhir:

- [x] FSM - Player State (DONE)
- [x] Health System (DONE)
- [ ] Shooting System dengan Probability Critical Hit
- [ ] Cooperative FSM (2 player coordination)
- [ ] Enemy AI dengan A* dan Dijkstra

---

## 🐛 Troubleshooting:

**UI tidak muncul:**
- Cek Canvas Render Mode = Screen Space - Overlay
- Cek EventSystem ada di scene

**State tidak update:**
- Cek reference GameUIManager sudah terisi
- Cek KeyMovement2D ada di player

**Health bar tidak berubah:**
- Cek Slider Min Value = 0, Max Value = 1
- Cek Slider.value sudah terisi (default 1)
- Cek reference healthBarSlider dan sliderFillImage sudah terisi di PlayerHealth
- Cek Interactable di Slider = OFF (biar tidak bisa diklik user)

**Text tidak keliatan di background hijau:**

**SOLUSI 1: Via GameUIManager (Otomatis)**
1. Pastikan StateText sudah di-drag ke GameUIManager Inspector
2. Cek GameUIManager → Use White Text With Outline = TRUE
3. Cek Text Outline Width = minimal 0.2 (naikan ke 0.3 kalau masih kurang)
4. Play game, lalu cek Console untuk log "Outline setup untuk: ..."
5. Kalau ada warning "null", berarti reference belum di-drag!

**SOLUSI 2: Via TextOutlineHelper (Manual per Text)**
1. Select StateText GameObject di Hierarchy
2. Add Component → TextOutlineHelper
3. Cek settings:
   - Apply On Start = TRUE
   - Text Color = White
   - Outline Color = Black
   - Outline Width = 0.2 - 0.3
   - Make Bold = TRUE
4. Play mode → Outline akan auto-apply
5. Atau klik kanan component → "Apply Outline Now" (tanpa play)

**SOLUSI 3: Manual di TextMeshPro Inspector**
1. Select StateText GameObject
2. Di TextMeshProUGUI component:
   - Vertex Color = White #FFFFFF
   - Font Style = Bold
3. Scroll ke **Extra Settings** → Outline:
   - Outline Color = Black #000000
   - Thickness = 0.2 (atau lebih)
4. Kalau masih tidak keliatan:
   - Tambah "Underlay" atau "Drop Shadow"
   - Settings sama seperti outline

**SOLUSI 4: Force Reapply (Debug)**
1. Play mode
2. Select GameUIManager di Hierarchy
3. Klik kanan GameUIManager component
4. Pilih "Force Reapply Text Outlines"
5. Cek Console untuk konfirmasi

**Kalau MASIH tidak keliatan:**
- Cek material TextMeshPro tidak corrupted
- Coba ganti font ke font lain (Arial, Roboto)
- Pastikan Canvas dalam mode "Screen Space - Overlay"
- Coba naikin outline width sampai 0.5

---

Made with ❤️ for AI for Game Final Project

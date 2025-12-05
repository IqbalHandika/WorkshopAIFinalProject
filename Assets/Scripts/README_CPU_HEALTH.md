# 🏥 CPU Health Bar System

## 📋 Overview
System health bar floating untuk CPU yang muncul **di atas kapal** (world space), berbeda dengan player yang health barnya di corner UI.

**Features:**
- ✅ Floating health bar di atas kapal CPU
- ✅ Menampilkan angka health (misal: 85/100)
- ✅ Color-coded: Green → Yellow → Red
- ✅ Auto-follow CPU movement
- ✅ Selalu menghadap camera (billboard effect)
- ✅ Auto-created via script (tidak perlu manual setup UI!)

## 🎮 Setup di Unity

### Quick Setup (Auto-Create UI)
Cara paling mudah - script akan auto-create semua UI!

1. **Pilih GameObject CPU** (CPU1 atau CPU2)
2. **Add Component** → `CPUHealthBar`
3. **Setup basic settings**:
   ```
   ├─ Max Health: 100
   ├─ Health Bar Offset: (0, 1.5, 0)  // Di atas kapal
   ├─ Canvas Size: (2, 0.5)
   └─ CPU Name: "CPU 1 (Dijkstra)"
   ```
4. **Done!** Health bar akan auto-create saat play mode ✅

### Manual Setup (Advanced)
Jika ingin custom UI secara manual:

1. **Create Canvas** (Child of CPU GameObject)
   ```
   - Render Mode: World Space
   - Width: 200
   - Height: 50
   - Position: (0, 1.5, 0) dari CPU
   ```

2. **Create Panel** (Child of Canvas)
   ```
   - Add Image (background)
   - Color: Black, Alpha: 0.5
   ```

3. **Create Slider** (Child of Panel)
   ```
   - Min Value: 0
   - Max Value: 1
   - Value: 1
   - Fill Rect: Assign Fill Image
   ```

4. **Create TextMeshPro** (Child of Panel)
   ```
   - Text: "100/100"
   - Alignment: Center
   - Font Size: 24
   - Color: White
   - Outline: Black, Width: 0.2
   ```

5. **Assign di CPUHealthBar component**:
   ```
   ├─ World Space Canvas: (drag Canvas)
   ├─ Health Bar Slider: (drag Slider)
   ├─ Slider Fill Image: (drag Fill Image)
   └─ Health Text: (drag TextMeshPro)
   ```

## 🎯 Component Settings

### Health Settings
```csharp
Max Health: 100f                 // Total health CPU
```

### Position & Size
```csharp
Health Bar Offset: (0, 1.5, 0)  // Offset dari posisi kapal
Canvas Size: (1, 0.3)            // Ukuran visual (tidak dipakai di auto-create)
Canvas Scale: 0.01               // Scale multiplier (0.001 - 0.1)
                                 // Smaller = lebih kecil health bar
```

**Tips Ukuran:**
- Default `canvasScale = 0.01` → Health bar normal size
- Terlalu besar? → `canvasScale = 0.005` (lebih kecil)
- Terlalu kecil? → `canvasScale = 0.015` (lebih besar)

### Color Thresholds
```csharp
Healthy Color: Green             // > 50% health
Mid Health Color: Yellow         // 25% - 50% health
Low Health Color: Red            // < 25% health

Mid Health Threshold: 0.5        // 50%
Low Health Threshold: 0.25       // 25%
```

### CPU Info
```csharp
CPU Name: "CPU 1 (Dijkstra)"     // Nama untuk debug log
```

## 📊 Visual Examples

### Health Bar States

**100% Health (Green)**
```
┌────────────────────┐
│     100/100        │ ← White text
├────────────────────┤
│████████████████████│ ← Green fill
└────────────────────┘
```

**50% Health (Yellow)**
```
┌────────────────────┐
│      50/100        │
├────────────────────┤
│██████████░░░░░░░░░░│ ← Yellow fill
└────────────────────┘
```

**20% Health (Red)**
```
┌────────────────────┐
│      20/100        │
├────────────────────┤
│████░░░░░░░░░░░░░░░░│ ← Red fill
└────────────────────┘
```

## 💻 Code Usage

### Dari Script Lain (Apply Damage)
```csharp
// Get CPU health component
CPUHealthBar cpu = GetComponent<CPUHealthBar>();

// Take damage
cpu.TakeDamage(25f);

// Heal
cpu.Heal(10f);

// Set health directly
cpu.SetHealth(50f);

// Check health
float currentHP = cpu.GetCurrentHealth();
float maxHP = cpu.GetMaxHealth();
float percent = cpu.GetHealthPercent();
bool alive = cpu.IsAlive();
```

### Integration dengan Cannonball
Script `Cannonball.cs` sudah di-update untuk detect CPU:
```csharp
CPUHealthBar cpuHealth = hit.GetComponent<CPUHealthBar>();
if (cpuHealth != null) {
    cpuHealth.TakeDamage(damage);
    // Spawn effects, floating text, etc
}
```

## 🎨 Customization

### Change Health Bar Position
```csharp
// Di Inspector atau code
healthBarOffset = new Vector3(0f, 2f, 0f);  // Lebih tinggi
healthBarOffset = new Vector3(0f, 1f, 0f);  // Lebih rendah
```

### Change Health Bar Size
```csharp
// Adjust canvas scale (Inspector or code)
canvasScale = 0.015f;  // Lebih besar
canvasScale = 0.01f;   // Default
canvasScale = 0.005f;  // Lebih kecil

// Atau manual scale di code
worldSpaceCanvas.transform.localScale = Vector3.one * 0.01f;
```

### Custom Colors
```csharp
healthyColor = Color.cyan;
midHealthColor = new Color(1f, 0.5f, 0f); // Orange
lowHealthColor = Color.magenta;
```

### Change Thresholds
```csharp
midHealthThreshold = 0.6f;   // Yellow di 60%
lowHealthThreshold = 0.3f;   // Red di 30%
```

## 🔧 Technical Details

### Billboard Effect
Health bar selalu menghadap camera (LateUpdate):
```csharp
void LateUpdate() {
    // Follow CPU position
    canvas.transform.position = transform.position + offset;
    
    // Face camera
    canvas.transform.rotation = Quaternion.LookRotation(
        canvas.transform.position - camera.transform.position
    );
}
```

### World Space Canvas
- **Render Mode**: World Space
- **Dynamic Pixels Per Unit**: 10
- **Position**: Follows CPU + offset
- **Rotation**: Billboard (faces camera)

### Layer Recommendations
Untuk avoid z-fighting dan sorting issues:
```
Canvas Sorting Layer: "UI" atau "Foreground"
Canvas Order in Layer: 10
```

## 🐛 Troubleshooting

### Health Bar Tidak Muncul
**Cek:**
- ✅ Script `CPUHealthBar` attached ke CPU GameObject
- ✅ Play mode (auto-create berjalan di Awake)
- ✅ Console log: "[CPU 1] Auto-created floating health bar UI"
- ✅ Main Camera tagged as "MainCamera"

### Health Bar Tidak Update Posisi
**Cek:**
- ✅ `mainCamera` reference di Inspector (atau auto-assign di Awake)
- ✅ `worldSpaceCanvas` reference tidak null
- ✅ LateUpdate berjalan (tidak ada error)

### Health Bar Tidak Berubah Warna
**Cek:**
- ✅ `sliderFillImage` assigned
- ✅ Thresholds di-set dengan benar (0.25 dan 0.5)
- ✅ Health value berubah (panggil `TakeDamage()`)

### Angka Health Tidak Muncul
**Cek:**
- ✅ `healthText` assigned (TextMeshProUGUI)
- ✅ Font TMP imported (default font biasanya OK)
- ✅ Text color bukan transparent
- ✅ Canvas sorting order > background

### Health Bar Terlalu Besar/Kecil
**Solusi:**
```csharp
// OPTION 1: Adjust di Inspector (RECOMMENDED)
Canvas Scale: 0.01   // Default
Canvas Scale: 0.005  // Lebih kecil (jika terlalu gede)
Canvas Scale: 0.015  // Lebih besar (jika terlalu kecil)

// OPTION 2: Adjust di code
worldSpaceCanvas.transform.localScale = Vector3.one * 0.01f;

// Range slider di Inspector: 0.001 - 0.1
```

### Health Bar Tidak Menghadap Camera
**Cek:**
- ✅ `mainCamera` reference valid
- ✅ Camera tagged "MainCamera"
- ✅ LateUpdate tidak di-override

## 🎯 Integration dengan Game Systems

### CPU AI + Health Bar
```
CPU1 (GameObject)
├─ AutoMovement           // AI pathfinding
├─ CPUHealthBar          // Health system ✅
├─ ShootingSystem        // Shooting (TODO)
└─ Collider2D            // Hit detection
```

### Player vs CPU
| Feature | Player | CPU |
|---------|--------|-----|
| **Health Script** | `PlayerHealth` | `CPUHealthBar` |
| **UI Location** | Screen corner (UI Canvas) | Floating (World Space) |
| **Health Display** | Slider + Text | Slider + Text |
| **Always Visible** | ✅ Yes | ✅ Yes (billboard) |
| **Color Coded** | ✅ Yes | ✅ Yes |

### Damage Flow
```
ShootingSystem (Player/CPU)
    ↓
Spawns Cannonball
    ↓
Cannonball.CheckHit()
    ↓
Detects CPU collider
    ↓
CPUHealthBar.TakeDamage()
    ↓
Update health bar UI + floating damage text
```

## 📝 Testing Checklist

Setup:
- [ ] CPUHealthBar component added to CPU1
- [ ] CPUHealthBar component added to CPU2
- [ ] Health bar muncul di atas kapal saat play mode
- [ ] Health bar mengikuti gerakan CPU

Functionality:
- [ ] Test damage: Health bar berkurang
- [ ] Test heal: Health bar bertambah
- [ ] Color change: Green → Yellow → Red
- [ ] Health text update: "85/100" → "60/100" → "25/100"
- [ ] Billboard effect: Health bar selalu menghadap camera

Integration:
- [ ] Cannonball hit CPU: Damage applied
- [ ] Floating damage text muncul saat hit
- [ ] CPU death: Health bar hidden

## 🎓 Comparison: Player vs CPU Health

### PlayerHealth.cs (Screen UI)
```
- Health bar di corner screen
- Part of GameUIManager
- UI Canvas (Screen Space - Overlay)
- Fixed position di screen
```

### CPUHealthBar.cs (World Space)
```
- Health bar di atas kapal
- Independent per CPU
- World Space Canvas
- Follows CPU movement
- Billboard effect (face camera)
```

## 🚀 Next Steps

1. **Test di Scene**:
   - Add CPUHealthBar ke CPU1 dan CPU2
   - Play mode dan cek health bar muncul
   - Test shoot CPU dengan player

2. **Customize Visual**:
   - Adjust colors sesuai theme game
   - Adjust position/size sesuai kebutuhan

3. **Add CPU Shooting**:
   - CPU bisa nembak player
   - Player health bar update saat kena hit dari CPU

4. **Polish**:
   - Add hit animation (shake, flash)
   - Add death effect (explosion, sink animation)
   - Add sound effects

## 📚 Related Documentation
- `README_SHOOTING.md` - Shooting system
- `README_SETUP_UI.md` - Player UI setup
- `README_WAYPOINT.md` - CPU pathfinding
- `README_PATHFINDING.md` - Dijkstra vs A*

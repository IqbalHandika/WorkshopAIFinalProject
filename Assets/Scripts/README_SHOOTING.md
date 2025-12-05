# Setup Guide - Shooting System dengan Probability Critical Hit

## ✅ Scripts yang Sudah Dibuat:

### 1. **ShootingSystem.cs** 
- Sistem shooting dengan probability-based critical hit
- Mouse aim untuk testing (Player 1)
- Siap untuk joystick analog kanan (nanti)
- Fire rate cooldown
- Visual feedback (aim line)

### 2. **Cannonball.cs**
- Projectile movement
- Collision detection
- Damage system
- Visual effects (critical vs normal)
- **Instant destroy on hit** (tidak mendorong kapal)
- **Ignore collision dengan owner** (tidak kena diri sendiri)

---

## 🎮 Setup di Unity Editor:

### A. Setup Cannonball Prefab:

1. **Buat Cannonball Prefab:**
   ```
   - Buat GameObject baru → Rename "Cannonball"
   - Add Component → SpriteRenderer
     - Sprite: Drag cannonball sprite
   - Add Component → Cannonball.cs
   ```

   **⚠️ PENTING:**
   - ✅ TIDAK PERLU Rigidbody2D
   - ✅ TIDAK PERLU CircleCollider2D
   - Manual movement pakai transform
   - Manual hit detection pakai OverlapCircle
   - **Sesimpel itu!**

2. **Optional - Add Trail:**
   ```
   - Add Component → TrailRenderer
   - Width: 0.1 - 0.3
   - Time: 0.5
   - Material: Default Sprite
   ```

3. **Simpan sebagai Prefab:**
   ```
   - Drag Cannonball ke folder Prefabs
   - Delete dari scene
   ```

### A2. Setup Floating Damage Text Prefab:

1. **Buat Floating Text GameObject:**
   ```
   - Create Empty → Rename "FloatingDamageText"
   - Add Component → TextMeshPro - Text (NOT UI!)
   - Add Component → FloatingDamageText.cs
   ```

2. **Setup TextMeshPro:**
   ```
   TextMeshPro Component:
   - Text: "99" (placeholder)
   - Font Size: 4
   - Alignment: Center + Middle
   - Color: White
   - Sorting Layer: Same as UI (paling depan)
   - Order in Layer: 100
   
   Extra Settings → Outline:
   - Outline Color: Black
   - Thickness: 0.3
   ```

3. **Setup FloatingDamageText.cs:**
   ```
   Animation:
   - Float Speed: 2
   - Fade Speed: 1
   - Lifetime: 1.5 seconds
   - Animate Scale: TRUE
   ```

4. **Simpan sebagai Prefab:**
   ```
   - Drag FloatingDamageText ke folder Prefabs
   - Delete dari scene
   ```

5. **Assign ke Cannonball Prefab:**
   ```
   - Select Cannonball Prefab
   - Cannonball.cs Component:
     - Damage Text Prefab: Drag FloatingDamageText prefab
     - Normal Damage Color: White (R:255 G:255 B:255)
     - Critical Damage Color: Red (R:255 G:0 B:0)
   ```

**⚠️ CHECKLIST PENTING:**
```
FloatingDamageText Prefab:
✅ Ada TextMeshPro component (bukan TextMeshProUGUI!)
✅ Ada FloatingDamageText.cs component
✅ TextMeshPro.textMesh di Inspector terisi (auto-assign)
✅ Text bisa apa aja ("99", "DMG", etc) - akan di-override
✅ Font Asset assigned
✅ Sorting Layer paling depan

Cannonball Prefab:
✅ Damage Text Prefab assigned
✅ Normal Damage Color = White
✅ Critical Damage Color = Red
✅ Hit Layers includes "Player" atau "Enemy"
```

### B. Setup Player Cannon:

1. **Setup Cannon Child Object:**
   ```
   Player1
   ├── SpriteRenderer (kapal)
   ├── Rigidbody2D
   ├── Collider
   ├── KeyMovement2D
   ├── PlayerHealth
   ├── ShootingSystem ← ADD INI!
   └── Cannon (Child) ← Pastikan ada!
       └── FirePoint (Child) ← Auto-created atau buat manual
   ```

2. **Setup Cannon GameObject:**
   ```
   - Select Cannon child object
   - Position: (0, 0.5, 0) atau sesuaikan
   - Rotation: (0, 0, 0)
   - Add SpriteRenderer dengan sprite cannon
   ```

3. **Setup FirePoint:**
   ```
   - Buat child baru di Cannon → Rename "FirePoint"
   - Position: (0, 0.5, 0) ← Ujung cannon
   - Ini adalah spawn point cannonball
   ```

### C. Setup ShootingSystem Component:

1. **Select Player1 GameObject**

2. **Add Component → ShootingSystem**

3. **Assign References:**
   ```
   Cannon References:
   - Cannon Transform: Drag "Cannon" child object
   - Fire Point: Drag "FirePoint" child object
   
   Cannonball:
   - Cannonball Prefab: Drag cannonball prefab dari folder
   - Cannonball Speed: 15
   - Cannonball Lifetime: 5
   
   ⚠️ PENTING - Setup di Cannonball Prefab:
   - Hit Radius: 0.3 (jarak detect hit)
   - Hit Layers: Set ke "Player", "Enemy", etc
     (Edit → Project Settings → Tags and Layers → Layers)
   
   Shooting Settings:
   - Fire Rate: 1 (1 shot per second)
   
   Damage Settings:
   - Damage: 20 (base damage) ← EDIT DI SINI!
   
   Aim Control (Testing):
   - Use Mouse Aim: ✅ TRUE (untuk testing)
   - Fire Key: Mouse0 (left click)
   
   ━━━━━ PROBABILITY: CRITICAL HIT ━━━━━
   - Critical Chance: 0.25 (slider 0-1) ← EDIT CHANCE!
     * 0.10 = 10% chance
     * 0.25 = 25% chance (default)
     * 0.50 = 50% chance
     * 1.00 = 100% (selalu critical)
   
   - Critical Multiplier: 2.0 (slider 1-5) ← EDIT MULTIPLIER!
     * 1.5 = 1.5x damage
     * 2.0 = 2x damage (default)
     * 3.0 = 3x damage
     * 5.0 = 5x damage (one-shot!)
   
   Critical Visual Settings:
   - Critical Color: Red
   - Normal Color: White
   
   Visual Feedback:
   - Show Aim Line: TRUE (optional)
   - Aim Line: (biarkan null atau add LineRenderer)
   ```

### D. Setup LineRenderer (Optional - untuk show aim):

1. **Select Player1 GameObject**
2. **Add Component → LineRenderer**
3. **Settings:**
   ```
   - Width: 0.05
   - Color: Yellow → Transparent
   - Material: Default Sprite
   - Positions: 2
   ```
4. **Drag ke ShootingSystem → Aim Line**

---

## 🚫 TANPA COLLIDER = TANPA PUSH!

### Cannonball System (SUPER SIMPLE):

**TIDAK ADA:**
- ❌ Rigidbody2D
- ❌ CircleCollider2D
- ❌ Physics collision
- ❌ Trigger collision
- ❌ Push effect

**YANG ADA:**
- ✅ Manual movement (transform.position)
- ✅ Manual hit detection (OverlapCircle)
- ✅ Instant destroy on hit
- ✅ Skip owner (tidak kena diri sendiri)

### Cara Kerja:

```csharp
void Update() {
    // 1. Gerak manual
    transform.position += direction * speed * Time.deltaTime;
    
    // 2. Check hit manual
    Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius, layers);
    
    // 3. Kalau hit → apply damage → destroy
    if (hit) {
        target.TakeDamage(damage);
        Destroy(this);
    }
}
```

### Hasil:
- ✅ Tidak ada collider di cannonball
- ✅ Tidak bisa push kapal (secara fisik impossible!)
- ✅ Hit detection tetap akurat
- ✅ Damage apply dengan benar
- ✅ Sesimpel itu!

---

## 🎯 Testing Shooting System:

### Test Critical Hit Probability:

1. **Play Mode**
2. **Arahkan mouse** ke arah target
3. **Left Click** untuk menembak
4. **Cek Console:**
   ```
   Normal shot. Damage: 20
   CRITICAL HIT! Damage: 40 (25% chance)
   ```
5. **Cek Visual:**
   - Cannonball putih = Normal
   - Cannonball merah = Critical
6. **Cek Floating Damage Text:**
   - Normal hit = Angka putih kecil "20" muncul dan naik
   - Critical hit = Angka merah BESAR "40" muncul dan naik
   - Text auto fade out setelah 1.5 detik

### Test Fire Rate:

- Spam left click → Hanya bisa tembak sesuai fire rate
- Fire Rate 1 = 1 detik sekali
- Fire Rate 2 = 0.5 detik sekali

### Test Damage & Floating Text:

1. Setup 2 player di scene
2. Player 1 tembak Player 2
3. Cek health bar Player 2 berkurang
4. Critical hit = 2x damage (default 40)
5. **Lihat floating text muncul:**
   ```
   Normal: "20" (putih, kecil)
   Critical: "40" (merah, besar, bold)
   ```
6. Text naik ke atas dan fade out
7. Ada sedikit random offset untuk multiple hits

**🐛 Debug Floating Text:**

Kalau text masih "99" atau tidak muncul:

```
1. Play mode
2. Tembak enemy
3. Cek Console log:

Expected:
✅ "Spawned floating text: Damage=20, Critical=false"
✅ "FloatingText initialized: 20 | Critical: false | Color: (1, 1, 1, 1)"

Kalau tidak ada log:
❌ Prefab tidak assigned di Cannonball
❌ Check Cannonball.cs → Damage Text Prefab

Kalau ada warning "not initialized":
❌ FloatingDamageText.cs tidak ada di prefab
❌ TextMeshPro component tidak ada
```

**Quick Fix:**
1. Open FloatingDamageText Prefab
2. Cek Inspector:
   - ✅ TextMeshPro component ada?
   - ✅ FloatingDamageText.cs component ada?
   - ✅ Text Mesh property terisi?
3. Open Cannonball Prefab
4. Cek Floating Damage Text section:
   - ✅ Damage Text Prefab = FloatingDamageText?
   - ✅ Normal Color = White?
   - ✅ Critical Color = Red?

---

## 🎨 Customization:

### Inspector UI (ShootingSystem):

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
ShootingSystem Component
━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Damage Settings
  Damage: [20      ] ← Base damage

━━━━━ PROBABILITY: CRITICAL HIT ━━━━━

  Critical Chance: [====□         ] 0.25
  Tooltip: Chance untuk critical hit (0 = 0%, 1 = 100%)
  
  Critical Multiplier: [====□     ] 2.0
  Tooltip: Damage multiplier untuk critical (2 = 2x damage)

Critical Visual Settings
  Critical Color: [🔴 Red]
  Normal Color: [⚪ White]
```

**Cara Edit:**

### Adjust Critical Chance (SLIDER):
```
Geser slider dari 0 sampai 1:
- 0.10 = 10% chance (jarang critical)
- 0.25 = 25% chance (default, balanced)
- 0.50 = 50% chance (sering critical)
- 1.00 = 100% chance (selalu critical)
```

### Adjust Critical Multiplier (SLIDER):
```
Geser slider dari 1 sampai 5:
- 1.5x = 30 damage (critical ringan)
- 2.0x = 40 damage (default)
- 3.0x = 60 damage (critical kuat!)
- 5.0x = 100 damage (one-shot!)
```

### Adjust Base Damage:
```
Edit angka langsung:
- 10 = Weak shot
- 20 = Normal (default)
- 50 = Strong shot
- 100 = One-shot potential
```

### Adjust Fire Rate:
```csharp
Fire Rate:
- 0.5 = 1 shot per 2 detik (slow)
- 1.0 = 1 shot per detik (default)
- 2.0 = 2 shot per detik (fast)
- 5.0 = 5 shot per detik (rapid fire!)
```

---

## 🕹️ Nanti: Ganti ke Joystick

### Untuk Player 2 dengan Joystick:

1. **Edit Input Manager:**
   ```
   Edit → Project Settings → Input Manager
   
   Tambah Axis baru:
   - Horizontal2 (Analog Kanan X)
   - Vertical2 (Analog Kanan Y)
   ```

2. **Update ShootingSystem:**
   ```csharp
   // Di Inspector Player2
   Use Mouse Aim: FALSE
   Horizontal Axis: "Horizontal2"
   Vertical Axis: "Vertical2"
   Fire Key: (ganti ke joystick button)
   ```

3. **Setup Input Actions (New Input System):**
   ```
   Atau pakai New Input System:
   - Buat Input Action "Aim"
   - Bind ke Right Stick
   - Buat Input Action "Fire"
   - Bind ke R1/RB
   ```

---

## 🎪 Probability System Explained:

### Bagaimana Critical Hit Bekerja:

```csharp
// Di ShootingSystem.Fire()
bool isCritical = Random.value <= criticalChance;
// Random.value = 0.0 sampai 1.0

Contoh dengan 25% chance (0.25):
- Random.value = 0.15 → 0.15 <= 0.25 → TRUE (Critical!)
- Random.value = 0.50 → 0.50 <= 0.25 → FALSE (Normal)
- Random.value = 0.99 → 0.99 <= 0.25 → FALSE (Normal)
- Random.value = 0.10 → 0.10 <= 0.25 → TRUE (Critical!)
```

### Formula Damage:
```csharp
if (isCritical)
    finalDamage = damage * criticalMultiplier
    // 20 * 2.0 = 40
else
    finalDamage = damage
    // 20
```

### Visual Feedback:
```csharp
- Normal: White cannonball
- Critical: Red cannonball
- Console log menunjukkan damage dan chance
```

---

## 🐛 Troubleshooting:

**Cannonball tidak spawn:**
- Cek cannonballPrefab sudah di-drag
- Cek firePoint sudah di-assign
- Cek Console untuk error message

**Cannon tidak rotate ke mouse:**
- Cek cannonTransform sudah di-drag
- Cek "Use Mouse Aim" = TRUE
- Adjust rotation offset di code (line 112)

**Cannonball tidak hit player:**
- Cek Cannonball.cs → Hit Layers sudah di-set
- Set ke layer "Player" atau "Enemy"
- Cek player GameObject ada di layer tersebut
- Cek hitRadius tidak terlalu kecil (default 0.3)

**Cannonball mendorong kapal:**
- ✅ IMPOSSIBLE! Tidak ada collider sama sekali!
- Tidak ada Rigidbody2D
- Tidak ada CircleCollider2D
- Manual movement + detection = NO PUSH!

**Cannonball kena diri sendiri:**
- ✅ SUDAH FIXED! Skip owner di CheckHit()
- if (hit.gameObject == owner) continue;
- Owner di-pass dari ShootingSystem

**Critical tidak pernah muncul:**
- Cek Critical Chance > 0
- Cek Console log setiap tembakan
- Test dengan Critical Chance = 1.0 (always critical)

**Damage tidak apply:**
- Cek target punya PlayerHealth component
- Cek collision detection di Cannonball
- Cek Console log untuk "Hit ..." message

**Floating text masih placeholder "99":**
- ✅ Cek Console untuk "FloatingText initialized" log
- ✅ Cek Cannonball Prefab → Damage Text Prefab assigned?
- ✅ Cek FloatingDamageText prefab punya component FloatingDamageText.cs?
- ✅ Cek FloatingDamageText.textMesh di Inspector tidak null?
- ⚠️ PENTING: Jangan set text di prefab ke "99" manual!
  - Text akan di-set otomatis oleh script
  - Prefab harus punya default text (apa aja)

**Floating text tidak berubah warna:**
- Cek Cannonball Prefab → Normal/Critical Damage Color sudah di-set?
- Cek Console log untuk "Spawned floating text" dengan color info
- Test dengan Critical Chance = 1.0 untuk selalu critical
- Pastikan TextMeshPro di prefab tidak pakai material yang override color

---

## 📊 Progress Tugas Akhir:

- ✅ **FSM** - Player & AI State (DONE)
- ✅ **A*** - Pathfinding (DONE)
- ✅ **Dijkstra** - Pathfinding (DONE)
- ✅ **Probability** - Critical Hit System (DONE!)
- ⏳ **Cooperative FSM** - Player Coordination (NEXT)

---

## 💥 Floating Damage Text (RPG Style):

### Visual Feedback saat Hit:

**Normal Hit:**
```
  20  ← Angka putih, font size 4
  ↑   Naik ke atas
  ↑   Fade out 1.5 detik
```

**Critical Hit:**
```
  40  ← Angka MERAH, font size 6, BOLD
  ↑   Naik lebih cepat
  ↑   Scale animation (pop effect)
  ↑   Fade out 1.5 detik
```

### Animasi:

1. **Spawn** - Muncul di posisi hit
2. **Scale Up** - Membesar dari 0.5x → 1.2x (0.2s)
3. **Bounce** - Bounce back ke 1.0x (0.1s)
4. **Float** - Naik ke atas dengan speed 2 unit/s
5. **Fade** - Alpha 1.0 → 0.0 dalam 1.5 detik
6. **Random Offset** - Sedikit spread kiri/kanan

### Customization:

**Di FloatingDamageText Prefab:**
```
Animation:
- Float Speed: 2 (kecepatan naik)
- Fade Speed: 1 (kecepatan fade)
- Lifetime: 1.5 (durasi total)
- Random Offset: (0.5, 0.5) (spread area)

Scale Animation:
- Animate Scale: TRUE
- Start Scale: 0.5 (mulai kecil)
- End Scale: 1.2 (pop besar)
- Scale Time: 0.2 (durasi pop)
```

**Di Cannonball Prefab:**
```
Floating Damage Text:
- Damage Text Prefab: FloatingDamageText
- Normal Damage Color: White #FFFFFF
- Critical Damage Color: Red #FF0000
```

### Result:

Setiap hit akan spawn floating text yang:
- ✅ Menunjukkan damage yang di-apply
- ✅ Warna berbeda untuk critical
- ✅ Size berbeda untuk critical
- ✅ Animasi smooth (pop + float + fade)
- ✅ Auto cleanup (destroy setelah lifetime)
- ✅ Multiple hits tidak overlap (random offset)

**Seperti game RPG!** ⚔️

---

## 🎮 Controls (Testing):

**Player 1:**
- Movement: W/A/S/D + Shift (boost)
- Aim: Mouse
- Shoot: Left Click

**Player 2 (Nanti):**
- Movement: Arrow Keys + Right Shift
- Aim: Right Analog Stick
- Shoot: R1/RB Button

---

Made with ❤️ for AI for Game Final Project
**Tema: Probability-based Critical Hit System**

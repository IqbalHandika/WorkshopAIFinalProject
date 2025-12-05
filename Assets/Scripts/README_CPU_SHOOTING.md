# 🎯 CPU Shooting System - Auto-Aim

## 📋 Overview
System shooting otomatis untuk CPU dengan **auto-aim ke player terdekat**. CPU akan otomatis detect player (P1 atau P2) yang paling dekat dan shoot secara otomatis.

**Features:**
- ✅ Auto-detect player terdekat (P1 atau P2)
- ✅ Auto-aim cannon ke target
- ✅ Auto-shoot dengan fire rate + random delay
- ✅ Range-based shooting (min/max distance)
- ✅ Probability critical hit (lebih rendah dari player)
- ✅ Skip target yang sudah mati
- ✅ Performance optimized (update target per interval)

## 🎮 Setup di Unity

### Quick Setup CPU Shooting

#### 1. Setup CPU1 (Dijkstra)
```
CPU1 (GameObject)
├─ Tag: "Enemy" atau custom
├─ Cannon (Child GameObject) ← Drag ini
│  └─ FirePoint (Child of Cannon) ← Drag ini
├─ Components:
│  ├─ AutoMovement (pathfinding)
│  ├─ CPUHealthBar (health)
│  └─ CPUShootingSystem ✅ ADD THIS
```

**Settings:**
```
CPUShootingSystem
├─ CPU Name: "CPU 1 (Dijkstra)"
├─ Cannon Transform: (drag Cannon child)
├─ Fire Point: (drag FirePoint)
├─ Cannonball Prefab: (drag cannonball prefab)
├─ Cannonball Speed: 15
├─ Fire Rate: 0.5 (shoot tiap 2 detik)
├─ Damage: 15
├─ Player Tag: "Player"
├─ Max Shoot Range: 15
├─ Min Shoot Range: 2
├─ Critical Chance: 0.15 (15%)
├─ Critical Multiplier: 2.0
└─ Shoot Delay Range: (0.2, 0.5)
```

#### 2. Setup CPU2 (A*)
Same as CPU1, tapi name-nya "CPU 2 (A*)"

#### 3. Setup Player Tag
**PENTING!** Pastikan P1 dan P2 GameObject punya Tag:
```
Player1 GameObject → Inspector → Tag: "Player" ✅
Player2 GameObject → Inspector → Tag: "Player" ✅
```

## ⚙️ Component Settings Explained

### Cannon References
```csharp
Cannon Transform: Transform    // Child GameObject "Cannon"
Fire Point: Transform          // Child of Cannon, ujung meriam
```

### Shooting Settings
```csharp
Fire Rate: 0.5f               // 0.5 shoots/second = tiap 2 detik
Damage: 15f                   // Base damage (lebih kecil dari player)
Cannonball Speed: 15f         // Kecepatan projectile
Cannonball Lifetime: 5f       // Auto-destroy setelah 5 detik
```

### Auto-Aim Settings
```csharp
Player Tag: "Player"          // Tag untuk detect player
Max Shoot Range: 15f          // Jarak maksimum (lebih jauh = tidak shoot)
Min Shoot Range: 2f           // Jarak minimum (terlalu dekat = tidak shoot)
Target Update Interval: 0.5f  // Update target tiap 0.5 detik (performance)
```

### AI Behavior
```csharp
Shoot Delay Range: (0.2, 0.5) // Random delay 0.2-0.5s sebelum shoot
                               // Bikin CPU tidak terlalu robotik
```

### Probability Settings
```csharp
Critical Chance: 0.15f        // 15% critical (lebih rendah dari player 25%)
Critical Multiplier: 2.0f     // 2x damage saat critical
```

## 🎯 How It Works

### 1. Target Detection
```
Every 0.5 seconds:
├─ Find all GameObjects with tag "Player"
├─ Filter: Skip dead players (health <= 0)
├─ Calculate distance to each player
├─ Select nearest player
└─ Update currentTarget
```

### 2. Auto-Aim
```
Every frame:
├─ Calculate direction to currentTarget
├─ Update aimDirection vector
├─ Rotate cannon to face target
└─ Visual: Update aim line (optional)
```

### 3. Auto-Shoot
```
Can shoot if:
├─ ✅ Fire rate cooldown passed
├─ ✅ Has valid target
├─ ✅ Target is alive
├─ ✅ Distance in range (min < d < max)
└─ ✅ Random shoot delay passed

Then:
├─ Roll probability for critical hit
├─ Spawn cannonball
├─ Initialize with direction, speed, damage
├─ Apply visual (color red if critical)
└─ Set next fire time
```

## 📊 Balancing

### CPU vs Player Comparison

| Setting | Player | CPU | Reason |
|---------|--------|-----|--------|
| **Fire Rate** | 1.0 (1/sec) | 0.5 (1/2sec) | CPU lebih lambat = lebih fair |
| **Damage** | 20 | 15 | CPU lebih lemah |
| **Critical Chance** | 25% | 15% | Player lebih sering crit |
| **Aim** | Manual | Auto | CPU auto-aim tapi slower |
| **Shoot Delay** | Instant | 0.2-0.5s | Random delay bikin CPU tidak robotik |

### Difficulty Adjustment

**Easy Mode (CPU Weak):**
```csharp
fireRate = 0.3f;              // 1 shot tiap 3.3 detik
damage = 10f;                 // Low damage
criticalChance = 0.1f;        // 10% crit
maxShootRange = 12f;          // Short range
```

**Normal Mode (Balanced):**
```csharp
fireRate = 0.5f;              // 1 shot tiap 2 detik ✅ DEFAULT
damage = 15f;                 // Medium damage
criticalChance = 0.15f;       // 15% crit
maxShootRange = 15f;          // Medium range
```

**Hard Mode (CPU Strong):**
```csharp
fireRate = 0.8f;              // 1 shot tiap 1.25 detik
damage = 20f;                 // Same as player
criticalChance = 0.2f;        // 20% crit
maxShootRange = 20f;          // Long range
```

## 🔧 Advanced Features

### 1. Range Visualization (Gizmos)
Di Scene view saat play mode:
- **Green Circle**: Max shoot range
- **Red Circle**: Min shoot range
- **Yellow Line**: Current aim direction
- **Cyan Line**: Connection to current target

### 2. Smart Target Selection
```csharp
// CPU prioritize player terdekat
FindNearestPlayer() {
    foreach (player in players) {
        // Skip dead players ✅
        if (!player.IsAlive()) continue;
        
        // Calculate distance
        distance = Vector2.Distance(cpu, player);
        
        // Select nearest
        if (distance < nearestDistance) {
            nearest = player;
        }
    }
}
```

### 3. Random Shoot Delay
```csharp
// Setelah fire rate cooldown, tambah random delay
if (CanShoot()) {
    if (shootDelay <= 0) {
        Fire();
        shootDelay = Random.Range(0.2f, 0.5f); // Random!
    }
}
```
Ini bikin CPU tidak shoot langsung begitu cooldown habis, lebih natural.

## 💻 Code Integration

### Integration dengan CPUHealthBar
```csharp
// Di CPUHealthBar.cs OnDeath()
void Die() {
    // Disable shooting saat mati
    CPUShootingSystem shooter = GetComponent<CPUShootingSystem>();
    if (shooter != null) {
        shooter.SetShootingEnabled(false);
    }
}
```

### Integration dengan Cannonball
Script `Cannonball.cs` sudah support CPU shooter:
```csharp
// Owner di-set ke CPU GameObject
cbScript.Initialize(aimDirection, speed, damage, isCritical, this.gameObject);

// Cannonball skip collision dengan owner
if (owner != null && hit.gameObject == owner)
    continue;
```

## 🐛 Troubleshooting

### CPU Tidak Shoot
**Cek:**
- ✅ `CPUShootingSystem` component attached
- ✅ `cannonballPrefab` assigned
- ✅ `cannonTransform` dan `firePoint` assigned
- ✅ Player GameObject punya Tag "Player"
- ✅ Player dalam range (2 < distance < 15)
- ✅ Console log: "[CPU 1] New target: Player1"

### CPU Shoot Terus-Terusan
**Cek:**
- ✅ `fireRate` terlalu tinggi? (set ke 0.5)
- ✅ `shootDelayRange` terlalu kecil? (set ke 0.2-0.5)

### CPU Tidak Aim ke Player
**Cek:**
- ✅ `FindNearestPlayer()` menemukan player (console log)
- ✅ `currentTarget` tidak null (Gizmos cyan line)
- ✅ Player Tag benar (case-sensitive!)

### Cannon Tidak Rotate
**Cek:**
- ✅ `cannonTransform` assigned (drag Cannon child)
- ✅ Cannon GameObject bebas rotate (not locked)
- ✅ Cannon offset angle benar (default: -90)

### CPU Hit Sendiri
**Cek:**
- ✅ `Initialize()` pass `this.gameObject` sebagai owner
- ✅ `Cannonball.cs` skip owner di CheckHit()

### CPU Shoot Player Mati
**Cek:**
- ✅ `PlayerHealth.IsAlive()` return false saat health <= 0
- ✅ `FindNearestPlayer()` skip dead players

## 🎨 Visual Customization

### Aim Line (Optional)
```csharp
showAimLine = true;  // Enable untuk debug (disable di production)
```
Add LineRenderer component:
```
CPU1 GameObject
└─ Add Component → LineRenderer
   ├─ Width: 0.05
   ├─ Color: Yellow
   └─ Material: Default-Line
```

### Critical Visual
```csharp
criticalColor = Color.red;      // Cannonball merah saat crit
normalColor = Color.white;      // Cannonball putih normal
```

### Audio
```csharp
// Add AudioSource component
audioSource = GetComponent<AudioSource>();

// Assign audio clips
fireSound = [your fire sound clip];
criticalSound = [your critical sound clip];
```

## 📝 Testing Checklist

Setup:
- [ ] CPUShootingSystem added to CPU1
- [ ] CPUShootingSystem added to CPU2
- [ ] Cannon & FirePoint assigned
- [ ] Cannonball prefab assigned
- [ ] Player1 & Player2 tagged "Player"

Functionality:
- [ ] CPU detect nearest player (check console log)
- [ ] Cannon rotate ke arah player
- [ ] CPU auto-shoot dengan interval
- [ ] Cannonball spawn dari FirePoint
- [ ] Damage applied ke player (check player health bar)
- [ ] Critical hit kadang muncul (red cannonball)
- [ ] CPU tidak shoot player mati
- [ ] CPU tidak shoot jika terlalu jauh/dekat

Polish:
- [ ] Gizmos range circles visible di Scene view
- [ ] Random shoot delay working (tidak robotik)
- [ ] Audio feedback (optional)

## 🎓 Game Balance Notes

**2 Players vs 2 CPUs:**
```
Team Player:
├─ P1: Manual aim, fast fire rate
├─ P2: Manual aim, fast fire rate
└─ Advantage: Coordination, skill-based

Team CPU:
├─ CPU1 (Dijkstra): Auto-aim, slower fire
├─ CPU2 (A*): Auto-aim, slower fire
└─ Advantage: Always accurate, different pathfinding
```

**Balance Strategy:**
- CPU auto-aim = always accurate
- BUT: Slower fire rate + random delay
- AND: Lower damage + lower crit chance
- RESULT: Fair 2v2 battle! ⚖️

## 🚀 Next Steps

1. **Test Balance**:
   - Play 2v2 match
   - Adjust fire rate, damage, range
   - Tune difficulty

2. **Add Polish**:
   - Muzzle flash effect
   - Sound effects
   - Screen shake on hit

3. **Advanced AI**:
   - Prediction (lead target)
   - Dodge behavior
   - Team coordination

4. **Cooperative FSM** (Final requirement):
   - P1 + P2 coordination
   - Complete 5 themes! ✅

## 📚 Related Documentation
- `README_SHOOTING.md` - Player shooting system
- `README_CPU_HEALTH.md` - CPU health bar
- `README_WAYPOINT.md` - CPU pathfinding
- `README_PATHFINDING.md` - Dijkstra vs A*

---

## 🎯 Summary: CPU Shooting Setup

```
1. Add CPUShootingSystem component to CPU GameObject
2. Drag Cannon child → Cannon Transform
3. Drag FirePoint child → Fire Point
4. Drag Cannonball prefab → Cannonball Prefab
5. Set Player Tag to "Player" on P1 & P2
6. Play & Test! ✅
```

CPU will automatically:
- ✅ Detect nearest player
- ✅ Aim cannon
- ✅ Shoot with timing
- ✅ Apply damage
- ✅ Roll for critical hits

**That's it! Simple setup, smart behavior!** 🚢⚔️

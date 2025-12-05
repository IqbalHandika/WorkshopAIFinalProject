# 🔧 CPU Shooting Troubleshooting - Quick Fix

## ❌ CPU Cannon Tidak Aim ke Player

### Cek Console Logs (PENTING!)

Play mode, lalu cek Console untuk messages:

#### 1. Initialization Check
```
Expected:
[CPU 1 (Dijkstra)] CPUShootingSystem initialized. Player tag: 'Player', Cannon: OK

Problems:
[CPU 1] Player tag: '', Cannon: NULL  ← Tag kosong & Cannon null!
```

#### 2. Target Detection Check
```
Expected:
[CPU 1] Finding nearest player... Found 2 objects with tag 'Player'
[CPU 1] Distance to Player1: 8.5
[CPU 1] Distance to Player2: 12.3
[CPU 1] NEW TARGET: Player1 (distance: 8.5)

Problems:
[CPU 1] Found 0 objects with tag 'Player'  ← Player tidak punya tag!
```

#### 3. Aim Direction Check
```
Expected:
[CPU 1] Aiming at Player1: direction = (0.8, 0.6), angle = 36.9°
[CPU 1] Cannon rotation: 306.9° (target angle: -53.1°)

Problems:
[CPU 1] Cannon Transform is NULL!  ← Cannon tidak di-assign!
```

---

## ✅ Step-by-Step Fix

### Fix 1: Assign Player Tag
**Problem:** Player GameObject tidak punya tag "Player"

**Solution:**
```
1. Select Player1 GameObject
2. Inspector → Tag (dropdown) → Player
3. Select Player2 GameObject
4. Inspector → Tag (dropdown) → Player
5. Play mode → Check console: "Found 2 objects with tag 'Player'"
```

### Fix 2: Assign Cannon Transform
**Problem:** Cannon Transform field kosong di Inspector

**Solution:**
```
1. Select CPU1 GameObject
2. Find CPUShootingSystem component
3. Cannon Transform: (drag "Cannon" child GameObject)
4. Fire Point: (drag "FirePoint" child GameObject)
5. Play mode → Check console: "Cannon: OK"
```

### Fix 3: Check Hierarchy Structure
**Expected structure:**
```
CPU1 (GameObject)
├─ CPUShootingSystem (Component) ← Script attached here
├─ Cannon (Child GameObject) ← Drag THIS to Cannon Transform
│  └─ FirePoint (Child of Cannon) ← Drag THIS to Fire Point
└─ Other components...
```

**Common mistake:**
```
CPU1
└─ Cannon ← Salah! Cannon harus jadi CHILD of CPU1!
```

### Fix 4: Verify Tag Name (Case Sensitive!)
```
✅ Correct: "Player"
❌ Wrong:   "player"  (lowercase)
❌ Wrong:   "PLAYER"  (uppercase)
❌ Wrong:   "player " (space di akhir)
```

### Fix 5: Check Cannon Rotation Settings
**Problem:** Cannon rotation terkunci atau constraint

**Solution:**
```
1. Select Cannon GameObject
2. Inspector → Transform
3. Make sure Rotation Z is NOT locked
4. Check parent constraints (should be none)
```

---

## 🔍 Debug Mode (Enhanced Logging)

Script sudah di-update dengan enhanced logging. Check console untuk:

### Every 0.5 seconds:
- Finding nearest player
- Number of players found
- Distance to each player
- Current target selection

### Every 1 second:
- Current aim direction
- Aim angle
- Cannon rotation angle

### Warnings:
- No players found
- Cannon transform null
- No valid target

---

## 📋 Quick Checklist

**Setup:**
- [ ] Player1 tagged "Player"
- [ ] Player2 tagged "Player"
- [ ] Cannon child exists
- [ ] FirePoint child exists
- [ ] CPUShootingSystem component attached

**Inspector (CPUShootingSystem):**
- [ ] Player Tag = "Player"
- [ ] Cannon Transform assigned
- [ ] Fire Point assigned
- [ ] Cannonball Prefab assigned

**In Play Mode - Console Logs:**
- [ ] "CPUShootingSystem initialized. Cannon: OK"
- [ ] "Found 2 objects with tag 'Player'"
- [ ] "NEW TARGET: Player1 (distance: X.X)"
- [ ] "Aiming at Player1: direction = ..."
- [ ] "Cannon rotation: X.X°"

**Visual Check:**
- [ ] Cannon GameObject rotates saat play mode
- [ ] Cannon menghadap ke player terdekat
- [ ] Gizmos: Cyan line dari CPU ke player (Scene view)

---

## 🎯 Test Commands

### Test 1: Tag Detection
```
Play mode → Console → Search "Found"
Expected: "Found 2 objects with tag 'Player'"
If 0: Fix player tags!
```

### Test 2: Cannon Assignment
```
Play mode → Console → Search "Cannon:"
Expected: "Cannon: OK"
If NULL: Assign Cannon Transform!
```

### Test 3: Target Lock
```
Play mode → Console → Search "NEW TARGET"
Expected: "NEW TARGET: Player1 (distance: X.X)"
If not found: Check player tags & health
```

### Test 4: Rotation
```
Play mode → Scene view → Watch Cannon GameObject
Expected: Cannon rotates to face player
If not: Check Transform lock or parent constraints
```

---

## 🐛 Common Issues & Solutions

### Issue: "Found 0 objects with tag 'Player'"
**Cause:** Player GameObject tidak punya tag
**Fix:** Tag Player1 & Player2 dengan "Player"

### Issue: "Cannon Transform is NULL"
**Cause:** Cannon Transform tidak di-assign di Inspector
**Fix:** Drag Cannon child ke field Cannon Transform

### Issue: Cannon rotate tapi salah arah
**Cause:** Sprite orientation atau offset angle salah
**Fix:** Adjust offset di `RotateCannon()`:
```csharp
// Try different offsets:
angle - 90f   // Default (sprite facing right)
angle         // Sprite facing up
angle - 180f  // Sprite facing left
angle + 90f   // Sprite facing down
```

### Issue: Cannon tidak smooth/glitchy rotation
**Cause:** Rotation applied tiap frame tanpa smoothing
**Fix:** Add smooth rotation:
```csharp
// Replace in RotateCannon():
cannonTransform.rotation = Quaternion.Slerp(
    cannonTransform.rotation, 
    targetRotation, 
    Time.deltaTime * 5f  // Smoothing factor
);
```

---

## 🎨 Visual Debug (Scene View)

Enable Gizmos di Scene view untuk lihat:
- **Green Circle**: Max shoot range
- **Red Circle**: Min shoot range
- **Cyan Line**: Connection ke current target
- **Yellow Line**: Aim direction

If no cyan line = no target detected!

---

## 📞 Still Not Working?

Check these:
1. Console logs (copy & check messages)
2. Inspector settings (screenshot CPUShootingSystem)
3. Hierarchy structure (is Cannon a child?)
4. Scene view Gizmos (any lines visible?)

**Quick test script:**
```csharp
// Add to CPU temporarily for debug
void Update() {
    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
    Debug.Log($"Found {players.Length} players");
    foreach(var p in players) {
        Debug.Log($"  - {p.name} at {p.transform.position}");
    }
}
```

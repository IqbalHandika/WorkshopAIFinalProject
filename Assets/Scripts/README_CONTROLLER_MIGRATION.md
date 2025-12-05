# 🔄 Migration Guide: Unified Controller System

## 📋 Overview

**OLD System (3 scripts):**
- ❌ `KeyMovement2D.cs` / `Movement.cs`
- ❌ `ShootingSystem.cs`
- ❌ `PlayerMovementController.cs`
- ❌ `PlayerShootingController.cs`

**NEW System (1 script):**
- ✅ `Controller.cs` - All-in-one player controller

**Benefits:**
- 🎯 Single script untuk movement + shooting
- 📊 Clean Inspector dengan organized headers
- 🎮 Unified keyboard/joystick support
- 🧹 Less clutter, easier to maintain

---

## 🚀 Migration Steps

### Step 1: Backup Project
```
IMPORTANT: Backup your project first!
- File → Save Project
- Copy entire project folder
```

### Step 2: Remove Old Components

#### For Player1 GameObject:
1. Select Player1 in Hierarchy
2. Remove these components (if present):
   - ❌ `KeyMovement2D`
   - ❌ `ShootingSystem`
   - ❌ `PlayerMovementController`
   - ❌ `PlayerShootingController`
3. Right-click component → Remove Component

#### For Player2 GameObject:
Same as Player1

### Step 3: Add New Component

#### For Player1:
1. Select Player1
2. Add Component → `Controller`
3. Done! ✅

#### For Player2:
Same as Player1

### Step 4: Configure Settings

#### Player1 Configuration:
```
Controller Component
├─ ━━━━━ PLAYER INFO ━━━━━
│  └─ Player Name: "Player 1"
│
├─ ━━━━━ INPUT MODE ━━━━━
│  └─ Use Keyboard Mouse: [✓] TRUE (keyboard/mouse)
│                    atau [✗] FALSE (joystick)
│
├─ Keyboard Input (if keyboard mode):
│  ├─ Up: W
│  ├─ Down: S
│  ├─ Left: A
│  ├─ Right: D
│  ├─ Boost Key: LeftShift
│  └─ Shoot Key: Mouse0
│
├─ Joystick Input (if joystick mode):
│  └─ Joystick Input: (assign JoystickInputMapper)
│
├─ ━━━━━ MOVEMENT SETTINGS ━━━━━
│  ├─ Max Speed: 6
│  ├─ Acceleration: 0.06
│  ├─ Friction: 0.995
│  └─ ... (keep defaults or adjust)
│
├─ ━━━━━ SHOOTING SETTINGS ━━━━━
│  ├─ Cannon Transform: (drag Cannon child)
│  ├─ Fire Point: (drag FirePoint child)
│  ├─ Cannonball Prefab: (drag prefab)
│  ├─ Cannonball Speed: 15
│  ├─ Fire Rate: 1
│  └─ Damage: 20
│
└─ ━━━━━ PROBABILITY: CRITICAL HIT ━━━━━
   ├─ Critical Chance: 0.25 (25%)
   └─ Critical Multiplier: 2.0 (2x damage)
```

#### Player2 Configuration:
Same as Player1, but:
- Player Name: "Player 2"
- Different keyboard keys (or joystick number 2)

---

## 🎯 Inspector Layout (NEW)

### Clean & Organized Sections:

```
▼ Controller (Script)
  
  ━━━━━━━━━ PLAYER INFO ━━━━━━━━━
  Player Name: Player 1
  
  ━━━━━━━━━ INPUT MODE ━━━━━━━━━
  Use Keyboard Mouse: [✓]
  
  Keyboard Input
  ├─ Up: W
  ├─ Down: S
  ├─ Left: A
  ├─ Right: D
  ├─ Boost Key: LeftShift
  └─ Shoot Key: Mouse0
  
  Joystick Input
  └─ Joystick Input: (None or JoystickInputMapper)
  
  ━━━━━━━━━ MOVEMENT SETTINGS ━━━━━━━━━
  Max Speed: 6
  Acceleration: 0.06
  Friction: 0.995
  Reverse Factor: 0.5
  Rudder Max Deg Per Frame: 0.7
  
  Boost Settings
  ├─ Boost Duration: 1
  ├─ Boost Max Speed Mult: 1.6
  └─ Boost Accel Mult: 1.8
  
  State Detection
  └─ Idle Speed Threshold: 0.1
  
  ━━━━━━━━━ SHOOTING SETTINGS ━━━━━━━━━
  
  Cannon References
  ├─ Cannon Transform: (Cannon)
  └─ Fire Point: (FirePoint)
  
  Cannonball
  ├─ Cannonball Prefab: (CannonballPrefab)
  ├─ Cannonball Speed: 15
  └─ Cannonball Lifetime: 5
  
  Fire Rate & Damage
  ├─ Fire Rate: 1
  └─ Damage: 20
  
  Aim Settings
  └─ Min Aim Distance: 0.3
  
  ━━━━━━━━━ PROBABILITY: CRITICAL HIT ━━━━━━━━━
  Critical Chance: 0.25
  Critical Multiplier: 2
  
  Critical Visual
  ├─ Critical Color: Red
  └─ Normal Color: White
  
  ━━━━━━━━━ VISUAL & AUDIO ━━━━━━━━━
  
  Aim Visual (Optional)
  ├─ Show Aim Line: [ ]
  ├─ Aim Line: (None)
  └─ Aim Line Length: 3
  
  Audio (Optional)
  ├─ Audio Source: (None)
  ├─ Fire Sound: (None)
  └─ Critical Sound: (None)
```

---

## 🔧 Code Changes

### Old Way (Multiple Scripts):
```csharp
// Need multiple components
KeyMovement2D movement = GetComponent<KeyMovement2D>();
ShootingSystem shooting = GetComponent<ShootingSystem>();

// Check state
movement.CurrentState
shooting.aimDirection
```

### New Way (Single Script):
```csharp
// Single component
Controller controller = GetComponent<Controller>();

// Check state
controller.CurrentState
controller.GetStateString()
controller.GetStateColor()
```

---

## 🎮 Input Mode Toggle

### Keyboard/Mouse Mode:
```
Use Keyboard Mouse: [✓] TRUE

Controls:
- WASD: Movement
- Mouse: Aim
- Left Click: Shoot
- Shift: Boost
```

### Joystick Mode:
```
Use Keyboard Mouse: [✗] FALSE

Controls:
- Analog Kiri: Movement
- Analog Kanan: Aim
- RB/R1: Shoot
- LT: Boost
- RT: Throttle
```

---

## 🧪 Testing Checklist

### Migration Test:
- [ ] Old components removed
- [ ] `Controller` component added
- [ ] Player name set correctly
- [ ] Input mode set (keyboard/joystick)

### Keyboard Mode Test:
- [ ] WASD moves ship
- [ ] Mouse aims cannon
- [ ] Left Click shoots
- [ ] Shift boosts
- [ ] FSM state updates (Idle/Moving/Boosting)

### Joystick Mode Test:
- [ ] Analog kiri moves ship
- [ ] Analog kanan aims cannon
- [ ] RB shoots
- [ ] LT boosts
- [ ] RT throttle works

### Shooting Test:
- [ ] Cannonball spawns
- [ ] Damage applied on hit
- [ ] Critical hits work (25% chance)
- [ ] Floating damage text appears
- [ ] Console logs shots

### UI Integration Test:
- [ ] GameUIManager detects `Controller` (not old scripts)
- [ ] PlayerStateUI works
- [ ] Health bar updates on damage

---

## ⚠️ Breaking Changes

### 1. Script References
If you have code that references old scripts:

**OLD:**
```csharp
KeyMovement2D movement = player.GetComponent<KeyMovement2D>();
```

**NEW:**
```csharp
Controller controller = player.GetComponent<Controller>();
```

### 2. GameUIManager References
Update `GameUIManager.cs` if it references old scripts:

**OLD:**
```csharp
public KeyMovement2D player1Movement;
public ShootingSystem player1Shooting;
```

**NEW:**
```csharp
public Controller player1Controller;
```

### 3. PlayerStateUI References
Update `PlayerStateUI.cs`:

**OLD:**
```csharp
KeyMovement2D movement;
```

**NEW:**
```csharp
Controller controller;
```

---

## 🐛 Troubleshooting

### Issue: Old component references missing
**Symptom:** Pink/missing script errors
**Fix:** 
1. Remove old components
2. Add new `Controller` component
3. Reconfigure settings

### Issue: Input not working
**Symptom:** Player tidak bergerak/shoot
**Fix:**
1. Check "Use Keyboard Mouse" toggle
2. If keyboard: Verify key bindings
3. If joystick: Assign JoystickInputMapper
4. Test in play mode

### Issue: Cannon not aiming
**Symptom:** Cannon tidak rotate
**Fix:**
1. Assign Cannon Transform in Inspector
2. Assign Fire Point in Inspector
3. Check hierarchy: Cannon is child of Player

### Issue: GameUIManager errors
**Symptom:** NullReferenceException
**Fix:**
1. Update GameUIManager script references
2. Change `KeyMovement2D` → `Controller`
3. Reassign in Inspector

---

## 📚 Related Documentation

- `README_JOYSTICK_INPUT.md` - Joystick setup
- `README_SHOOTING.md` - Shooting system
- `README_SETUP_UI.md` - UI integration

---

## ✅ Migration Checklist Summary

**Preparation:**
- [ ] Backup project

**Per Player (P1 & P2):**
- [ ] Remove old components
- [ ] Add `Controller` component
- [ ] Set player name
- [ ] Configure input mode
- [ ] Assign cannon references
- [ ] Assign cannonball prefab
- [ ] Test movement
- [ ] Test shooting
- [ ] Test FSM states

**Code Updates:**
- [ ] Update GameUIManager references
- [ ] Update PlayerStateUI references
- [ ] Update any custom scripts

**Final Test:**
- [ ] 2-player gameplay works
- [ ] Both keyboard/joystick modes work
- [ ] Shooting & critical hits work
- [ ] UI displays correctly

---

## 🎉 Benefits of New System

✅ **Single Script** - Easier to manage
✅ **Clean Inspector** - Organized sections with headers
✅ **Less Complexity** - No need to sync multiple components
✅ **Better Performance** - Less overhead
✅ **Easier Debugging** - All logic in one place
✅ **Consistent Code** - Unified input handling

---

**Migration complete! Now you have a clean, unified controller system!** 🚢⚔️

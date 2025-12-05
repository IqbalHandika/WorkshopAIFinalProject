# 🎮 Joystick Input Remapping System

## 📋 Overview
System untuk detect dan assign joystick input langsung dari Inspector dengan **button "Listen"**. Support 2 joystick berbeda untuk Player 1 dan Player 2.

**Features:**
- ✅ Button "🎮 Listen" di Inspector untuk detect input
- ✅ Support 2 joystick berbeda (Xbox, PlayStation, Generic, dll)
- ✅ Auto-detect axis names (Horizontal, Joy1Horizontal, RightStickHorizontal, etc)
- ✅ Auto-detect button numbers (JoystickButton0-19)
- ✅ Custom deadzone per player
- ✅ Real-time feedback saat listening
- ✅ 5 second timeout untuk prevent stuck

## 🎮 Control Scheme

### Player Controls:
```
STEER:    Analog Kiri (←→↑↓)
AIM:      Analog Kanan (←→↑↓)
THROTTLE: RT (Right Trigger)
BOOST:    LT (Left Trigger)
SHOOT:    RB/R1 (Right Bumper)
```

## 🚀 Quick Setup Guide

### Step 1: Setup Player 1

1. **Select Player1 GameObject**
2. **Add Component** → `PlayerMovementController`
3. **Add Component** → `PlayerShootingController`

4. **Configure PlayerMovementController:**
   ```
   ├─ Use Keyboard: FALSE (uncheck untuk joystick)
   └─ Joystick Input: (expand)
      ├─ Player Name: "Player 1"
      ├─ Joystick Number: 1
      └─ (Click "🎮 Listen" buttons - see Step 3)
   ```

5. **Configure PlayerShootingController:**
   ```
   ├─ Use Mouse Aim: FALSE (uncheck untuk joystick)
   └─ Joystick Input: (sama dengan movement)
   ```

### Step 2: Setup Player 2

Same as Player 1, tapi:
- Joystick Number: **2**
- Player Name: "Player 2"

### Step 3: Remap Input (IMPORTANT!)

#### For Each Input Field:

1. **Click "🎮 Listen" button** di sebelah input field
2. Button berubah jadi **"Listening..."** (kuning)
3. **Gerakkan analog stick atau tekan button** yang mau di-assign
4. System auto-detect dan assign axis/button name
5. Console log: `✅ Assigned 'Horizontal1' to 'moveHorizontalAxis'`

#### Example Workflow:

**Move Horizontal (Steer ←→):**
```
1. Click "🎮 Listen" di sebelah "Move Horizontal"
2. Gerakkan analog KIRI ke kiri/kanan
3. System detect: "Horizontal1" (atau "Joy1Horizontal")
4. Auto-assigned! ✅
```

**Aim Horizontal (Aim ←→):**
```
1. Click "🎮 Listen" di sebelah "Aim Horizontal"
2. Gerakkan analog KANAN ke kiri/kanan
3. System detect: "RightStickHorizontal1" (atau "4th axis")
4. Auto-assigned! ✅
```

**Throttle (RT):**
```
1. Click "🎮 Listen" di sebelah "Throttle (RT)"
2. Tekan RT (Right Trigger)
3. System detect: "RT1" (atau "9th axis")
4. Auto-assigned! ✅
```

**Shoot Button (RB):**
```
1. Click "🎮 Listen" di sebelah "Shoot Button"
2. Tekan RB/R1 button
3. System detect: "JoystickButton5"
4. Auto-assigned! ✅
```

## 🎯 Inspector Layout

### When Expanded:
```
Joystick Input Mapper
├─ Player Name: "Player 1"
├─ Joystick Number: 1
│
├─ ━━━ MOVEMENT - Analog Kiri ━━━
│  ├─ Move Horizontal (Analog L ←→): [Horizontal1    ] [🎮 Listen]
│  └─ Move Vertical (Analog L ↑↓):   [Vertical1      ] [🎮 Listen]
│
├─ ━━━ AIM - Analog Kanan ━━━
│  ├─ Aim Horizontal (Analog R ←→):  [4th axis       ] [🎮 Listen]
│  └─ Aim Vertical (Analog R ↑↓):    [5th axis       ] [🎮 Listen]
│
├─ ━━━ BOOST & THROTTLE - Triggers ━━━
│  ├─ Throttle (RT):                 [RT1            ] [🎮 Listen]
│  ├─ Boost (LT):                    [LT1            ] [🎮 Listen]
│  └─ Trigger Threshold: 0.1
│
├─ ━━━ SHOOT - Button ━━━
│  ├─ Shoot Button:                  [JoystickButton5] [🎮 Listen]
│  ├─ Shoot Key Code: JoystickButton5
│  └─ Boost Key Code: JoystickButton4
│
└─ ━━━ Deadzone Settings ━━━
   └─ Analog Deadzone: 0.15
```

## 🔧 Common Axis Names

### Xbox Controller:
```
Left Analog:
├─ Horizontal: "Horizontal", "Horizontal1"
└─ Vertical:   "Vertical", "Vertical1"

Right Analog:
├─ Horizontal: "4th axis", "RightStickHorizontal"
└─ Vertical:   "5th axis", "RightStickVertical"

Triggers:
├─ RT: "9th axis", "RT"
└─ LT: "10th axis", "LT"

Buttons:
├─ A:  JoystickButton0
├─ B:  JoystickButton1
├─ X:  JoystickButton2
├─ Y:  JoystickButton3
├─ LB: JoystickButton4
├─ RB: JoystickButton5
└─ Start: JoystickButton7
```

### PlayStation Controller:
```
Left Analog:
├─ Horizontal: "Horizontal", "Joy1Horizontal"
└─ Vertical:   "Vertical", "Joy1Vertical"

Right Analog:
├─ Horizontal: "3rd axis", "RightStickHorizontal"
└─ Vertical:   "4th axis", "RightStickVertical"

Triggers:
├─ R2: "6th axis"
└─ L2: "7th axis"

Buttons:
├─ Cross:  JoystickButton0
├─ Circle: JoystickButton1
├─ Square: JoystickButton2
├─ Triangle: JoystickButton3
├─ L1: JoystickButton4
├─ R1: JoystickButton5
└─ Options: JoystickButton9
```

## 📊 Testing Checklist

### Setup Test:
- [ ] Player1 has `PlayerMovementController` component
- [ ] Player1 has `PlayerShootingController` component
- [ ] Player2 has `PlayerMovementController` component
- [ ] Player2 has `PlayerShootingController` component
- [ ] Both Use Keyboard = FALSE
- [ ] Both Use Mouse Aim = FALSE
- [ ] Joystick Number: P1=1, P2=2

### Input Remapping Test:
- [ ] Listened and assigned Move Horizontal
- [ ] Listened and assigned Move Vertical
- [ ] Listened and assigned Aim Horizontal
- [ ] Listened and assigned Aim Vertical
- [ ] Listened and assigned Throttle (RT)
- [ ] Listened and assigned Boost (LT)
- [ ] Listened and assigned Shoot Button (RB)

### Play Mode Test - Player 1:
- [ ] Analog kiri: Kapal steer kiri/kanan
- [ ] Analog kiri: Kapal maju (atau RT throttle)
- [ ] RT: Kapal maju dengan throttle
- [ ] LT: Boost activated
- [ ] Analog kanan: Cannon rotate aim
- [ ] RB: Shoot cannonball

### Play Mode Test - Player 2:
- [ ] Same tests as Player 1
- [ ] Both players can control independently

## 🐛 Troubleshooting

### Issue: Button "Listen" tidak muncul
**Cause:** Custom Editor tidak ter-load
**Fix:**
1. Check file `JoystickInputMapperDrawer.cs` di folder `Assets/Scripts/Editor/`
2. Restart Unity Editor
3. Reimport script (right-click → Reimport)

### Issue: "Listening..." tidak stop
**Cause:** 5 second timeout atau input tidak detected
**Fix:**
1. Click button "Listening..." lagi untuk cancel
2. Coba gerakkan analog/button lebih kuat (> 30% threshold)
3. Check joystick ter-connect di Windows (joy.cpl)

### Issue: Input tidak detected saat listening
**Cause:** Joystick tidak configured di Unity Input Manager
**Fix:**
1. Edit → Project Settings → Input Manager
2. Add new axis manually, atau:
3. Try different axis variations (system auto-try multiple names)

### Issue: Wrong joystick responding
**Cause:** Joystick Number salah atau joystick connection order
**Fix:**
1. Check Joystick Number: P1=1, P2=2
2. Reconnect joystick di urutan yang benar (P1 first, then P2)
3. Test di Windows Game Controllers (joy.cpl)

### Issue: Analog stick drift (kapal gerak sendiri)
**Cause:** Deadzone terlalu kecil
**Fix:**
1. Increase Analog Deadzone: 0.15 → 0.25
2. Increase Trigger Threshold: 0.1 → 0.2

### Issue: Console log tidak muncul saat listening
**Cause:** Debug log filter atau listening tidak active
**Fix:**
1. Check Console clear on play = OFF
2. Check Console filter = All messages
3. Click "🎮 Listen" button dengan benar

## 💡 Tips & Tricks

### Tip 1: Test Joystick di Windows
```
1. Win+R → joy.cpl
2. Select joystick → Properties
3. Test all buttons and axes
4. Verify axis numbers dan button numbers
```

### Tip 2: Copy Input Mapping
```
Setup P1 first, then:
1. Right-click JoystickInputMapper → Copy Component
2. Select P2 → Right-click → Paste Component Values
3. Change Joystick Number dari 1 → 2
4. Remap jika axis berbeda
```

### Tip 3: Keyboard Fallback
```
For testing atau jika joystick disconnect:
1. Use Keyboard = TRUE
2. Define keyboard keys (WASD, Shift, Mouse)
3. Quick switch tanpa reconfigure
```

### Tip 4: Save Preset
```
Create prefab atau ScriptableObject untuk:
- Xbox preset
- PlayStation preset
- Generic USB preset
Quick load untuk different controllers
```

## 📝 Code Integration

### Get Input in Your Scripts:
```csharp
// Movement
float steer = joystickInput.GetMoveHorizontal();
float throttle = joystickInput.GetMoveVertical(); // or GetThrottle()
float boost = joystickInput.GetBoost();

// Aim
float aimH = joystickInput.GetAimHorizontal();
float aimV = joystickInput.GetAimVertical();
Vector2 aimDir = new Vector2(aimH, aimV).normalized;

// Shoot
if (joystickInput.GetShootButtonDown()) {
    Fire();
}
```

### Check Deadzone:
```csharp
// All Get methods already apply deadzone
float h = joystickInput.GetMoveHorizontal();
// Returns 0 if abs(value) < analogDeadzone
```

## 🎨 Visual Feedback

### Console Logs During Listening:
```
[Input Mapper] Listening for 'Move Horizontal'... Press button or move analog stick!
[Input Mapper] Assigned 'Horizontal1' to 'moveHorizontalAxis' (value: 0.87)
```

### Inspector Feedback:
- **White Button** = Ready to listen
- **Yellow Button** = Currently listening (move input now!)
- **Auto-update Field** = Value assigned when detected

## 📚 Advanced Features

### Custom Axis Detection:
System auto-try multiple axis name variations:
```csharp
string[] variations = {
    "Horizontal",           // Generic
    "Horizontal1",          // With joystick number
    "Joy1Horizontal",       // Joy prefix
    "Joystick1Horizontal",  // Full name
};
```

### Button Detection:
System check all 20 possible joystick buttons (0-19):
```csharp
for (int i = 0; i < 20; i++) {
    if (Input.GetKeyDown(KeyCode.JoystickButton0 + i)) {
        return $"JoystickButton{i}";
    }
}
```

### Timeout System:
5 second timeout prevents stuck listening state:
```csharp
if (timeSinceStartup - listenStartTime > 5f) {
    StopListening();
    Debug.LogWarning("Listen timeout");
}
```

## 🚀 Next Steps

1. **Setup Both Players:**
   - Attach components
   - Set Use Keyboard/Mouse = FALSE
   - Assign Joystick Numbers

2. **Remap All Inputs:**
   - Click all "🎮 Listen" buttons
   - Move/press corresponding inputs
   - Verify assignments

3. **Test in Play Mode:**
   - Both players can move
   - Both players can aim
   - Both players can shoot
   - No input conflicts

4. **Fine-tune:**
   - Adjust deadzones
   - Adjust trigger thresholds
   - Save optimal settings

## 📚 Related Documentation
- `README_SETUP_UI.md` - UI setup
- `README_SHOOTING.md` - Shooting system
- Unity Input Manager documentation

---

## ✅ Summary

**Setup Steps:**
1. Add `PlayerMovementController` & `PlayerShootingController`
2. Set Use Keyboard/Mouse = FALSE
3. Expand Joystick Input Mapper
4. Click "🎮 Listen" for each input
5. Move analog/press button
6. Auto-assigned! ✅

**Result:** Full joystick support dengan easy remapping! 🎮

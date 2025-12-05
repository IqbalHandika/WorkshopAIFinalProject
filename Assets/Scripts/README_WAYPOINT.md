# 🗺️ Waypoint System - CPU Patrol

## 📋 Overview
Sistem waypoint untuk CPU dengan **ping-pong looping pattern** (maju-mundur):
- CPU akan bergerak mengikuti waypoint secara berurutan
- Setelah sampai waypoint terakhir, CPU akan mundur kembali
- Pattern: `W1 → W2 → W3 → W2 → W1 → W2 → W3 ...`

## 🎮 Setup di Unity

### 1. Buat GameObject Waypoint
1. Buat Empty GameObject di scene
2. Rename: `Waypoint_CPU1_01`, `Waypoint_CPU1_02`, dll
3. Posisikan waypoint di jalur yang diinginkan
4. **TIP**: Buat parent folder `Waypoints_CPU1` untuk organisasi

### 2. Setup CPU dengan Waypoint
1. Pilih GameObject CPU (CPU1 atau CPU2)
2. Di Inspector, cari component **AutoMovement**
3. Di section **Pathfinding Algorithm**:
   - **CPU1**: Pilih **Dijkstra** ✅
   - **CPU2**: Pilih **A Star** ✅
4. Di section **Waypoint System**:
   - Set **Waypoints Size** = jumlah waypoint (misal 4)
   - Drag waypoint dari scene ke array **Waypoints**
   - Urutkan sesuai keinginan: `[0] = W1, [1] = W2, [2] = W3, [3] = W4`
5. Atur **Waypoint Reach Distance** (default 0.5):
   - Lebih kecil = lebih presisi (0.3)
   - Lebih besar = lebih smooth (0.8)
6. **Kosongkan field Target** (tidak dipakai jika pakai waypoint)

### 3. Contoh Setup

#### CPU1 (Dijkstra)
```
CPU1 (GameObject)
├─ AutoMovement (Component)
│  ├─ AI Name: "CPU 1 (Dijkstra)"
│  ├─ Pathfinding Algorithm: Dijkstra ✅
│  ├─ Waypoints: [4]
│  │  ├─ [0]: Waypoint_CPU1_01
│  │  ├─ [1]: Waypoint_CPU1_02
│  │  ├─ [2]: Waypoint_CPU1_03
│  │  └─ [3]: Waypoint_CPU1_04
│  ├─ Waypoint Reach Distance: 0.5
│  └─ Target: (None)
```

#### CPU2 (A*)
```
CPU2 (GameObject)
├─ AutoMovement (Component)
│  ├─ AI Name: "CPU 2 (A*)"
│  ├─ Pathfinding Algorithm: A Star ✅
│  ├─ Waypoints: [3]
│  │  ├─ [0]: Waypoint_CPU2_01
│  │  ├─ [1]: Waypoint_CPU2_02
│  │  └─ [2]: Waypoint_CPU2_03
│  ├─ Waypoint Reach Distance: 0.5
│  └─ Target: (None)
```

## 🔄 Pattern Looping

### Example: 4 Waypoints
```
Waypoints: W1, W2, W3, W4

Urutan gerakan:
W1 → W2 → W3 → W4 (maju)
      ↓
W4 → W3 → W2 (mundur)
      ↓
W2 → W3 → W4 (maju)
      ↓
W4 → W3 → W2 (mundur)
      ↓
... (loop terus)
```

### Code Logic
- **movingForward = true**: Index naik (0 → 1 → 2 → 3)
- Sampai mentok (index >= length): **movingForward = false**, mundur
- **movingForward = false**: Index turun (3 → 2 → 1 → 0)
- Sampai mentok (index < 0): **movingForward = true**, maju lagi

## ⚙️ Settings

### Waypoint Reach Distance
- **0.3 - 0.4**: Presisi tinggi, CPU harus benar-benar sampai waypoint
- **0.5 - 0.6**: Balanced (recommended)
- **0.7 - 1.0**: Smooth, CPU mulai ke waypoint berikutnya lebih awal

### Move Speed (di AutoMovement)
- Default: `3f`
- CPU lambat: `2f - 2.5f`
- CPU cepat: `3.5f - 4f`

## 🎯 Tips & Tricks

### 1. Visualisasi Waypoint di Scene
Buat script `WaypointVisualizer.cs`:
```csharp
using UnityEngine;

public class WaypointVisualizer : MonoBehaviour {
    public Color waypointColor = Color.cyan;
    public float gizmoSize = 0.5f;
    
    void OnDrawGizmos() {
        Gizmos.color = waypointColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
    }
}
```
Attach ke setiap waypoint GameObject untuk visualisasi di Scene view.

### 2. Setup Cepat Multiple Waypoints
1. Buat 1 waypoint
2. Duplicate (Ctrl+D)
3. Rename & reposition
4. Drag semua sekaligus ke Waypoints array

### 3. Testing Waypoint Order
Cek console untuk log:
```
[CPU 1 (A*)] Moving to waypoint 1 (FORWARD)
[CPU 1 (A*)] Moving to waypoint 2 (FORWARD)
[CPU 1 (A*)] Moving to waypoint 3 (FORWARD)
[CPU 1 (A*)] Moving to waypoint 2 (BACKWARD)
```

### 4. CPU1 vs CPU2 Different Routes & Algorithms
- **CPU1 (Dijkstra)**: 
  - Waypoint route A (misal jalur atas)
  - Pathfinding Algorithm: **Dijkstra** ✅
  - Karakteristik: Explore semua kemungkinan, cost-based
  
- **CPU2 (A*)**: 
  - Waypoint route B (misal jalur bawah)
  - Pathfinding Algorithm: **A Star** ✅
  - Karakteristik: Heuristic-based, lebih cepat, goal-directed
  
- Bisa overlap beberapa waypoint untuk area strategis
- **PENTING**: Pastikan GameObject Grid punya kedua component:
  - `Pathfinding` (untuk A*)
  - `PathfindingDijkstra` (untuk Dijkstra)

## 🐛 Troubleshooting

### CPU Tidak Bergerak
- ✅ Cek waypoints array terisi (tidak kosong)
- ✅ Cek waypoint GameObject aktif (not disabled)
- ✅ Cek PathRequestManager ada di scene
- ✅ Cek Grid system setup dengan benar
- ✅ Cek algoritma yang dipilih:
  - CPU1 harus **Dijkstra**
  - CPU2 harus **A Star**
- ✅ Cek GameObject Grid punya component `Pathfinding` dan `PathfindingDijkstra` (keduanya enabled)

### CPU Stuck di 1 Waypoint
- Perbesar **Waypoint Reach Distance** (coba 0.8)
- Cek apakah waypoint terlalu dekat dengan obstacle
- Cek console untuk error pathfinding

### CPU Loncat-loncat Waypoint
- Kecilkan **Waypoint Reach Distance** (coba 0.3)
- Pastikan waypoint tidak terlalu jauh satu sama lain

### CPU Pakai Target, Bukan Waypoint
- **Kosongkan field Target** di AutoMovement
- Pastikan Waypoints array terisi

## 🔧 Advanced: Mixed Mode

Jika ingin CPU kadang patrol, kadang chase player:
```csharp
// Di AutoMovement.cs, bisa tambahkan:
public bool useWaypointMode = true;

void Update() {
    if(useWaypointMode) {
        // Waypoint logic
    } else {
        // Target chase logic
    }
}
```

## 📝 Summary
- ✅ Waypoint system untuk CPU patrol pattern
- ✅ Ping-pong looping (maju-mundur otomatis)
- ✅ Mudah di-setup via Inspector
- ✅ Kompatibel dengan A* dan Dijkstra pathfinding
- ✅ Bisa different routes untuk CPU1 & CPU2

# 🧠 Pathfinding: A* vs Dijkstra - Penjelasan Lengkap

## 📖 Overview
Game ini menggunakan **2 algoritma pathfinding berbeda** untuk CPU:
- **CPU1**: Dijkstra's Algorithm (Reliable, Exploration-focused)
- **CPU2**: A* Algorithm (Smart, Goal-oriented)

**Dijkstra** dan **A*** adalah algoritma pathfinding untuk mencari jalur terpendek dari titik A ke titik B. Keduanya mirip, tapi ada perbedaan penting di cara kerjanya.

---

## 🔵 Dijkstra's Algorithm

### Cara Kerja:
1. **Explore semua arah** dari starting point
2. Pilih node dengan **cost terendah** (dari start)
3. Ulangi sampai ketemu goal
4. **Tidak peduli arah goal** - cek semua kemungkinan

### Karakteristik:
- ✅ **Guaranteed optimal path** (pasti jalur terpendek)
- ✅ **Reliable** - selalu cari jalur terbaik
- ❌ **Lambat** - explore banyak node yang gak perlu
- ❌ **Tidak efisien** kalau goal jauh

### Analogi:
Kayak **kamu nyari rumah teman** tapi **gak tahu arahnya**:
- Kamu jalan ke semua jalan dari rumah
- Cek semua gang kiri-kanan
- Akhirnya ketemu, tapi udah capek explore kemana-mana

### Formula:
```
f(n) = g(n)
```
- `g(n)` = jarak dari start ke node n

### Visualisasi:
```
Start → ●
         ├─→ ● ← explore semua arah
         ├─→ ●
         └─→ ●
              ├─→ ●
              ├─→ ● ← Goal (ketemu terakhir)
              └─→ ●
```

**Contoh di Map:**
```
Dijkstra explore: S → A → C → B → A → G
(explore semua kemungkinan berdasarkan cost)
```

---

## 🟢 A* Algorithm (A-Star)

### Cara Kerja:
1. **Explore arah ke goal** (pake heuristic)
2. Pilih node dengan **cost + estimate** terendah
3. Fokus ke arah goal, skip node yang jauh
4. **Pintar** - tahu arah mana yang lebih potensial

### Karakteristik:
- ✅ **Fast** - langsung ke arah goal
- ✅ **Efficient** - explore node yang perlu aja
- ✅ **Optimal** (kalau heuristic admissible)
- ❌ **Perlu heuristic function** (Manhattan/Euclidean distance)

### Analogi:
Kayak **kamu nyari rumah teman pakai GPS**:
- Kamu tahu arah rumah teman (utara/selatan)
- Fokus jalan ke arah sana
- Skip jalan yang keliatan salah arah
- Lebih cepat sampe!

### Formula:
```
f(n) = g(n) + h(n)
```
- `g(n)` = jarak dari start ke node n
- `h(n)` = **heuristic estimate** dari node n ke goal (contoh: jarak lurus)

### Visualisasi:
```
Start → ●
         └─→ ● ← fokus ke arah goal
              └─→ ● ← skip node yang jauh dari goal
                   └─→ Goal ✅ (ketemu cepat!)
```

**Contoh di Map:**
```
Start (S) → Goal (G)
S---1---A---2---G
 \       |
  3      1
   \     |
    B----C

A* explore: S → A → G
(langsung ke arah goal, skip B dan C karena h-cost tinggi)
```

---

## ⚖️ Comparison Table

| Aspect | Dijkstra | A* |
|--------|----------|-----|
| **Speed** | 🐢 Slow | 🚀 Fast |
| **Efficiency** | ❌ Low | ✅ High |
| **Optimal Path** | ✅ Yes | ✅ Yes (with good heuristic) |
| **Heuristic** | ❌ No | ✅ Yes (h(n)) |
| **Nodes Explored** | ⬆️ Many | ⬇️ Few |
| **Use Case** | Unknown goal direction | Known goal position |
| **Formula** | f(n) = g(n) | f(n) = g(n) + h(n) |

---

## 📊 Performance Comparison

### Scenario: Cari player di jarak 20 tiles

| Algorithm | Nodes Explored | Time | Path Length |
|-----------|----------------|------|-------------|
| **Dijkstra** | ~300 nodes | 15ms | 20 tiles |
| **A*** | ~25 nodes | 2ms | 20 tiles |

**Winner:** A* (12x faster!)

---

## 🎯 Kapan Pakai Dijkstra vs A*?

### Pakai Dijkstra kalau:
- ❓ Gak tahu posisi goal
- 🗺️ Perlu explore map
- 🔄 Multiple goals (cari yang terdekat)
- 📍 Goal bisa berubah-ubah

### Pakai A* kalau:
- ✅ Tahu posisi goal pasti
- 🎯 Single target
- ⚡ Butuh response cepat
- 🧠 Goal fixed (gak berubah)

---

## 💡 Heuristic Functions (untuk A*)

```csharp
// Manhattan Distance (grid-based)
float h = Mathf.Abs(current.x - goal.x) + Mathf.Abs(current.y - goal.y);

// Euclidean Distance (free movement)
float h = Vector2.Distance(current, goal);

// Diagonal Distance (8-directional)
float dx = Mathf.Abs(current.x - goal.x);
float dy = Mathf.Abs(current.y - goal.y);
float h = dx + dy + (1.414f - 2) * Mathf.Min(dx, dy);
```

### Pilih Heuristic:
- **Manhattan**: Grid-based game (4-directional)
- **Euclidean**: Free movement (top-down, racing) ← **Game ini pakai ini**
- **Diagonal**: Grid with diagonal (8-directional)

---

## 📝 Summary

| | Dijkstra | A* |
|---|---|---|
| **Analogi** | Jalan tanpa GPS | Jalan pakai GPS |
| **Explore** | Semua arah | Fokus ke goal |
| **Speed** | Lambat | Cepat |
| **Use Case** | Exploration | Chase target |
| **Formula** | f(n) = g(n) | f(n) = g(n) + h(n) |

**Kesimpulan:**
- **Dijkstra** = Reliable tapi lambat, cocok untuk exploration
- **A*** = Fast dan smart, cocok untuk chase/attack

**Untuk game ini:**
- **CPU 1 (Dijkstra)**: Patrol random, defensive behavior
- **CPU 2 (A*)**: Aggressive chase, offensive behavior

---

## 🎮 Implementation dalam Game

### Setup GameObject Grid
GameObject Grid harus punya **kedua component**:
```
Grid (GameObject)
├─ Grid (Component) - Grid system
├─ PathRequestManager (Component) - Request handler
├─ Pathfinding (Component) - A* algorithm ✅
└─ PathfindingDijkstra (Component) - Dijkstra algorithm ✅
```

### Setup CPU
#### CPU1 - Dijkstra
```
CPU1 (GameObject)
└─ AutoMovement (Component)
   ├─ AI Name: "CPU 1 (Dijkstra)"
   └─ Pathfinding Algorithm: Dijkstra ✅
```

#### CPU2 - A*
```
CPU2 (GameObject)
└─ AutoMovement (Component)
   ├─ AI Name: "CPU 2 (A*)"
   └─ Pathfinding Algorithm: A Star ✅
```

## 💻 Code Flow

### 1. CPU Request Path
```csharp
// Di AutoMovement.cs
PathRequestManager.RequestPath(
    transform.position,           // Start
    targetWaypoint.position,      // Goal
    OnPathFound,                  // Callback
    pathfindingAlgorithm          // Dijkstra atau A*
);
```

### 2. PathRequestManager Pilih Algorithm
```csharp
// Di PathRequestManager.cs
if (request.algorithm == PathfindingAlgorithm.Dijkstra) {
    dijkstra.StartFindPath(start, end);
} else {
    aStar.StartFindPath(start, end);
}
```

### 3. Algorithm Cari Path
**Dijkstra:**
```csharp
// Dijkstra: Sort by cost saja
frontier.Enqueue(node, costSoFar);
```

**A*:**
```csharp
// A*: Sort by f-cost (g + h)
int fCost = gCost + hCost;
openSet.Sort(by fCost);
```

### 4. Return Path ke CPU
```csharp
// Callback dengan hasil
OnPathFound(waypoints, success);
```

## 📊 Perbandingan Performance

| Aspect | Dijkstra | A* |
|--------|----------|-----|
| **Speed** | ⭐⭐ Slower | ⭐⭐⭐⭐ Faster |
| **Optimality** | ⭐⭐⭐⭐ Always | ⭐⭐⭐⭐ Yes (with admissible h) |
| **Nodes Explored** | ⭐⭐ Many | ⭐⭐⭐⭐ Few |
| **Memory Usage** | ⭐⭐⭐ Medium | ⭐⭐⭐ Medium |
| **Goal Awareness** | ❌ No | ✅ Yes |
| **Complexity** | ⭐⭐⭐ Simple | ⭐⭐⭐⭐ More Complex |

## 🎯 Kapan Gunakan Masing-Masing?

### Dijkstra - Best For:
- ✅ Multiple goals (cari shortest path ke semua tujuan)
- ✅ Tidak tahu tujuan akhir
- ✅ Map kecil (performance tidak masalah)
- ✅ Butuh explore semua kemungkinan

### A* - Best For:
- ✅ Single goal (tujuan jelas)
- ✅ Map besar (performance penting)
- ✅ Real-time game (butuh cepat)
- ✅ Goal-directed behavior

## 🔧 Tuning Pathfinding

### Grid Settings
Di component `Grid`:
```csharp
public float straightStepCost = 10f;  // Cost jalan lurus
public float diagonalStepCost = 14f;  // Cost diagonal (√2 × 10)
```

### A* Heuristic
Di `Pathfinding.cs`:
```csharp
// Manhattan distance (4-directional)
int h = Mathf.Abs(dx) + Mathf.Abs(dy);

// Diagonal distance (8-directional) ✅ Currently used
int h = 14 * Mathf.Min(dx, dy) + 10 * Mathf.Abs(dx - dy);

// Euclidean distance (true distance)
float h = Mathf.Sqrt(dx*dx + dy*dy);
```

## 🐛 Troubleshooting

### CPU1 Tidak Pakai Dijkstra
**Cek:**
1. GameObject Grid punya component `PathfindingDijkstra` (enabled)
2. CPU1 AutoMovement setting: `Pathfinding Algorithm = Dijkstra`
3. Console tidak ada error "Dijkstra pathfinding diminta tapi component tidak tersedia"

### CPU2 Tidak Pakai A*
**Cek:**
1. GameObject Grid punya component `Pathfinding` (enabled)
2. CPU2 AutoMovement setting: `Pathfinding Algorithm = A Star`
3. Console tidak ada error "A* pathfinding diminta tapi component tidak tersedia"

### Kedua CPU Pakai Algoritma Yang Sama
**Solusi:**
- Pastikan setting di AutoMovement berbeda:
  - CPU1: `Dijkstra`
  - CPU2: `A Star`

### Path Tidak Optimal
**Dijkstra:**
- Selalu optimal, jika tidak ada masalah di Grid atau movement cost

**A*:**
- Cek heuristic tidak overestimate (must be admissible)
- Pastikan diagonal cost benar (14 vs 10)

## 📝 Console Debug

Untuk verify algoritma yang digunakan, tambahkan di `PathRequestManager.cs`:
```csharp
Debug.Log($"[PathRequestManager] Using {currentPathRequest.algorithm} for path");
```

Output expected:
```
[PathRequestManager] Using Dijkstra for path  // CPU1
[PathRequestManager] Using AStar for path     // CPU2
```

## 🎓 Academic Context

Ini memenuhi requirement **tugas akhir AI for Game**:
- ✅ **FSM**: Player & CPU state management
- ✅ **Dijkstra**: CPU1 pathfinding
- ✅ **A***: CPU2 pathfinding
- ✅ **Probability**: Critical hit system
- ⏳ **Cooperative FSM**: 2-player coordination (TODO)

## 📚 References

**Dijkstra's Algorithm:**
- Invented by Edsger W. Dijkstra (1956)
- Shortest path from single source to all nodes
- Time complexity: O(V² ) or O(E log V) with priority queue

**A* Algorithm:**
- Invented by Peter Hart, Nils Nilsson, Bertram Raphael (1968)
- Extension of Dijkstra with heuristic guidance
- Time complexity: O(E) with good heuristic, O(b^d) worst case

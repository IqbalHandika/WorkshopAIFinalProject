using System.Collections.Generic;
using UnityEngine;

public class Node {
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX, gridY;
    public int gCost, hCost;
    public Node parent;
    public int movementCost;
    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _movementCost = 1){
        walkable=_walkable; worldPosition=_worldPos; gridX=_gridX; gridY=_gridY;
        movementCost = Mathf.Max(1,_movementCost);
    }
    public int fCost => gCost + hCost;
}

public class Grid : MonoBehaviour {
    public enum RouteNumberMode { Off, TileCost, RouteCumulative }

    [Header("Grid Area")]
    public Vector2 gridWorldSize = new Vector2(50,50);
    public float nodeRadius = 0.5f;

    [Header("Collision Check")]
    public LayerMask unwalkableMask;

    [Header("Terrain Cost (opsional)")]
    public LayerMask forestMask;
    public int forestTileCost = 5;
    public int defaultTileCost = 1;

    [Header("Route Label Options")]
    public RouteNumberMode routeNumberMode = RouteNumberMode.RouteCumulative;
    public float straightStepCost = 1f;
    public float diagonalStepCost = 1.5f;

    [Header("Progress (tuning)")]
    [Tooltip("Margin untuk membedakan jarak (world units).")]
    public float progressEpsilon = 0.02f;

    [Header("Gizmos")]
    public bool displayGridGizmos = true;

    // Data rute
    [System.NonSerialized] public List<Node> routeList;
    [System.NonSerialized] public List<float> routeCumCost;
    [System.NonSerialized] public Dictionary<Node,int> routeIndexMap;
    [System.NonSerialized] public int routeStartIndex = 0;

    public void SetRoute(IEnumerable<Node> nodes, IList<float> cumulativeCosts=null){
        if(nodes==null){ routeList=null; routeCumCost=null; routeIndexMap=null; return; }
        routeList = new List<Node>(nodes);
        if(cumulativeCosts!=null && cumulativeCosts.Count==routeList.Count)
            routeCumCost = new List<float>(cumulativeCosts);
        else routeCumCost=null;
        routeIndexMap = new Dictionary<Node,int>(routeList.Count);
        for(int i=0;i<routeList.Count;i++) routeIndexMap[routeList[i]]=i;
        // jangan reset routeStartIndex; biar gak flicker saat repath cepat
        routeStartIndex = Mathf.Clamp(routeStartIndex,0,Mathf.Max(0,routeList.Count-1));
    }

    public void ResetRouteProgress(){ routeStartIndex = 0; }

    // ======= PROGRESS: pakai proyeksi ke segmen i -> i+1 =======
    public void UpdateRouteProgressByPosition(Vector3 pos){
        if(routeList==null || routeList.Count<2) return;
        int i = Mathf.Clamp(routeStartIndex, 0, routeList.Count-2);

        // Coba maju beberapa segmen ke depan jika sudah melewati tengah segmen
        const int LOOKAHEAD = 8;
        int maxI = Mathf.Min(routeList.Count-2, i+LOOKAHEAD);
        float eps2 = progressEpsilon*progressEpsilon;

        while(i<=maxI){
            Vector3 A = routeList[i].worldPosition;
            Vector3 B = routeList[i+1].worldPosition;
            Vector3 AB = B - A;
            float ab2 = AB.sqrMagnitude;
            if (ab2 < 1e-6f) { i++; continue; } // node tumpuk: lewati

            // parameter t proyeksi pos ke garis AB
            float t = Vector3.Dot(pos - A, AB) / ab2;
            float dA2 = (pos - A).sqrMagnitude;
            float dB2 = (pos - B).sqrMagnitude;

            // aturan maju:
            // 1) kalau t > 0.5 (sudah melewati tengah segmen), atau
            // 2) jarak ke B lebih kecil dari jarak ke A dengan margin eps
            if (t > 0.5f || dB2 + eps2 < dA2) {
                routeStartIndex = i+1;
                i++;
            } else break;
        }
    }

    Node[,] grid;
    float nodeDiameter; int gridSizeX, gridSizeY;

    void Awake(){
        nodeDiameter = nodeRadius*2f;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
        CreateGrid();
    }

    void CreateGrid(){
        grid = new Node[gridSizeX,gridSizeY];
        Vector3 bl = transform.position - Vector3.right*gridWorldSize.x/2f - Vector3.up*gridWorldSize.y/2f;
        for(int x=0;x<gridSizeX;x++){
            for(int y=0;y<gridSizeY;y++){
                Vector3 p = bl + Vector3.right*(x*nodeDiameter+nodeRadius) + Vector3.up*(y*nodeDiameter+nodeRadius);
                bool walk = !Physics2D.OverlapCircle(p, nodeRadius*0.9f, unwalkableMask);
                int tileCost = defaultTileCost;
                if(Physics2D.OverlapCircle(p, nodeRadius*0.9f, forestMask)) tileCost = Mathf.Max(1,forestTileCost);
                grid[x,y] = new Node(walk,p,x,y,tileCost);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 wp){
        float px = (wp.x - (transform.position.x - gridWorldSize.x/2f))/gridWorldSize.x;
        float py = (wp.y - (transform.position.y - gridWorldSize.y/2f))/gridWorldSize.y;
        px=Mathf.Clamp01(px); py=Mathf.Clamp01(py);
        int x = Mathf.RoundToInt((gridSizeX-1)*px);
        int y = Mathf.RoundToInt((gridSizeY-1)*py);
        return grid[x,y];
    }

    public List<Node> GetNeighbours(Node n){
        var list = new List<Node>();
        for(int dx=-1; dx<=1; dx++){
            for(int dy=-1; dy<=1; dy++){
                if(dx==0 && dy==0) continue;
                int cx=n.gridX+dx, cy=n.gridY+dy;
                if(cx>=0 && cx<gridSizeX && cy>=0 && cy<gridSizeY) list.Add(grid[cx,cy]);
            }
        }
        return list;
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x,gridWorldSize.y,1));

        if(grid==null || !displayGridGizmos) return;

        // kotak semua node kecuali rute (biar angka rute gak ketutup)
        foreach(var n in grid){
            bool isRoute = (routeIndexMap!=null && routeIndexMap.ContainsKey(n));
            if(isRoute) continue;
            if(!n.walkable) Gizmos.color = Color.red;
            else if(n.movementCost>defaultTileCost) Gizmos.color = Color.yellow;
            else Gizmos.color = Color.white;
            Gizmos.DrawCube(n.worldPosition, Vector3.one*(nodeDiameter*0.5f));
        }

        #if UNITY_EDITOR
        if(routeList!=null && routeList.Count>0 && routeNumberMode!=RouteNumberMode.Off){
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            var style = new GUIStyle(UnityEditor.EditorStyles.boldLabel){
                fontSize=14, alignment=TextAnchor.MiddleCenter
            };
            style.normal.textColor = Color.black;

            int start = Mathf.Clamp(routeStartIndex,0,routeList.Count);
            for(int i=start;i<routeList.Count;i++){
                var n = routeList[i];
                string label;
                if(routeNumberMode==RouteNumberMode.TileCost) label = n.movementCost.ToString();
                else {
                    if(routeCumCost!=null && i<routeCumCost.Count) label = routeCumCost[i].ToString("0.##");
                    else label = n.movementCost.ToString();
                }
                UnityEditor.Handles.Label(n.worldPosition, label, style);
            }
        }
        #endif
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingDijkstra : MonoBehaviour {
    Grid grid; PathRequestManager req;

    void Awake(){ grid=GetComponent<Grid>(); req=GetComponent<PathRequestManager>(); }
    public void StartFindPath(Vector3 s, Vector3 t){ StartCoroutine(FindPath(s,t)); }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos){
        Vector3[] waypoints = new Vector3[0];
        bool ok=false;

        Node start = grid.NodeFromWorldPoint(startPos);
        Node goal  = grid.NodeFromWorldPoint(targetPos);

        if(start.walkable && goal.walkable){
            var frontier = new PriorityQueue<Node>();
            var cameFrom = new Dictionary<Node,Node>();
            var costSoFar= new Dictionary<Node,float>();

            frontier.Enqueue(start,0f); costSoFar[start]=0f;

            while(frontier.Count>0){
                Node cur = frontier.Dequeue();
                if(cur==goal){ ok=true; break; }

                foreach(Node nb in grid.GetNeighbours(cur)){
                    if(!nb.walkable) continue;
                    int dx=Mathf.Abs(nb.gridX-cur.gridX), dy=Mathf.Abs(nb.gridY-cur.gridY);
                    bool diag = (dx==1 && dy==1);
                    float step = diag? grid.diagonalStepCost : grid.straightStepCost;
                    float edge = step * Mathf.Max(1, nb.movementCost);
                    float nc = costSoFar[cur] + edge;
                    if(!costSoFar.ContainsKey(nb) || nc < costSoFar[nb]){
                        costSoFar[nb]=nc; cameFrom[nb]=cur; frontier.Enqueue(nb,nc);
                    }
                }
            }

            if(ok){
                var nodePath = Retrace(cameFrom,start,goal);

                // cumulative mulai dari langkah pertama
                var cum = new List<float>(nodePath.Count);
                float acc=0f;
                for(int i=0;i<nodePath.Count;i++){
                    if(i==0){
                        if(nodePath.Count>1){
                            var a=nodePath[0]; var b=nodePath[1];
                            int dx=Mathf.Abs(b.gridX-a.gridX), dy=Mathf.Abs(b.gridY-a.gridY);
                            bool diag=(dx==1 && dy==1);
                            float step = diag? grid.diagonalStepCost : grid.straightStepCost;
                            acc = step * Mathf.Max(1,b.movementCost);
                        } else acc=0f;
                        cum.Add(acc);
                    } else {
                        var a=nodePath[i-1]; var b=nodePath[i];
                        int dx=Mathf.Abs(b.gridX-a.gridX), dy=Mathf.Abs(b.gridY-a.gridY);
                        bool diag=(dx==1 && dy==1);
                        float step = diag? grid.diagonalStepCost : grid.straightStepCost;
                        acc += step * Mathf.Max(1,b.movementCost);
                        cum.Add(acc);
                    }
                }

                grid.SetRoute(nodePath, cum);
                waypoints = Simplify(nodePath);
            }
        }

        req.FinishedProcessingPath(waypoints, ok);
        yield return null;
    }

    List<Node> Retrace(Dictionary<Node,Node> prev, Node s, Node g){
        var path=new List<Node>(); Node cur=g;
        if(cur!=s && !prev.ContainsKey(cur)) return path;
        while(cur!=s){ path.Add(cur); cur=prev[cur]; }
        path.Add(s); path.Reverse(); return path;
    }

    Vector3[] Simplify(List<Node> p){
        if(p==null||p.Count==0) return new Vector3[0];
        var wp=new List<Vector3>(); Vector2 old=Vector2.zero;
        for(int i=1;i<p.Count;i++){
            int dx=p[i].gridX-p[i-1].gridX, dy=p[i].gridY-p[i-1].gridY;
            Vector2 nd=new Vector2(dx,dy);
            if(nd!=old){ wp.Add(p[i-1].worldPosition); old=nd; }
        }
        wp.Add(p[p.Count-1].worldPosition); return wp.ToArray();
    }
}

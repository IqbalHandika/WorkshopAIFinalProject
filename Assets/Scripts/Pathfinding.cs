using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour {
    Grid grid;
    PathRequestManager requestManager;

    void Awake() {
        grid = GetComponent<Grid>();
        requestManager = GetComponent<PathRequestManager>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos) {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos) {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        // hanya cari kalau dua-duanya walkable
        if (startNode.walkable && targetNode.walkable) {
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0) {
                // cari node dengan fCost terkecil
                Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++) {
                    if (openSet[i].fCost < currentNode.fCost ||
                        (openSet[i].fCost == currentNode.fCost &&
                         openSet[i].hCost < currentNode.hCost)) {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // kalau sudah sampai
                if (currentNode == targetNode) {
                    pathSuccess = true;
                    waypoints = RetracePath(startNode, targetNode);
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
                    if (!neighbour.walkable || closedSet.Contains(neighbour)) {
                        continue;
                    }

                    int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                        neighbour.gCost = newCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }
        }

        // kasih hasil balik ke manager
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);

        // nunggu frame berikutnya (sesuai konsep coroutine di video)
        yield return null;
    }

    Vector3[] RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(startNode);

        // path sekarang dari target → start, jadi kita balik urutannya
        path.Reverse();

        // convert Node list -> array posisi Vector3 (waypoints)
        List<Vector3> waypoints = new List<Vector3>();
        for (int i = 0; i < path.Count; i++) {
            waypoints.Add(path[i].worldPosition);
        }

        return waypoints.ToArray();
    }

    // heuristic jarak (8 arah gerak)
    int GetDistance(Node a, Node b) {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);

        // pakai cost 14 untuk diagonal, 10 untuk straight (classic A*)
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}

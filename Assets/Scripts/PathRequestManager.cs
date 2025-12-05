using System;
using System.Collections.Generic;
using UnityEngine;

// Enum untuk pilih algoritma pathfinding
public enum PathfindingAlgorithm {
    AStar,      // A* pathfinding
    Dijkstra    // Dijkstra pathfinding
}

public class PathRequestManager : MonoBehaviour {
    struct PathRequest {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;
        public PathfindingAlgorithm algorithm; // Algoritma yang digunakan

        public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback, PathfindingAlgorithm _algorithm) {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
            algorithm = _algorithm;
        }
    }

    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    PathRequest currentPathRequest;

    static PathRequestManager instance;

    PathfindingDijkstra dijkstra;
    Pathfinding aStar; // opsional (kalau script A* lama masih ada)

    bool isProcessingPath;

    void Awake() {
        instance = this;
        dijkstra = GetComponent<PathfindingDijkstra>();
        aStar    = GetComponent<Pathfinding>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback, PathfindingAlgorithm algorithm = PathfindingAlgorithm.AStar) {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback, algorithm);
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    void TryProcessNext() {
        if (!isProcessingPath && pathRequestQueue.Count > 0) {
            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;

            // Pilih algoritma berdasarkan request
            if (currentPathRequest.algorithm == PathfindingAlgorithm.Dijkstra) {
                // Gunakan Dijkstra
                if (dijkstra != null && dijkstra.enabled) {
                    dijkstra.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
                } else {
                    Debug.LogWarning("Dijkstra pathfinding diminta tapi component tidak tersedia atau disabled!");
                    FinishedProcessingPath(new Vector3[0], false);
                }
            } else {
                // Gunakan A* (default)
                if (aStar != null && aStar.enabled) {
                    aStar.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
                } else {
                    Debug.LogWarning("A* pathfinding diminta tapi component tidak tersedia atau disabled!");
                    FinishedProcessingPath(new Vector3[0], false);
                }
            }
        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool success) {
        currentPathRequest.callback(path, success);
        isProcessingPath = false;
        TryProcessNext();
    }
}

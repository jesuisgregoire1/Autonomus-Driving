using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;

class RoadPiece : IEquatable<RoadPiece>
{
    public Vector3Int position;
    public CityGridGenerator.RoadType type;
    public int yRotation;
    public GameObject road;


    public bool Equals(RoadPiece other)
    {
        return (position == other.position && type == other.type && yRotation == other.yRotation ||
            position == other.position && type == CityGridGenerator.RoadType.STRAIGHT && other.type == CityGridGenerator.RoadType.STRAIGHT
            && Mathf.Abs(yRotation - other.yRotation) == 180);
    }
}



public class CityGridGenerator : MonoBehaviour
{
    public GameObject crawler;
    public GameObject straight;
    public GameObject corner;
    public GameObject crossroad;
    public GameObject tjunction;
    public GameObject deadend;
    #region blocks
    public GameObject[] residentialSmall;
    public GameObject[] residentialMedium;
    public GameObject[] residentialLarge;
    public GameObject[] commercialSmall;
    public GameObject[] commercialMedium;
    public GameObject[] commercialLarge;
    public GameObject[] industrialSmall;
    public GameObject[] industrialMedium;
    public GameObject[] industrialLarge;
    public GameObject park;
    #endregion
    public GameObject[] fillers;
    public GameObject[] trees;

    public Vector3Int minDimensions = Vector3Int.zero;
    public Vector3Int maxDimensions = Vector3Int.zero;

    public enum RoadType { STRAIGHT, CROSS, CORNER, TJUNC };
    List<RoadPiece> roadPieces = new List<RoadPiece>();

    public enum PieceType { ROAD, HOUSE, SHACK, LAWN, COMMERCIAL, INDUSTRY, PARK, DEADEND, NONE };
    public Dictionary<Vector3Int, PieceType> cityMap = new Dictionary<Vector3Int, PieceType>();

    public enum ZoneType { R, C, I };
    List<List<int>> zones = new List<List<int>>();

    public int width = 10;
    public int depth = 10;
    Vector3Int crawlerPos;
    Vector3 dir = new Vector3(0, 0, 1);
    Vector3 neutral = new Vector3(0, 0, 1);

    float progress = 0.1f;
    private int centerZ;
    private int centerX;

    public Vector3 startPosition;
    public Vector3 endPosition;

    public delegate void CreateGrid();

    public event CreateGrid LetsCreateGrid;
    // Start is called before the first frame update
    void Start()
    {
        zones.Add(new List<int> { 0, 1 }); //residential
        zones.Add(new List<int> { 2, 3 }); //commercials
        zones.Add(new List<int> { 4, 5 }); //industry

        /*for (int i = 0; i <= 5; i++)
        {
            Vector3Int mapKey = Vector3Int.RoundToInt(Vector3Int.RoundToInt(dir * -i));
            if (!cityMap.ContainsKey(mapKey))
                cityMap.Add(mapKey, PieceType.ROAD);
        }*/ 
        
        //UnityEditor.EditorUtility.DisplayProgressBar("Generating City", "Drawing Roads", progress);
        startPosition = crawlerPos;
        StartCoroutine(Crawl());

    }


    void CheckOutOfBounds()
    {
        if ((crawlerPos.x) > width ||
             (crawlerPos.x) < 0 ||
             (crawlerPos.z) > depth ||
             (crawlerPos.z) < 0)
        {
            //ReclaimMap();
            crawlerPos.x = 0;
            crawlerPos.z = 0;
        }
    }

    public int numCrawls = 10;

    IEnumerator Crawl()
    {
        int crawls = 0;
        while (crawls < numCrawls)
        {
            //UnityEditor.EditorUtility.DisplayProgressBar("Generating City", "Drawing Roads", progress += crawls / numCrawls * 0.5f);
            int randomTurn = UnityEngine.Random.Range(0, 3);
            float rot;
            GameObject go;
            RoadPiece newRoad;

            if (randomTurn == 0)
            {
                dir = Quaternion.Euler(0, -90, 0) * dir;
                rot = Vector3.SignedAngle(neutral, dir, this.transform.up) + 90;
                go = Instantiate(corner, crawlerPos, Quaternion.identity);
                go.transform.Rotate(0, rot, 0);

                newRoad = new RoadPiece
                {
                    position = crawlerPos,
                    type = RoadType.CORNER,
                    yRotation = (int)Mathf.Round(go.transform.rotation.eulerAngles.y / 90) * 90,
                    road = go
                };

            }
            else if (randomTurn == 1)
            {
                dir = Quaternion.Euler(0, 90, 0) * dir;
                rot = Vector3.SignedAngle(neutral, dir, this.transform.up) + 180;
                go = Instantiate(corner, crawlerPos, Quaternion.identity);
                go.transform.Rotate(0, rot, 0);

                newRoad = new RoadPiece
                {
                    position = crawlerPos,
                    type = RoadType.CORNER,
                    yRotation = (int)Mathf.Round(go.transform.rotation.eulerAngles.y / 90) * 90,
                    road = go
                };
            }
            else
            {
                rot = Vector3.SignedAngle(neutral, dir, this.transform.up);
                go = Instantiate(straight, crawlerPos, Quaternion.identity);
                go.transform.Rotate(0, rot, 0);

                newRoad = new RoadPiece
                {
                    position = crawlerPos,
                    type = RoadType.STRAIGHT,
                    yRotation = (int)Mathf.Round(go.transform.rotation.eulerAngles.y / 90) * 90,
                    road = go
                };
            }

            yield return null;

            AddNoDuplications(newRoad);

            Vector3Int straightPos = crawlerPos + Vector3Int.RoundToInt(dir * 10);
            rot = Vector3.SignedAngle(neutral, dir, this.transform.up);
            go = Instantiate(straight, straightPos, Quaternion.identity);
            go.transform.Rotate(0, rot, 0);

            newRoad = new RoadPiece
            {
                position = straightPos,
                type = RoadType.STRAIGHT,
                yRotation = (int)Mathf.Round(go.transform.rotation.eulerAngles.y / 90) * 90,
                road = go
            };

            AddNoDuplications(newRoad);

            yield return null;

            /*for (int i = 0; i <= 20; i++)
            {
                Vector3Int mapKey = Vector3Int.RoundToInt(crawlerPos + Vector3Int.RoundToInt(dir * i));
                if (!cityMap.ContainsKey(mapKey))
                {
                    cityMap.Add(mapKey, PieceType.ROAD);
                }
            }*/

            yield return null;

            crawlerPos += Vector3Int.RoundToInt(dir * 20);
            crawler.transform.position = crawlerPos;

            if (minDimensions.x > crawlerPos.x) minDimensions.x = crawlerPos.x;
            if (minDimensions.z > crawlerPos.z) minDimensions.z = crawlerPos.z;
            if (maxDimensions.x < crawlerPos.x) maxDimensions.x = crawlerPos.x;
            if (maxDimensions.z < crawlerPos.z) maxDimensions.z = crawlerPos.z;

            CheckOutOfBounds();
            crawls++;
            
        }
        //ReclaimMap();
        //UnityEditor.EditorUtility.DisplayProgressBar("Generating City", "Mapping Roads", progress += 0.2f);
        Invoke("FindRoads", 0.1f);
        //UnityEditor.EditorUtility.DisplayProgressBar("Generating City", "Fixing Roads", progress += 0.2f);
        Invoke("FixRoads", 0.3f);
        //UnityEditor.EditorUtility.DisplayProgressBar("Generating City", "Building Houses", progress += 0.2f);
        Invoke("BuildHouses", 0.7f);
        endPosition = crawlerPos;
    }

    /*void ReclaimMap()
    {
        
        for (int i = 0; i < 5; i++)
        {
            Vector3Int mapKey = Vector3Int.RoundToInt(crawlerPos - Vector3Int.RoundToInt(dir * i));
            if (cityMap.ContainsKey(mapKey))
                if (!HitRoad(mapKey))
                    cityMap.Remove(mapKey);
        }
    }*/

    bool HitRoad(Vector3Int gridPos)
    {
        int roadMask = 1 << 6;
        RaycastHit hitUp;
        return Physics.Raycast(gridPos + new Vector3Int(0, 5, 0), Vector3.down, out hitUp, 10, roadMask);
       
    }

    void AddNoDuplications(RoadPiece newPiece)
    {
        bool found = false;
        foreach (RoadPiece r in roadPieces)
        {
            if (r.Equals(newPiece))
            {
                found = true;
                break;
            }
        }
        if (!found)
            roadPieces.Add(newPiece);
        else
            DestroyImmediate(newPiece.road);
    }

    void FixRoads()
    {
        Lookup<Vector3Int, RoadPiece> lookup = (Lookup<Vector3Int, RoadPiece>)roadPieces.ToLookup(p => p.position, p => p);

        foreach (IGrouping<Vector3Int, RoadPiece> roadGroup in lookup)
        {
            if (roadGroup.Count() > 1)
            {
                bool hasCorner0 = false;
                bool hasCorner90 = false;
                bool hasCorner180 = false;
                bool hasCorner270 = false;
                bool hasStraight0 = false;
                bool hasStraight90 = false;
                bool hasStraight180 = false;
                bool hasStraight270 = false;

                foreach (RoadPiece r in roadGroup)
                {
                    if (r.yRotation == 0 && r.type == RoadType.CORNER) hasCorner0 = true;
                    if (r.yRotation == 90 && r.type == RoadType.CORNER) hasCorner90 = true;
                    if (r.yRotation == 180 && r.type == RoadType.CORNER) hasCorner180 = true;
                    if (r.yRotation == 270 && r.type == RoadType.CORNER) hasCorner270 = true;
                    if (r.yRotation == 0 && r.type == RoadType.STRAIGHT) hasStraight0 = true;
                    if (r.yRotation == 90 && r.type == RoadType.STRAIGHT) hasStraight90 = true;
                    if (r.yRotation == 180 && r.type == RoadType.STRAIGHT) hasStraight180 = true;
                    if (r.yRotation == 270 && r.type == RoadType.STRAIGHT) hasStraight270 = true;
                    DestroyImmediate(r.road);
                }

                GameObject go = null;

                if (hasStraight0 && hasStraight90 ||
                    hasStraight90 && hasStraight180 ||
                    hasStraight180 && hasStraight270 ||
                    hasStraight270 && hasStraight0 ||
                    hasCorner0 && hasCorner180 ||
                    hasCorner90 && hasCorner270
                    ||
                    hasCorner90 && (hasStraight90 || hasStraight270) && hasCorner0 ||
                    hasCorner0 && (hasStraight0 || hasStraight180) && hasCorner270
                    )
                    Instantiate(crossroad, roadGroup.Key, Quaternion.identity);
                else if (hasCorner0 && hasCorner90 ||
                    hasCorner0 && hasStraight0 ||
                    hasCorner0 && hasStraight180 ||
                    hasCorner90 && hasStraight0 ||
                    hasCorner90 && hasStraight180)
                    Instantiate(tjunction, roadGroup.Key, Quaternion.identity);
                else if (hasCorner0 && hasCorner270 ||
                    hasCorner0 && hasStraight90 ||
                    hasCorner0 && hasStraight270 ||
                    hasCorner270 && hasStraight90 ||
                    hasCorner270 && hasStraight270)
                {
                    go = Instantiate(tjunction, roadGroup.Key, Quaternion.identity);
                    go.transform.Rotate(0, -90, 0);
                }
                else if (hasCorner90 && hasCorner180 ||
                    hasCorner90 && hasStraight90 ||
                    hasCorner90 && hasStraight270 ||
                    hasCorner180 && hasStraight90 ||
                    hasCorner180 && hasStraight270)
                {
                    go = Instantiate(tjunction, roadGroup.Key, Quaternion.identity);
                    go.transform.Rotate(0, 90, 0);
                }
                else if (hasCorner180 && hasCorner270 ||
                    hasCorner180 && hasStraight0 ||
                    hasCorner180 && hasStraight180 ||
                    hasCorner270 && hasStraight0 ||
                    hasCorner270 && hasStraight180)
                {
                    go = Instantiate(tjunction, roadGroup.Key, Quaternion.identity);
                    go.transform.Rotate(0, 180, 0);
                }
            }
        }
        
        CleanUpDeadEnds();
    }

    bool IsVoronoiType(int x, int z, ZoneType type)
    {
        //UnityEditor.EditorUtility.DisplayProgressBar("Generating City", "Generating Voronoi", progress += 0.2f);
        foreach (int t in zones[(int)type])
        {
            if (MeshUtils.voronoiMap[x + Mathf.Abs(minDimensions.x) + 10,
                    z + Mathf.Abs(minDimensions.z) + 10] == t)
                return true;
        }
        return false;
    }

    bool OutsideMap(Vector3Int gridPos)
    {
        return (gridPos.x > maxDimensions.x || gridPos.x < minDimensions.x ||
            gridPos.z > maxDimensions.z || gridPos.z < minDimensions.z);
    }

    void FindRoads()
    {
        for (int z = minDimensions.z - 10; z < maxDimensions.z + 10; z++)
        {
            for (int x = minDimensions.x - 10; x < maxDimensions.x + 10; x++)
            {
                Vector3Int pos = new Vector3Int(x, 0, z);
                if (HitRoad(pos) && !cityMap.ContainsKey(pos))
                {
                    cityMap.Add(pos, PieceType.ROAD);
                }
            }
        }

    }
    
    void BuildHouses()
    {
        MeshUtils.GenerateVoronoi(6, maxDimensions.x + Mathf.Abs(minDimensions.x) + 20,
                                                maxDimensions.z + Mathf.Abs(minDimensions.z) + 20);
        /*for (int z = minDimensions.z - 10; z < maxDimensions.z + 10; z++)
        {
            for (int x = minDimensions.x - 10; x < maxDimensions.x + 10; x++)
            {
                Vector3Int pos = new Vector3Int(x, 0, z);

                GameObject go = null;
                PieceType pt = PieceType.NONE;
                int r;
                float density = MeshUtils.fBM(x * 0.006f, z * 0.006f, 5);

                if (UnityEngine.Random.Range(0, 100) < 1)
                {
                    go = Instantiate(park, pos, Quaternion.identity);
                    pt = PieceType.PARK;
                }
                else 
                {

                    if (IsVoronoiType(x, z, ZoneType.R))
                    {
                        if (density < 0.5f)
                        {
                            r = UnityEngine.Random.Range(0, residentialSmall.Length);
                            go = Instantiate(residentialSmall[r], pos, Quaternion.identity);
                        }
                        else if (density < 0.6f)
                        {
                            r = UnityEngine.Random.Range(0, residentialMedium.Length);
                            go = Instantiate(residentialMedium[r], pos, Quaternion.identity);
                        }
                        else
                        {
                            r = UnityEngine.Random.Range(0, residentialLarge.Length);
                            go = Instantiate(residentialLarge[r], pos, Quaternion.identity);
                        }
                        pt = PieceType.HOUSE;
                    }

                    if (IsVoronoiType(x, z, ZoneType.C))
                    {
                        if (density < 0.464f)
                        {
                            r = UnityEngine.Random.Range(0, commercialSmall.Length);
                            go = Instantiate(commercialSmall[r], pos, Quaternion.identity);
                        }
                        else if (density < 0.62f)
                        {
                            r = UnityEngine.Random.Range(0, commercialMedium.Length);
                            go = Instantiate(commercialMedium[r], pos, Quaternion.identity);
                        }
                        else
                        {
                            r = UnityEngine.Random.Range(0, commercialLarge.Length);
                            go = Instantiate(commercialLarge[r], pos, Quaternion.identity);
                        }


                        pt = PieceType.COMMERCIAL;
                    }

                    if (IsVoronoiType(x, z, ZoneType.I))
                    {
                        if (density < 0.3f)
                        {
                            r = UnityEngine.Random.Range(0, industrialSmall.Length);
                            go = Instantiate(industrialSmall[r], pos, Quaternion.identity);
                        }
                        else if (density < 0.4f)
                        {
                            r = UnityEngine.Random.Range(0, industrialMedium.Length);
                            go = Instantiate(industrialMedium[r], pos, Quaternion.identity);
                        }
                        else
                        {
                            r = UnityEngine.Random.Range(0, industrialLarge.Length);
                            go = Instantiate(industrialLarge[r], pos, Quaternion.identity);
                        }
                        pt = PieceType.INDUSTRY;
                    }
                }
                if (go == null) continue;

                BoxCollider box = go.GetComponent<BoxCollider>();
                bool found = false;

                for (int j = (int)(-box.size.z / 2.0f); j < box.size.z / 2.0f; j++)
                {
                    for (int i = (int)(-box.size.x / 2.0f); i < box.size.x / 2.0f; i++)
                    {
                        Vector3Int mapKey = Vector3Int.RoundToInt(go.transform.position + new Vector3Int(i, 0, j));
                        if (cityMap.ContainsKey(mapKey) || OutsideMap(mapKey))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found == true) break;
                }

                if (found)
                {
                    DestroyImmediate(go);
                    go = null;
                    continue;
                }

                RaycastHit hitUp = new RaycastHit();
                RaycastHit hitForward = new RaycastHit();
                RaycastHit hitBack = new RaycastHit();
                RaycastHit hitLeft = new RaycastHit();
                RaycastHit hitRight = new RaycastHit();

                int roadMask = 1 << 6;
                if (!Physics.Raycast(pos, go.transform.forward, out hitForward, box.size.z + 1, roadMask) &&
                    !Physics.Raycast(pos, -go.transform.forward, out hitBack, box.size.z + 1, roadMask) &&
                    !Physics.Raycast(pos, -go.transform.right, out hitLeft, box.size.x + 1, roadMask) &&
                    !Physics.Raycast(pos, go.transform.right, out hitRight, box.size.x + 1, roadMask)
                    )
                {
                    DestroyImmediate(go);
                    go = null;
                }

                if (go != null)
                {
                    UnityEditor.EditorUtility.DisplayProgressBar("Generating City", "Adding Houses", progress += 0.01f);
                    if (hitForward.normal != Vector3.zero)
                    {
                        go.transform.LookAt(hitForward.point);
                    }
                    else if (hitBack.normal != Vector3.zero)
                    {
                        go.transform.LookAt(hitBack.point);
                    }
                    else if (hitLeft.normal != Vector3.zero)
                    {
                        go.transform.LookAt(hitLeft.point);
                    }
                    else if (hitRight.normal != Vector3.zero)
                    {
                        go.transform.LookAt(hitRight.point);
                    }

                    if (UnityEngine.Random.Range(0, 2) == 1)
                    {
                        if (!Physics.Raycast(go.transform.position - go.transform.forward, Vector3.up, out hitUp, 3))
                        {
                            go.transform.Translate(0, 0, -1);
                        }
                    }


                    for (int j = (int)(-box.size.z / 2.0f); j < box.size.z / 2.0f; j++)
                    {
                        for (int i = (int)(-box.size.x / 2.0f); i < box.size.x / 2.0f; i++)
                        {
                            Vector3Int mapKey = Vector3Int.RoundToInt(go.transform.position + new Vector3Int(i, 0, j));
                            if (!cityMap.ContainsKey(mapKey))
                            {
                                cityMap.Add(mapKey, pt);
                            }
                        }
                    }

                }
            }
        }
    */
        //Instantiate(fillers[0]);
        AddFillers(0, ZoneType.R);
        AddFillers(1, ZoneType.C);
        AddFillers(2, ZoneType.I);
        CleanUpDeadEnds();
        
        UnityEditor.EditorUtility.ClearProgressBar();
        if (LetsCreateGrid != null)
        {
            LetsCreateGrid.Invoke();
        }
    }

    void CleanUpDeadEnds()
    {
        GameObject[] ends = GameObject.FindGameObjectsWithTag("DeadEnd");
        Dictionary<Vector3Int, GameObject> matches = new Dictionary<Vector3Int, GameObject>();
        foreach (GameObject e in ends)
        {
            Vector3Int endPos = Vector3Int.RoundToInt(e.transform.position);
            if (!matches.ContainsKey(endPos))
                matches.Add(endPos, e);
            else
            {
                DestroyImmediate(matches[endPos]);
                DestroyImmediate(e);
            }
        }
    }

    void AddFillers(int modelId, ZoneType type)
    {
        List<Mesh> meshes = new List<Mesh>();
        List<Vector3> mPositions = new List<Vector3>();
        GameObject go = null;
        Material mat = null;
        PieceType thisPiece = PieceType.NONE;

        for (int z = minDimensions.z - 10; z < maxDimensions.z + 10; z++)
        {
            for (int x = minDimensions.x - 10; x < maxDimensions.x + 10; x++)
            {
                Vector3Int mapKey = new Vector3Int(x, 0, z);

                if (!cityMap.ContainsKey(mapKey) && IsVoronoiType(x, z, type))
                {
                    if (type == ZoneType.R) thisPiece = PieceType.LAWN;
                    else if (type == ZoneType.C) thisPiece = PieceType.COMMERCIAL;
                    else if (type == ZoneType.I) thisPiece = PieceType.INDUSTRY;

                    cityMap.Add(mapKey, thisPiece);

                    Vector3 treePos = mapKey + new Vector3(UnityEngine.Random.Range(-0.8f, 0.8f),
                                                                0,
                                                        UnityEngine.Random.Range(-0.8f, 0.8f));

                   /* if (!HitRoad(Vector3Int.RoundToInt(treePos)) && !OutsideMap(Vector3Int.RoundToInt(treePos)))
                    {
                        float placedTree1 = MeshUtils.fBM(x * 0.006f, z * 0.006f, 5);
                        float placedTree2 = MeshUtils.fBM(x * 0.005f, z * 0.005f, 2);
                        float placedTree3 = MeshUtils.fBM(x * 0.007f, z * 0.007f, 6);
                        float placedTree4 = MeshUtils.fBM(x * 0.004f, z * 0.004f, 3);
                        if (thisPiece == PieceType.LAWN)
                        {
                            if (placedTree1 < 0.4 && UnityEngine.Random.Range(0,10) < 1 )
                            {
                                go = Instantiate(trees[0], treePos, Quaternion.identity);
                                go.transform.localScale *= 1 + Mathf.PerlinNoise(x * 0.005f, z * 0.005f) * 2.0f;
                            }
                            else if (placedTree2 < 0.5 && UnityEngine.Random.Range(0, 10) < 3)
                            {
                                go = Instantiate(trees[1], treePos, Quaternion.identity);
                                go.transform.localScale *= 1 + Mathf.PerlinNoise(x * 0.004f, z * 0.004f) * 3.0f;
                            }
                            else if (placedTree3 < 0.3 && UnityEngine.Random.Range(0, 10) < 5)
                            {
                                go = Instantiate(trees[2], treePos, Quaternion.identity);
                                go.transform.localScale *= 1 + Mathf.PerlinNoise(x * 0.002f, z * 0.002f);
                            }
                            else if (placedTree4 < 0.6 && UnityEngine.Random.Range(0, 10) < 1)
                            {
                                go = Instantiate(trees[3], treePos, Quaternion.identity);
                                go.transform.localScale *= 1 + Mathf.PerlinNoise(x * 0.006f, z * 0.006f) * 4.0f;
                            }
                        }
                    }*/
                   go = Instantiate(fillers[modelId], mapKey, Quaternion.identity);
                   mat = go.GetComponent<MeshRenderer>().material;
                    MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
                    foreach (MeshFilter mf in meshFilters)
                    {
                        meshes.Add(mf.mesh);
                        mPositions.Add(go.transform.position);
                    }
                    DestroyImmediate(go);
                }

            }
            
           
        }
        GetCenterZ = maxDimensions.z - Math.Abs(minDimensions.z - 10) + 10;
        GetCenterX = maxDimensions.x - Math.Abs(minDimensions.x - 10) + 10;
  
        if (meshes.Count > 0)
        {
            GameObject combinedMesh = new GameObject("Combined Mesh");
            List<List<Mesh>> allMeshes = MeshTools.Split(meshes, 1000);
            List<List<Vector3>> allPositions = MeshTools.Split(mPositions, 1000);
            
            for (int i = 0; i < allMeshes.Count; i++)
            {
                GameObject subMesh = new GameObject("SubMesh");
                subMesh.layer = 7;
                subMesh.AddComponent<MeshCollider>();
                subMesh.GetComponent<MeshCollider>().sharedMesh = m;
                subMesh.transform.parent = combinedMesh.transform;
                MeshRenderer mr = subMesh.AddComponent<MeshRenderer>();
                mr.material = mat;
                MeshFilter mf = subMesh.AddComponent<MeshFilter>();
                mf.mesh = MeshTools.MergeMeshes(allMeshes[i], allPositions[i]);
            }
        }
    }

    public Mesh m;
    public int GetL
    {
        get
        {
            return maxDimensions.z - minDimensions.z + 20;
        }
    }

    public int Getl
    {
        get
        {
            return maxDimensions.x - minDimensions.x + 20;
        }
    }
    public int GetCenterZ
    {
        get => centerZ;
        set => centerZ = value;
    }

    public int GetCenterX
    {
        get => centerX;
        set => centerX = value;
    }
}


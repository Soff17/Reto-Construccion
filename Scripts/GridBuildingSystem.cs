using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem current;

    public GridLayout gridLayout;
    public Tilemap MainTilemap;
    public Tilemap TempTilemap;

    private static Dictionary<TileType, TileBase> tileBases = new Dictionary<TileType, TileBase>();

    private Building temp;
    private Vector3 prevPos;
    private BoundsInt prevArea;

    #region Unity Methods
    private void Awake()
    {
        current = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        string tilePath = @"Tiles\";
        tileBases.Add(TileType.Empty, null);
        tileBases.Add(TileType.Green, Resources.Load<TileBase>(tilePath + "green"));
        tileBases.Add(TileType.Red, Resources.Load<TileBase>(tilePath + "red"));
        tileBases.Add(TileType.White, Resources.Load<TileBase>(tilePath + "white"));

    }

    // Update is called once per frame
    private void Update()
    {
        if (!temp)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject(0))
            {
                return;
            }

            if (!temp.Placed)
            {
                Vector2 touchPos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPos = gridLayout.LocalToCell((Vector3)touchPos);

                if (prevPos != (Vector3)cellPos)
                {
                    temp.transform.localPosition = gridLayout.CellToLocalInterpolated(cellPosition: (Vector3)cellPos
                        + new Vector3(x: .5f, y: .5f, z: 0f));
                    prevPos = (Vector3)cellPos;
                    FollowBuilding();
                }

            }

        }
    }

    #endregion

    #region Tilemap Managment

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var Vector3Int in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(Vector3Int.x,Vector3Int.y,0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }

        return array;

    }

    private static void FillTiles(TileBase[] arr,TileType type)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = tileBases[type];
        }

    }

    private static void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap)
    {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }

    #endregion

    #region Building Placement

    public void InitializeWithBuilding(GameObject building)
    {
        temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();
        FollowBuilding();
    }

    private void ClearArea()
    {
        TileBase[] toClear = new TileBase[prevArea.size.x * prevArea.size.y * prevArea.size.z];
        FillTiles(toClear, TileType.Empty);
        TempTilemap.SetTilesBlock(prevArea, toClear);
    }

    private void FollowBuilding()
    {
        ClearArea();

        temp.area.position = gridLayout.WorldToCell(temp.gameObject.transform.position);
        BoundsInt buildingArea = temp.area;

        TileBase[] baseArray = GetTilesBlock(buildingArea, MainTilemap);

        int size = baseArray.Length;
        TileBase[] tileArray = new TileBase[size];

        for (int i = 0;i<baseArray.Length;i++)
        {
            if (baseArray[i] == tileBases[TileType.White])
            {
                tileArray[i] = tileBases[TileType.Green];
            }
            else
            {
                FillTiles(tileArray, TileType.Red);
                break;
            }
        }

        TempTilemap.SetTilesBlock(buildingArea, tileArray);
        prevArea = buildingArea;

    }

    #endregion 


}

public enum TileType
{
    Empty,
    White,
    Red,
    Green
}

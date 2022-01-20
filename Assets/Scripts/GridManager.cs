using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] int width, height;
    [SerializeField] GameObject tilePrefab;
    Dictionary<Vector2, GameObject> tiles;

    void Start() 
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        tiles = new Dictionary<Vector2, GameObject>();
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                var spawnedTile = Instantiate(tilePrefab, new Vector3(i, j), Quaternion.identity, transform);
                spawnedTile.name = $"Tile {i} {j}";

                var isOffset = (i % 2 == 0 && j % 2 != 0) || (i % 2 != 0 && j % 2 == 0);
                spawnedTile.GetComponent<TileGrid>().Init(isOffset);

                tiles[new Vector2(i, j)] = spawnedTile;
            }
        }

        Camera.main.transform.position = new Vector3((float) width/2 - 0.5f, (float) height/2 - 0.5f, -10f);
    }

    public GameObject GetTileAtPosition(Vector2 pos)
    {
        if (tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }

        return null;
    } 
}

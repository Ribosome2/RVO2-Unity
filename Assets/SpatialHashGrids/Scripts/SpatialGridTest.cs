using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace SpatialGrids
{
    public class SpatialGridTest:MonoBehaviour
    {
        public Bounds gridBounds=new Bounds(Vector3.zero,new Vector3(100,100,1));
        public int rows =100;
        public int col =100;

        public SpatialHashGrid testGrid;
        public int moveClientNum = 60;
        public GameObject PlayerGo;
        public Vector3 PlayerSize=new Vector3(3,3,1);

        class  TestUnit
        {
            public GameObject go;
            public GridClient gridClient;
        }

        private List<TestUnit> _testUnits = new List<TestUnit>();
        public GameObject moveClientPrefab;
        public bool FindNearEveryFrame = true;
        public bool FindWithForce = true; //暴力查找的方式
        public Text txtInfo;
        private void Start()
        {
            testGrid=new SpatialHashGrid(gridBounds,rows,col);
            for (int i = 0; i < moveClientNum; i++)
            {
                var go = Instantiate(moveClientPrefab);
                go.transform.position = new Vector3(Random.Range(gridBounds.min.x, gridBounds.max.x),
                    Random.Range(gridBounds.min.y, gridBounds.max.y));

                GridClient client = testGrid.NewClient(go.transform.position, new Vector2(1, 1));
                _testUnits.Add(new TestUnit()
                {
                    go=go,
                    gridClient = client
                });
               
            }
        }

        private void Update()
        {
            if (PlayerGo)
            {
                int gridX;int gridY;
                testGrid._GetCellIndex(PlayerGo.transform.position.x, PlayerGo.transform.position.y, out gridX,
                    out gridY);
                int key = testGrid._Key(gridX, gridY);
                txtInfo.text = "PlayerAtGrid x "+gridX+" y "+gridY+" key "+key;
            }
            
            if (FindNearEveryFrame)
            {
                FindNearTest();
            }

            
        }

        [ContextMenu("FindNear")]
        void FindNearTest()
        {
            if (PlayerGo == null)
            {
                return;
            }
            var pos = PlayerGo.transform.position;
            var searchBounds = new Bounds(pos, PlayerSize);
            result = new List<GridClient>();
            if (FindWithForce)
            {
                foreach (var testUnit in _testUnits)
                {
                    if (searchBounds.Contains(testUnit.gridClient.position))
                    {
                        result.Add(testUnit.gridClient);
                    }
                }
            }
            else
            {
                
                var rawResult =testGrid.FindNear(pos, searchBounds);
                //FindNear找到的只是包含的大格子，还需要对粗略结果进行判断
                foreach (var gridClient in rawResult)
                {
                    if (searchBounds.Contains(gridClient.position))
                    {
                        result.Add(gridClient);
                    }
                }
            }
         
            // Debug.Log("findCount "+ result.Count);
        }

        private List<GridClient> result = new List<GridClient>();
        private void OnDrawGizmos()
        {
            var prevColor = Gizmos.color;
            
            DrawGrid();

            // DrawGridCells();

            Gizmos.color=Color.cyan;
            Gizmos.DrawWireCube(PlayerGo.transform.position,PlayerSize);
            Gizmos.color=Color.yellow;
            foreach (var gridClient in result)
            {
                Gizmos.DrawWireCube(gridClient.position,new Vector3(gridClient.dimensions.x,gridClient.dimensions.y,1));
            }

          

            Gizmos.color = prevColor;
        }

        private void DrawGridCells()
        {
            if (testGrid == null)
            {
                return;
            }
            foreach (var kv in testGrid.cells)
            {
                int gridIndex = kv.Key;
                foreach (var gridClient in kv.Value)
                {
                    Handles.Label(gridClient.position,"GridIndex"+gridIndex.ToString());
                }
            }
        }

        private void DrawGrid()
        {
            //draw grids
            var yGap = gridBounds.size.y / rows;
            var xGap = gridBounds.size.x / rows;
            for (int i = 0; i < rows+1; i++)
            {
                var yPos = gridBounds.min.y + yGap * i;
                Gizmos.DrawLine(new Vector3(gridBounds.min.x,yPos,0),
                    new Vector3(gridBounds.max.x,yPos,0));
            }
            
            //
            for (int i = 0; i < col+1; i++)
            {
                var xPos = gridBounds.min.x + xGap * i;
                Gizmos.DrawLine(new Vector3(xPos,gridBounds.min.y,0),
                    new Vector3(xPos,gridBounds.max.y,0));
            }
        }
    }
}
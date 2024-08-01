using System;
using System.Collections.Generic;
using UnityEngine;
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
        private void Start()
        {
            testGrid=new SpatialHashGrid(gridBounds,new Vector2(rows,col));
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
            if (FindWithForce)
            {
                result = new HashSet<GridClient>();
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
                
                result=testGrid.FindNear(pos, searchBounds);
            }
         
            // Debug.Log("findCount "+ result.Count);
        }

        private HashSet<GridClient> result = new HashSet<GridClient>();
        private void OnDrawGizmos()
        {
            var prevColor = Gizmos.color;
            Gizmos.color=Color.cyan;
            Gizmos.DrawWireCube(PlayerGo.transform.position,PlayerSize);
            foreach (var gridClient in result)
            {
                Gizmos.DrawWireCube(gridClient.position,new Vector3(gridClient.dimensions.x,gridClient.dimensions.y,1));
            }

            Gizmos.color = prevColor;
        }
    }
}
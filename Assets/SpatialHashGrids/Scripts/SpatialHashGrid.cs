using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGrids
{
	

	public class GridClient
	{
		public Vector2 position;   
		public Vector2 dimensions; //width and height
		public int[] gridRange=new int[4];  //占的格子索引范围，0，1是x的区间，2 3是y的区间
		public int queryId = 0;
	}

	public class SpatialHashGrid
	{
		public Bounds Bounds;
		public int DimensionsRow; //划分为几行几列
		public int DimensionsCol; //划分为几行几列
		public Dictionary<int, List<GridClient>> cells=new Dictionary<int, List<GridClient>>();
		private int _queryId = 0;

		public SpatialHashGrid(Bounds bounds, int row,int col)
		{
			this.Bounds = bounds;
			this.DimensionsRow = row;
			this.DimensionsCol = col;
		}

		public GridClient NewClient(Vector2 position, Vector2 dimensions)
		{
			var client = new GridClient()
			{
				position = position,
				dimensions = dimensions,
			};

			this._Insert(client);
			
			return client;
		}

		private void _Insert(GridClient client)
		{
			float x = client.position.x;
			float y = client.position.y;

			float w = client.dimensions.x;
			float h = client.dimensions.y;
			int minX;
			int minY;
			int maxX;
			int maxY;
			this._GetCellIndex(x - w / 2, y - h / 2,out  minX,out  minY ); //用Client左下角取到的做索引
			this._GetCellIndex(x + w / 2, y + h / 2,out maxX,out maxY); //用Client右上角取到的索引

			for (int xIndex = minX; xIndex <= maxX; xIndex++)
			{
				for (int yIndex = minY; yIndex <= maxY; yIndex++)
				{
					var key = this._Key(xIndex, yIndex);
					// Debug.Log("x "+x+" y "+y+"  xIndex  "+xIndex+" yIndex"+yIndex + "key"+key);
					if (cells.ContainsKey(key) == false)
					{
						cells[key] = new List<GridClient>();
					}
					this.cells[key].Add(client);
				}
			}

			client.gridRange[0] = minX;
			client.gridRange[1] = maxX;
			client.gridRange[2] = minX;
			client.gridRange[3] = maxY;

		}

		public int _Key(int xIndex, int yIndex)
		{
			return yIndex*this.DimensionsCol + xIndex;
		}

		public void _GetCellIndex(float x, float y,out int xIndex,out int yIndex)
		{
			var factorX = (x - this.Bounds.min.x) / this.Bounds.size.x;
			xIndex = Mathf.FloorToInt(factorX * this.DimensionsCol);
			
			var factorY = (y - this.Bounds.min.y) / this.Bounds.size.y;
			yIndex = Mathf.FloorToInt(factorY * this.DimensionsRow);
		}
		

		public List<GridClient> FindNear(Vector2 position, Bounds bound)
		{
			float x = position.x;
			float y = position.y;

			float w = bound.size.x;
			float h = bound.size.y;
			int minX;
			int minY;
			int maxX;
			int maxY;
			this._GetCellIndex(x - w / 2, y - h / 2, out minX, out minY); //用Client左下角取到的做索引
			this._GetCellIndex(x + w / 2, y + h / 2, out maxX, out maxY); //用Client右上角取到的索引
			List<GridClient> clients = new List<GridClient>();
			this._queryId++;

			for (int xIndex = minX; xIndex <= maxX; xIndex++)
			{
				for (int yIndex = minY; yIndex <= maxY; yIndex++)
				{
					var key = this._Key(xIndex, yIndex);
					if (cells.ContainsKey(key))
					{
						foreach (var gridClient in this.cells[key])
						{
							if (gridClient.queryId != _queryId) //额外用一个每次查询都会变化的id来避免了使用set来避免重复
							{
								gridClient.queryId = _queryId;
								clients.Add(gridClient);
							}
						}
					}
				}
			}

			return clients;
		}

		public void UpdateClient(GridClient client)
		{
			//todo:在执行更新前，先判断单曲坐标是否跟之前的坐标算出来的结果 格子索引是不是一致的，如果一致就没有必要执行更新了
			
			this.RemoveClient(client);
			this._Insert(client);
		}

		private void RemoveClient(GridClient client)
		{
			
			for (int xIndex = client.gridRange[0]; xIndex < client.gridRange[1]; xIndex++)
			{
				for (int yIndex = client.gridRange[2]; yIndex < client.gridRange[3]; yIndex++)
				{
					var key = this._Key(xIndex, yIndex);
					if (cells.ContainsKey(key))
					{
						cells[key].Remove(client);
					}
				}
			}
		}
	}
}



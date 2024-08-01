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
	}

	public class SpatialHashGrid
	{
		public Bounds Bounds;
		public Vector2 Dimensions; //划分为几行几列
		public Dictionary<string, List<GridClient>> cells=new Dictionary<string, List<GridClient>>();

		public SpatialHashGrid(Bounds bounds, Vector2 dimensions)
		{
			this.Bounds = bounds;
			this.Dimensions = dimensions;
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

			for (int xIndex = minX; xIndex < maxX; xIndex++)
			{
				for (int yIndex = minY; yIndex < maxY; yIndex++)
				{
					var key = this._Key(xIndex, yIndex);
					if (cells.ContainsKey(key) == false)
					{
						cells[key] = new List<GridClient>();
					}
					this.cells[key].Add(client);
				}
			}

			client.gridRange[0] = minX;
			client.gridRange[1] = maxX;
			client.gridRange[2] = maxY;
			client.gridRange[3] = maxY;

		}

		private string _Key(int xIndex, int yIndex)
		{
			return xIndex + "." + yIndex;
		}

		private void _GetCellIndex(float x, float y,out int xIndex,out int yIndex)
		{
			var factorX = (x - this.Bounds.min.x) / this.Bounds.size.x;
			xIndex = Mathf.FloorToInt(factorX * this.Dimensions.x);
			
			var factorY = (y - this.Bounds.min.y) / this.Bounds.size.y;
			yIndex = Mathf.FloorToInt(factorY * this.Dimensions.y);
		}

		public HashSet<GridClient> FindNear(Vector2 position, Bounds bound)
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
			HashSet<GridClient> clients = new HashSet<GridClient>();

			for (int xIndex = minX; xIndex < maxX; xIndex++)
			{
				for (int yIndex = minY; yIndex < maxY; yIndex++)
				{
					var key = this._Key(xIndex, yIndex);
					if (cells.ContainsKey(key))
					{
						foreach (var gridClient in this.cells[key])
						{
							clients.Add(gridClient);
						}
					}
				}
			}

			return clients;
		}

		public void UpdateClient(GridClient client)
		{
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



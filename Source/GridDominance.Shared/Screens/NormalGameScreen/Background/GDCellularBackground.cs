﻿using GridDominance.Shared.Resources;
using GridDominance.Shared.Screens.NormalGameScreen.Entities;
using Microsoft.Xna.Framework;
using MonoSAMFramework.Portable.BatchRenderer;
using MonoSAMFramework.Portable.ColorHelper;
using MonoSAMFramework.Portable.DebugTools;
using MonoSAMFramework.Portable.Input;
using MonoSAMFramework.Portable.GameMath;
using MonoSAMFramework.Portable.Screens.Background;
using GridDominance.Shared.Screens.NormalGameScreen.Fractions;
using MonoSAMFramework.Portable.GameMath.Geometry;
using MonoSAMFramework.Portable.RenderHelper;
using MonoSAMFramework.Portable.Screens;
using MonoSAMFramework.Portable.Extensions;
using GridDominance.Shared.Screens.ScreenGame;

namespace GridDominance.Shared.Screens.NormalGameScreen.Background
{
	internal sealed class GridCellMembership
	{
		public Fraction Fraction = null;

		public float PowerCurr = 0;
		public float PowerNext = 0;

		public Cannon SpawnSource = null;
		public int SourceDistance = GDCellularBackground.SRC_DIST_INF;

		public bool BlockNorth = false;
		public bool BlockEast  = false;
		public bool BlockSouth = false;
		public bool BlockWest  = false;

		public bool IsNeutralDraining = false;

		public float DifficultyMod = 1f;
	}

	class GDCellularBackground : GameBackground, IGDGridBackground
	{
		private const int TILE_COUNT_X = GDConstants.GRID_WIDTH  + 2 * MAX_EXTENSION;
		private const int TILE_COUNT_Y = GDConstants.GRID_HEIGHT + 2 * MAX_EXTENSION;

		private const int MAX_EXTENSION = 2;
		
		public const int SRC_DIST_INF = 12;

		private const float MIN_TRANSFER_POWER     = 0.25f;
		private const float MIN_NEUTRALIZE_POWER   = 0.55f;
		
		private const float RANDOM_MOD_FACTOR      = 0.55f;

		private const float SPAWNCELL_REGENERATION = 2.50f; // power per second
		private const float CELL_ATTACK_SPEED      = 1.05f; // power per second
		private const float CELL_OCCUPY_SPEED      = 0.35f; // power per second
		private const float CELL_DECAY_SPEED       = 0.70f; // power per second
		private const float CELL_NEUTRALIZE_SPEED  = 0.20f; // power per second

		private readonly GridCellMembership[,] _grid = new GridCellMembership[TILE_COUNT_X, TILE_COUNT_Y];
		private readonly FRectangle[,] _rects = new FRectangle[TILE_COUNT_X, TILE_COUNT_Y];
		
		public GDCellularBackground(GDGameScreen scrn) : base(scrn)
		{
			Initialize();
		}

		private void Initialize()
		{
			for (int x = 0; x < TILE_COUNT_X; x++)
			{
				for (int y = 0; y < TILE_COUNT_Y; y++)
				{
					_grid[x, y] = new GridCellMembership();
					_grid[x, y].DifficultyMod = 1 + FloatMath.Sin(FloatMath.GetRandom()*FloatMath.RAD_POS_360) * RANDOM_MOD_FACTOR;
					if (x == 0) _grid[x, y].BlockWest = true;
					if (y == 0) _grid[x, y].BlockNorth = true;
					if (x == TILE_COUNT_X-1) _grid[x, y].BlockEast = true;
					if (y == TILE_COUNT_Y-1) _grid[x, y].BlockSouth = true;
				}
			}

			for (int ox = 0; ox < TILE_COUNT_X; ox++)
			{
				for (int oy = 0; oy < TILE_COUNT_Y; oy++)
				{
					var x = ox - MAX_EXTENSION; // array coords -> real coords
					var y = oy - MAX_EXTENSION;

					_rects[ox, oy] = new FRectangle(
						x * GDConstants.TILE_WIDTH, 
						y * GDConstants.TILE_WIDTH, 
						1 * GDConstants.TILE_WIDTH, 
						1 * GDConstants.TILE_WIDTH);
				}
			}
		}

		public override void Draw(IBatchRenderer sbatch)
		{
			int extensionX = MathHelper.Min(MAX_EXTENSION, FloatMath.Ceiling(VAdapter.VirtualGuaranteedBoundingsOffsetX / GDConstants.TILE_WIDTH));
			int extensionY = MathHelper.Min(MAX_EXTENSION, FloatMath.Ceiling(VAdapter.VirtualGuaranteedBoundingsOffsetY / GDConstants.TILE_WIDTH));

			for (int ox = -extensionX; ox < GDConstants.GRID_WIDTH + extensionX; ox++)
			{
				for (int oy = -extensionY; oy < GDConstants.GRID_HEIGHT + extensionY; oy++)
				{
					var x = ox + MAX_EXTENSION; // real coords -> array coords
					var y = oy + MAX_EXTENSION;

					if (x < 0) continue;
					if (y < 0) continue;
					if (x >= TILE_COUNT_X) continue;
					if (y >= TILE_COUNT_Y) continue;

					var color = GetGridColor(x, y);

					sbatch.DrawStretched(Textures.TexPixel, _rects[x, y], color);
					sbatch.DrawStretched(Textures.TexTileBorder, _rects[x, y], Color.White);

#if DEBUG
					if (DebugSettings.Get("DebugBackground"))
					{

						var tx = _rects[x, y].X + 8;
						var ty = _rects[x, y].Y + 8;

						sbatch.DrawString(
							Textures.DebugFontSmall,
							string.Format("({4}|{5})\n{0,2}: {1:000}\n[{2}]{3}", _grid[x, y].Fraction?.ToString() ?? "##", _grid[x, y].PowerCurr * 100, _grid[x, y].SourceDistance, _grid[x, y].IsNeutralDraining ? "D" : "", x, y),
							new Vector2(tx, ty),
							_grid[x, y].Fraction?.Color ?? Color.Black);

						if (_grid[x, y].SpawnSource != null)
							SimpleRenderHelper.DrawCross(sbatch, _rects[x, y], _grid[x, y].SpawnSource.Fraction.Color * 0.5f, 2);

						var v4tl = new Vector2(+5,+5);
						var v4tr = new Vector2(-5,+5);
						var v4br = new Vector2(-5,-5);
						var v4bl = new Vector2(+5,-5);

						if (_grid[x, y].BlockNorth)
							sbatch.DrawLine(_rects[x, y].TopLeft + v4tl, _rects[x, y].TopRight + v4tr, Color.Yellow * 0.6f, 4);

						if (_grid[x, y].BlockEast)
							sbatch.DrawLine(_rects[x, y].TopRight + v4tr, _rects[x, y].BottomRight + v4br, Color.Yellow * 0.6f, 4);

						if (_grid[x, y].BlockSouth)
							sbatch.DrawLine(_rects[x, y].BottomRight + v4br, _rects[x, y].BottomLeft + v4bl, Color.Yellow * 0.6f, 4);

						if (_grid[x, y].BlockWest)
							sbatch.DrawLine(_rects[x, y].BottomLeft + v4bl, _rects[x, y].TopLeft + v4tl, Color.Yellow * 0.6f, 4);

					}
#endif
				}
			}
		}

		public override void Update(SAMTime gameTime, InputState state)
		{
			for (int x = 0; x < TILE_COUNT_X; x++)
			{
				for (int y = 0; y < TILE_COUNT_Y; y++)
				{
					UpdateCell(x, y, gameTime);
					
					if (_grid[x, y].SpawnSource != null) UpdateSpawnCell(x, y, gameTime);
				}
			}

			for (int x = 0; x < TILE_COUNT_X; x++)
			{
				for (int y = 0; y < TILE_COUNT_Y; y++)
				{
					_grid[x, y].PowerCurr = _grid[x, y].PowerNext;
				}
			}
		}

		private void UpdateCell(int x, int y, SAMTime gameTime)
		{
			GridCellMembership me = _grid[x, y];

			int totalNeighbourCount = 0;
			int newSourceDistance = SRC_DIST_INF;

			int occupyCount = 0;
			GridCellMembership support = null;
			GridCellMembership attack = null;

			if (!_grid[x, y].BlockNorth) AnalyseCell(me, x + 0, y - 1, ref totalNeighbourCount, ref newSourceDistance, ref support, ref occupyCount, ref attack);
			if (!_grid[x, y].BlockEast)  AnalyseCell(me, x + 1, y + 0, ref totalNeighbourCount, ref newSourceDistance, ref support, ref occupyCount, ref attack);
			if (!_grid[x, y].BlockSouth) AnalyseCell(me, x + 0, y + 1, ref totalNeighbourCount, ref newSourceDistance, ref support, ref occupyCount, ref attack);
			if (!_grid[x, y].BlockWest)  AnalyseCell(me, x - 1, y + 0, ref totalNeighbourCount, ref newSourceDistance, ref support, ref occupyCount, ref attack);

			if (totalNeighbourCount == 0)
			{
				// pathological Case
				me.SourceDistance = SRC_DIST_INF;
				me.Fraction = null;
				me.PowerNext = 0f;
				return;
			}

			// [1] update SourceDistance 

			if (newSourceDistance <= _grid[x, y].SourceDistance)
				_grid[x, y].SourceDistance = newSourceDistance;
			else
				_grid[x, y].SourceDistance = SRC_DIST_INF;


			// [2] update Power 

			if (support == null && attack == null)
			{
				if (occupyCount < totalNeighbourCount)
				{
					// border of unsupported island
					DrainPower(x, y, gameTime.ElapsedSeconds * CELL_DECAY_SPEED);
				}
				else
				{
					// center of unsupported island
				}
			}
			else if (support != null && attack == null)
			{
				//improv agains none
				GainPower(support.Fraction, x, y, gameTime.ElapsedSeconds * CELL_OCCUPY_SPEED);
			}
			else if (support != null && attack != null && attack.SourceDistance > support.SourceDistance)
			{
				//improv against enemy
				GainPower(support.Fraction, x, y, gameTime.ElapsedSeconds * CELL_ATTACK_SPEED);
			}
			else if (support == null && attack != null)
			{
				//loose with no support
				GainPower(attack.Fraction, x, y, gameTime.ElapsedSeconds * CELL_DECAY_SPEED);
			}
			else if (support != null && attack != null && attack.SourceDistance < support.SourceDistance)
			{
				//loose with against enemy
				GainPower(attack.Fraction, x, y, gameTime.ElapsedSeconds * CELL_ATTACK_SPEED);
			}
			else if (support != null && attack != null && attack.SourceDistance == support.SourceDistance)
			{
				//contested cell
				if (_grid[x, y].PowerCurr > MIN_NEUTRALIZE_POWER || _grid[x, y].IsNeutralDraining)
				{
					// drain
					_grid[x, y].IsNeutralDraining = true;
					GainPower(attack.Fraction, x, y, gameTime.ElapsedSeconds * CELL_NEUTRALIZE_SPEED);
				}
				else
				{
					//gain
					GainPower(support.Fraction, x, y, gameTime.ElapsedSeconds * CELL_NEUTRALIZE_SPEED);
				}
			}
		}

		private void AnalyseCell(GridCellMembership me, int x, int y, ref int totalNeighbourCount, ref int newSourceDistance, ref GridCellMembership support, ref int occupyCount, ref GridCellMembership attack)
		{
			totalNeighbourCount++;

			if (me.Fraction != null && _grid[x, y].Fraction == me.Fraction && _grid[x, y].SourceDistance + 1 < newSourceDistance)
				newSourceDistance = _grid[x, y].SourceDistance + 1;

			if (_grid[x, y].Fraction != null && me.Fraction == _grid[x, y].Fraction && _grid[x, y].SourceDistance < SRC_DIST_INF && _grid[x, y].PowerCurr > MIN_TRANSFER_POWER && (support == null || support.SourceDistance > _grid[x, y].SourceDistance))
				support = _grid[x, y];

			if (_grid[x, y].Fraction != null && !_grid[x, y].Fraction.IsNeutral)
				occupyCount++;

			if (_grid[x, y].Fraction != null && me.Fraction != _grid[x, y].Fraction && _grid[x, y].SourceDistance < SRC_DIST_INF && _grid[x, y].PowerCurr > MIN_TRANSFER_POWER && (attack == null || attack.SourceDistance > _grid[x, y].SourceDistance))
				attack = _grid[x, y];
		}

		private void UpdateSpawnCell(int x, int y, SAMTime gameTime)
		{
			if (_grid[x, y].PowerCurr < 1f || _grid[x, y].Fraction != _grid[x, y].SpawnSource.Fraction)
			{
				// Regenerate spawn cells
				GainPower(_grid[x, y].SpawnSource.Fraction, x, y, gameTime.ElapsedSeconds * SPAWNCELL_REGENERATION);

				if (_grid[x, y].Fraction != null && _grid[x, y].Fraction.IsNeutral) _grid[x, y].PowerNext = 0f;

				if (_grid[x, y].PowerNext < 1f) _grid[x, y].SourceDistance = SRC_DIST_INF;
			}
			else
			{
				if (_grid[x, y].SpawnSource.CannonHealth.TargetValue < 0.95f)
					_grid[x, y].SourceDistance = SRC_DIST_INF;
				else
					_grid[x, y].SourceDistance = 0;
			}
		}

		private void GainPower(Fraction f, int x, int y, float power)
		{
			power *=_grid[x, y].DifficultyMod;

			if (_grid[x, y].Fraction == null || _grid[x, y].Fraction.IsNeutral)
			{
				_grid[x, y].Fraction = f;
				_grid[x, y].PowerNext = power;
			}
			else if (f == _grid[x, y].Fraction)
			{
				_grid[x, y].PowerNext += power;
				if (_grid[x, y].PowerNext > 1)
				{
					_grid[x, y].PowerNext = 1;
					_grid[x, y].IsNeutralDraining = false;
				}
			}
			else
			{
				_grid[x, y].PowerNext -= power;
				if (_grid[x, y].PowerNext < 0)
				{
					_grid[x, y].Fraction = f;
					_grid[x, y].PowerNext = -_grid[x, y].PowerNext;
					_grid[x, y].IsNeutralDraining = false;
				}
			}
		}

		private void DrainPower(int x, int y, float power)
		{
			if (_grid[x, y].Fraction == null || _grid[x, y].Fraction.IsNeutral) return;

			power *= _grid[x, y].DifficultyMod;

			_grid[x, y].PowerNext -= power;
			if (_grid[x, y].PowerNext < 0)
			{
				_grid[x, y].Fraction = null;
				_grid[x, y].PowerNext = 0;
				_grid[x, y].IsNeutralDraining = false;
			}
		}

		private Color GetGridColor(int x, int y)
		{
			if (x < 0) return GetGridColor(0, y);
			if (y < 0) return GetGridColor(x, 0);
			if (x >= TILE_COUNT_X) return GetGridColor(TILE_COUNT_X-1, y);
			if (y >= TILE_COUNT_Y) return GetGridColor(x, TILE_COUNT_Y-1);

			if (_grid[x, y].Fraction == null) return FlatColors.Background;

			return ColorMath.Blend(FlatColors.Background, _grid[x, y].Fraction.BackgroundColor, _grid[x, y].PowerCurr);
		}

		public void RegisterSpawn(Cannon cannon, FCircle circle)
		{
			for (int ox = (int)((circle.X-circle.Radius) / GDConstants.TILE_WIDTH); ox <= (int)((circle.X + circle.Radius) / GDConstants.TILE_WIDTH); ox++)
			{
				for (int oy = (int)((circle.Y - circle.Radius) / GDConstants.TILE_WIDTH); oy <= (int)((circle.Y + circle.Radius) / GDConstants.TILE_WIDTH); oy++)
				{
					int x = ox + MAX_EXTENSION; // real coords -> array coords
					int y = oy + MAX_EXTENSION;

					if (x < 0) continue;
					if (y < 0) continue;
					if (x >= TILE_COUNT_X) continue;
					if (y >= TILE_COUNT_Y) continue;
					
					if (!_rects[x, y].Intersects(circle)) continue;

					_grid[x, y].SpawnSource = cannon;
					_grid[x, y].PowerCurr = 1f;
				}
			}
		}

		public void RegisterBlockedLine(Vector2 start, Vector2 end)
		{
			var delta = end - start;
			var angle = delta.ToAngle();

			if ((angle + FloatMath.RAD_POS_045)% FloatMath.RAD_POS_180 < FloatMath.RAD_POS_090)
			{
				// HORZ

				if (start.X > end.X){ var tmp = start; start = end; end = tmp; }

				int firstX = FloatMath.Ceiling((start.X - 8f) / GDConstants.TILE_WIDTH);
				int lastX = FloatMath.Floor((end.X + 8f) / GDConstants.TILE_WIDTH);

				int lastoy = FloatMath.Round((start.Y + (end.Y - start.Y) * ((firstX * GDConstants.TILE_WIDTH - start.X) / (end.X - start.X))) / GDConstants.TILE_WIDTH);

				for (int ox = firstX + 1; ox <= lastX; ox++)
				{
					var oy = FloatMath.Round((start.Y + (end.Y - start.Y) * ((ox * GDConstants.TILE_WIDTH - start.X) / (end.X - start.X))) / GDConstants.TILE_WIDTH);

					if (oy != lastoy)
					{
						BlockSegmentVert(ox + MAX_EXTENSION-1, lastoy + MAX_EXTENSION, oy + MAX_EXTENSION);
					}

					BlockSegmentHorz(oy + MAX_EXTENSION, ox - 1 + MAX_EXTENSION, ox + MAX_EXTENSION);

					lastoy = oy;
				}
			}
			else
			{
				// VERT

				if (start.Y > end.Y) { var tmp = start; start = end; end = tmp; }

				int firstY = FloatMath.Ceiling((start.Y - 8f) / GDConstants.TILE_WIDTH);
				int lastY = FloatMath.Floor((end.Y + 8f) / GDConstants.TILE_WIDTH);

				int lastox = FloatMath.Round((start.X + (end.X - start.X) * ((firstY - start.Y) / (end.Y - start.Y))) / GDConstants.TILE_WIDTH);

				for (int oy = firstY + 1; oy <= lastY; oy++)
				{
					var ox = FloatMath.Round((start.X + (end.X - start.X) * ((oy - start.Y) / (end.Y - start.Y))) / GDConstants.TILE_WIDTH);

					if (ox != lastox)
					{
						BlockSegmentHorz(oy + MAX_EXTENSION, lastox + MAX_EXTENSION, ox + MAX_EXTENSION);
					}

					BlockSegmentVert(ox + MAX_EXTENSION, oy - 1 + MAX_EXTENSION, oy + MAX_EXTENSION);

					lastox = ox;
				}
			}

		}

		private void BlockSegmentHorz(int y, int x1, int x2)
		{
			for (int x = x1; x < x2; x++)
			{
				if (x >= 0 && y >= 0 && x < TILE_COUNT_X && y < TILE_COUNT_Y)
				{
					// bot
					_grid[x, y].BlockNorth = true;
				}

				if (x >= 0 && y - 1 >= 0 && x < TILE_COUNT_X && y - 1 < TILE_COUNT_Y)
				{
					// bot
					_grid[x, y - 1].BlockSouth = true;
				}
			}
		}

		private void BlockSegmentVert(int x, int y1, int y2)
		{
			for (int y = y1; y < y2; y++)
			{
				if (x >= 0 && y >= 0 && x < TILE_COUNT_X && y < TILE_COUNT_Y)
				{
					// bot
					_grid[x, y].BlockWest = true;
				}

				if (x - 1 >= 0 && y >= 0 && x - 1 < TILE_COUNT_X && y < TILE_COUNT_Y)
				{
					// bot
					_grid[x - 1, y].BlockEast = true;
				}
			}
		}

		public void RegisterBlockedCircle(FCircle circle)
		{
			for (int ox = (int)((circle.X - circle.Radius) / GDConstants.TILE_WIDTH); ox <= (int)((circle.X + circle.Radius) / GDConstants.TILE_WIDTH); ox++)
			{
				for (int oy = (int)((circle.Y - circle.Radius) / GDConstants.TILE_WIDTH); oy <= (int)((circle.Y + circle.Radius) / GDConstants.TILE_WIDTH); oy++)
				{
					int x = ox + MAX_EXTENSION; // real coords -> array coords
					int y = oy + MAX_EXTENSION;

					if (x < 0) continue;
					if (y < 0) continue;
					if (x >= TILE_COUNT_X) continue;
					if (y >= TILE_COUNT_Y) continue;

					if (circle.Contains(_rects[x, y].TopLeft, 0.5f) && circle.Contains(_rects[x, y].TopRight, 0.5f))
						_grid[x, y].BlockNorth = true;

					if (circle.Contains(_rects[x, y].TopRight, 0.5f) && circle.Contains(_rects[x, y].BottomRight, 0.5f))
						_grid[x, y].BlockEast = true;

					if (circle.Contains(_rects[x, y].BottomRight, 0.5f) && circle.Contains(_rects[x, y].BottomLeft, 0.5f))
						_grid[x, y].BlockSouth = true;

					if (circle.Contains(_rects[x, y].BottomLeft, 0.5f) && circle.Contains(_rects[x, y].TopLeft, 0.5f))
						_grid[x, y].BlockWest = true;
				}
			}
		}

		public void RegisterBlockedBlock(FRectangle block)
		{
			for (int ox = (int)(block.Left / GDConstants.TILE_WIDTH); ox <= (int)(block.Right / GDConstants.TILE_WIDTH); ox++)
			{
				for (int oy = (int)(block.Top / GDConstants.TILE_WIDTH); oy <= (int)(block.Bottom / GDConstants.TILE_WIDTH); oy++)
				{
					int x = ox + MAX_EXTENSION; // real coords -> array coords
					int y = oy + MAX_EXTENSION;

					if (x < 0) continue;
					if (y < 0) continue;
					if (x >= TILE_COUNT_X) continue;
					if (y >= TILE_COUNT_Y) continue;

					if (block.Contains(_rects[x, y].TopLeft, 0.5f) && block.Contains(_rects[x, y].TopRight, 0.5f))
						_grid[x, y].BlockNorth = true;

					if (block.Contains(_rects[x, y].TopRight, 0.5f) && block.Contains(_rects[x, y].BottomRight, 0.5f))
						_grid[x, y].BlockEast = true;

					if (block.Contains(_rects[x, y].BottomRight, 0.5f) && block.Contains(_rects[x, y].BottomLeft, 0.5f))
						_grid[x, y].BlockSouth = true;

					if (block.Contains(_rects[x, y].BottomLeft, 0.5f) && block.Contains(_rects[x, y].TopLeft, 0.5f))
						_grid[x, y].BlockWest = true;
				}
			}
		}
	}
}
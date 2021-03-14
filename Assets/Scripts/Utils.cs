using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{

	/// <summary>
	/// 矩形を描画
	/// </summary>
	/// <param name="texPixels"></param>
	/// <param name="w"></param>
	/// <param name="h"></param>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="color"></param>
	/// <param name="radius"></param>
	public static void DrawRect(ref Color32[] texPixels, int w, int h, int x, int y, Color32 color, int radius = 3)
	{
		float rSquared = radius * radius;

		for (int u = x - radius; u < x + radius + 1; u++)
		{
			for (int v = y - radius; v < y + radius + 1; v++)
			{
				if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
				{
					int i = v * w + u; //int i = y*width+x

					if (i >= 0 && i < texPixels.Length)
					{
						texPixels[i].r = color.r;
						texPixels[i].g = color.g;
						texPixels[i].b = color.b;
					}
				}
			}
		}
	}

	/// <summary>
	/// 線を描画
	/// </summary>
	/// <param name="texPixels"></param>
	/// <param name="w"></param>
	/// <param name="h"></param>
	/// <param name="x0"></param>
	/// <param name="y0"></param>
	/// <param name="x1"></param>
	/// <param name="y1"></param>
	/// <param name="color"></param>
	public static void DrawLine(ref Color32[] texPixels, int w, int h, int x0, int y0, int x1, int y1, Color32 color)
	{
		int dy = (int)(y1 - y0);
		int dx = (int)(x1 - x0);
		int stepx, stepy;

		if (dy < 0) { dy = -dy; stepy = -1; }
		else { stepy = 1; }
		if (dx < 0) { dx = -dx; stepx = -1; }
		else { stepx = 1; }
		dy <<= 1;
		dx <<= 1;

		float fraction = 0;

		//tex.SetPixel(x0, y0, col);
		int i = y0 * w + x0;
		if (i >= 0 && i < texPixels.Length)
		{
			texPixels[i].r = color.r;
			texPixels[i].g = color.g;
			texPixels[i].b = color.b;
		}

		if (dx > dy)
		{
			fraction = dy - (dx >> 1);
			while (Mathf.Abs(x0 - x1) > 1)
			{
				if (fraction >= 0)
				{
					y0 += stepy;
					fraction -= dx;
				}
				x0 += stepx;
				fraction += dy;
				//tex.SetPixel(x0, y0, col);
				i = y0 * w + x0;
				if (i >= 0 && i < texPixels.Length)
				{
					texPixels[i].r = color.r;
					texPixels[i].g = color.g;
					texPixels[i].b = color.b;
				}
			}
		}
		else
		{
			fraction = dx - (dy >> 1);
			while (Mathf.Abs(y0 - y1) > 1)
			{
				if (fraction >= 0)
				{
					x0 += stepx;
					fraction -= dy;
				}
				y0 += stepy;
				fraction += dx;
				// tex.SetPixel(x0, y0, col);
				i = y0 * w + x0;
				if (i >= 0 && i < texPixels.Length)
				{
					texPixels[i].r = color.r;
					texPixels[i].g = color.g;
					texPixels[i].b = color.b;
				}
			}
		}
	}

}
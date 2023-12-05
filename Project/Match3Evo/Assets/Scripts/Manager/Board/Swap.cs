using Match3_Evo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swap 
{
	public Field Left { get; set; }
	public Field Right { get; set; }

	public Swap(Field left, Field right)
	{
		this.Left = left;
		this.Right = right;
	}
}

﻿using MercuryEngine.Data.Core.Framework.Structures;

namespace MercuryEngine.Data.Definitions.DataTypes;

public class Vector2 : DataStructure<Vector2>
{
	public float X { get; set; }
	public float Y { get; set; }

	protected override void Describe(DataStructureBuilder<Vector2> builder)
		=> builder.Property(m => m.X)
				  .Property(m => m.Y);
}
﻿using MercuryEngine.Data.Core.Framework.Structures;

namespace MercuryEngine.Data.Definitions.DataTypes;

public class Vector3 : DataStructure<Vector3>
{
	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }

	protected override void Describe(DataStructureBuilder<Vector3> builder)
		=> builder.Property(m => m.X)
				  .Property(m => m.Y)
				  .Property(m => m.Z);
}
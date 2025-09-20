using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MercuryEngine.Data.Types.Bcmdl.Wrappers;

public class VertexData
{
	public Vector3? Position    { get; set; }
	public Vector3? Normal      { get; set; }
	public Vector4? Color       { get; set; }
	public Vector2? UV1         { get; set; }
	public Vector2? UV2         { get; set; }
	public Vector2? UV3         { get; set; }
	public Vector4? Tangent     { get; set; }
	public Vector4? JointIndex  { get; set; }
	public Vector4? JointWeight { get; set; }

	public void Clear()
	{
		Position = null;
		Normal = null;
		Color = null;
		UV1 = null;
		UV2 = null;
		UV3 = null;
		Tangent = null;
		JointIndex = null;
		JointWeight = null;
	}

	internal void ReadFromVertexBuffer(ReadOnlySpan<byte> vertexBufferData, VertexInfoDescription infoSlot, uint vertexIndex)
	{
		Debug.Assert(infoSlot.DataType == 3); // All known models use "3", which is presumably "float"

		var componentByteSize = infoSlot.Count * sizeof(float);
		var componentStart = (int) ( infoSlot.StartOffset + ( vertexIndex * componentByteSize ) );
		var componentEnd = componentStart + componentByteSize;
		var componentData = vertexBufferData[componentStart..componentEnd];

		switch (infoSlot.Type)
		{
			case VertexInfoType.Position:
				Position = ReadVector3(componentData);
				break;
			case VertexInfoType.Normal:
				Normal = ReadVector3(componentData);
				break;
			case VertexInfoType.Color:
				Color = ReadVector4(componentData);
				break;
			case VertexInfoType.UV1:
				UV1 = ReadVector2(componentData);
				break;
			case VertexInfoType.UV2:
				UV2 = ReadVector2(componentData);
				break;
			case VertexInfoType.UV3:
				UV3 = ReadVector2(componentData);
				break;
			case VertexInfoType.Tangent:
				Tangent = ReadVector4(componentData);
				break;
			case VertexInfoType.JointIndex:
				JointIndex = ReadVector4(componentData);
				break;
			case VertexInfoType.JointWeight:
				JointWeight = ReadVector4(componentData);
				break;
		}

		static Vector2 ReadVector2(ReadOnlySpan<byte> data)
		{
			if (data.Length != 2 * sizeof(float))
				throw new ArgumentException($"Wrong amount of data for {nameof(Vector2)}!", nameof(data));

			var values = MemoryMarshal.Cast<byte, float>(data);
			var x = values[0];
			var y = values[1];

			return new Vector2(x, y);
		}

		static Vector3 ReadVector3(ReadOnlySpan<byte> data)
		{
			if (data.Length != 3 * sizeof(float))
				throw new ArgumentException($"Wrong amount of data for {nameof(Vector3)}!", nameof(data));

			var values = MemoryMarshal.Cast<byte, float>(data);
			var x = values[0];
			var y = values[1];
			var z = values[2];

			return new Vector3(x, y, z);
		}

		static Vector4 ReadVector4(ReadOnlySpan<byte> data)
		{
			if (data.Length != 4 * sizeof(float))
				throw new ArgumentException($"Wrong amount of data for {nameof(Vector4)}!", nameof(data));

			var values = MemoryMarshal.Cast<byte, float>(data);
			var x = values[0];
			var y = values[1];
			var z = values[2];
			var w = values[3];

			return new Vector4(x, y, z, w);
		}
	}
}
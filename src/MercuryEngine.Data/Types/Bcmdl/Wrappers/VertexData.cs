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

	internal void ReadFromVertexBuffer(in ReadOnlySpan<byte> vertexBufferData, VertexInfoDescription infoSlot, uint vertexIndex)
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
			case VertexInfoType.Tangent when infoSlot.Count == 3:
				// This is a rare case with billboard models - a tangent for a 2D plane does not have a fourth component
				Tangent = new Vector4(ReadVector3(componentData), 1f);
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

	internal void WriteToVertexBuffer(in Span<byte> vertexBufferData, VertexInfoDescription infoSlot, uint vertexIndex)
	{
		Debug.Assert(infoSlot.DataType == 3); // All known models use "3", which is presumably "float"

		var componentByteSize = infoSlot.Count * sizeof(float);
		var componentStart = (int) ( infoSlot.StartOffset + ( vertexIndex * componentByteSize ) );
		var componentEnd = componentStart + componentByteSize;
		var componentData = vertexBufferData[componentStart..componentEnd];

		switch (infoSlot.Type)
		{
			case VertexInfoType.Position:
				WriteVector3(componentData, Position);
				break;
			case VertexInfoType.Normal:
				WriteVector3(componentData, Normal);
				break;
			case VertexInfoType.Color:
				WriteVector4(componentData, Color);
				break;
			case VertexInfoType.UV1:
				WriteVector2(componentData, UV1);
				break;
			case VertexInfoType.UV2:
				WriteVector2(componentData, UV2);
				break;
			case VertexInfoType.UV3:
				WriteVector2(componentData, UV3);
				break;
			case VertexInfoType.Tangent:
				WriteVector4(componentData, Tangent);
				break;
			case VertexInfoType.JointIndex:
				WriteVector4(componentData, JointIndex);
				break;
			case VertexInfoType.JointWeight:
				WriteVector4(componentData, JointWeight);
				break;
		}

		static void WriteVector2(Span<byte> data, Vector2? value)
		{
			if (data.Length != 2 * sizeof(float))
				throw new ArgumentException($"Wrong amount of data allocated for {nameof(Vector2)}!", nameof(data));

			var values = MemoryMarshal.Cast<byte, float>(data);

			values[0] = value?.X ?? 0;
			values[1] = value?.Y ?? 0;
		}

		static void WriteVector3(Span<byte> data, Vector3? value)
		{
			if (data.Length != 3 * sizeof(float))
				throw new ArgumentException($"Wrong amount of data allocated for {nameof(Vector3)}!", nameof(data));

			var values = MemoryMarshal.Cast<byte, float>(data);

			values[0] = value?.X ?? 0;
			values[1] = value?.Y ?? 0;
			values[2] = value?.Z ?? 0;
		}

		static void WriteVector4(Span<byte> data, Vector4? value)
		{
			if (data.Length != 4 * sizeof(float))
				throw new ArgumentException($"Wrong amount of data allocated for {nameof(Vector4)}!", nameof(data));

			var values = MemoryMarshal.Cast<byte, float>(data);

			values[0] = value?.X ?? 0;
			values[1] = value?.Y ?? 0;
			values[2] = value?.Z ?? 0;
			values[3] = value?.W ?? 0;
		}
	}

	internal IEnumerable<VertexInfoDescription> GetVertexInfoSlots()
	{
		// This ordering tries to maintain the most consistent ordering seen in Dread's base models

		if (Position.HasValue)
			yield return new VertexInfoDescription(VertexInfoType.Position);
		if (Normal.HasValue)
			yield return new VertexInfoDescription(VertexInfoType.Normal);
		if (UV1.HasValue)
			yield return new VertexInfoDescription(VertexInfoType.UV1);
		if (UV2.HasValue)
			yield return new VertexInfoDescription(VertexInfoType.UV2);
		if (UV3.HasValue)
			yield return new VertexInfoDescription(VertexInfoType.UV3);
		if (Color.HasValue)
			yield return new VertexInfoDescription(VertexInfoType.Color);
		if (JointIndex.HasValue)
			yield return new VertexInfoDescription(VertexInfoType.JointIndex);
		if (JointWeight.HasValue)
			yield return new VertexInfoDescription(VertexInfoType.JointWeight);
		if (Tangent.HasValue)
			yield return new VertexInfoDescription(VertexInfoType.Tangent);
	}

	internal void ValidateMatchesLayout(List<VertexInfoDescription> expectedSlots, uint vertexIndex)
	{
		var expectedTypes = expectedSlots.Select(s => s.Type).ToHashSet();

		var shouldHavePosition = expectedTypes.Contains(VertexInfoType.Position);
		var shouldHaveNormal = expectedTypes.Contains(VertexInfoType.Normal);
		var shouldHaveTangent = expectedTypes.Contains(VertexInfoType.Tangent);
		var shouldHaveColor = expectedTypes.Contains(VertexInfoType.Color);
		var shouldHaveUV1 = expectedTypes.Contains(VertexInfoType.UV1);
		var shouldHaveUV2 = expectedTypes.Contains(VertexInfoType.UV2);
		var shouldHaveUV3 = expectedTypes.Contains(VertexInfoType.UV3);
		var shouldHaveJointIndex = expectedTypes.Contains(VertexInfoType.JointIndex);
		var shouldHaveJointWeight = expectedTypes.Contains(VertexInfoType.JointWeight);

		var doesHavePosition = Position.HasValue;
		var doesHaveNormal = Normal.HasValue;
		var doesHaveTangent = Tangent.HasValue;
		var doesHaveColor = Color.HasValue;
		var doesHaveUV1 = UV1.HasValue;
		var doesHaveUV2 = UV2.HasValue;
		var doesHaveUV3 = UV3.HasValue;
		var doesHaveJointIndex = JointIndex.HasValue;
		var doesHaveJointWeight = JointWeight.HasValue;

		AssertType(shouldHavePosition, doesHavePosition);
		AssertType(shouldHaveNormal, doesHaveNormal);
		AssertType(shouldHaveTangent, doesHaveTangent);
		AssertType(shouldHaveColor, doesHaveColor);
		AssertType(shouldHaveUV1, doesHaveUV1);
		AssertType(shouldHaveUV2, doesHaveUV2);
		AssertType(shouldHaveUV3, doesHaveUV3);
		AssertType(shouldHaveJointIndex, doesHaveJointIndex);
		AssertType(shouldHaveJointWeight, doesHaveJointWeight);

		return;

		void AssertType(bool shouldHave, bool doesHave)
		{
			if (shouldHave != doesHave)
			{
				var expectedTypeNames = string.Join(", ", expectedTypes);

				throw new IOException($"The data for vertex {vertexIndex} does not match the expected data layout of: {expectedTypeNames}");
			}
		}
	}
}
using System.Dynamic;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Structures.Fields;

namespace MercuryEngine.Data.Core.Framework.Structures;

[PublicAPI]
public sealed class DynamicStructure : DynamicObject, IDataStructure
{
	public static DynamicStructure Create(string typeName, Action<DynamicStructureBuilder> buildStructure)
	{
		var structure = new DynamicStructure(typeName);
		var builder = new Builder(structure);

		buildStructure(builder);

		return structure;
	}

	private readonly Dictionary<ulong, IDynamicStructureField> fields = new();

	private DynamicStructure(string typeName)
	{
		TypeName = typeName;
	}

	public IEnumerable<IDynamicStructureField> Fields => this.fields.Values;

	public string TypeName { get; }

	public uint Size => (uint) this.fields.Values.Where(f => f.HasValue).Sum(f => f.Size);

	public void Read(BinaryReader reader)
	{
		foreach (var field in Fields)
			field.ClearValue();

		var fieldCount = reader.ReadUInt32();

		for (var i = 0; i < fieldCount; i++)
		{
			var fieldId = reader.ReadUInt64();

			if (!this.fields.TryGetValue(fieldId, out var field))
			{
				var hexDisplay = BitConverter.GetBytes(fieldId).ToHexString();

				throw new IOException($"Unrecognized field ID \"{fieldId}\" ({hexDisplay}) while reading field {i} of {GetType().Name}");
			}

			try
			{
				field.Read(reader);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.FriendlyDescription}) of {GetType().Name}", ex);
			}
		}
	}

	public void Write(BinaryWriter writer)
	{
		var fieldsToWrite = this.fields.Where(f => f.Value.HasValue).ToList();

		writer.Write(fieldsToWrite.Count);

		foreach (var (fieldId, field) in fieldsToWrite)
		{
			writer.Write(fieldId);

			try
			{
				field.Write(writer);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while writing field \"{field.FieldName}\" ({field.FriendlyDescription}) of {GetType().Name}", ex);
			}
		}
	}

	#region DynamicObject

	public override IEnumerable<string> GetDynamicMemberNames()
		=> this.fields.Values.Select(f => f.FieldName);

	public override bool TryGetMember(GetMemberBinder binder, out object? result)
	{
		result = default;

		if (!this.fields.TryGetValue(binder.Name.GetCrc64(), out var field))
			return false;

		result = field.Value;
		return true;
	}

	public override bool TrySetMember(SetMemberBinder binder, object? value)
	{
		if (value is null)
			throw new ArgumentNullException(nameof(value));

		if (!this.fields.TryGetValue(binder.Name.GetCrc64(), out var field))
			return false;

		field.Value = value;
		return true;
	}

	#endregion

	#region Builder

	private sealed class Builder : DynamicStructureBuilder
	{
		internal Builder(DynamicStructure owner)
		{
			StructureBeingBuilt = owner;
		}

		protected override DynamicStructure StructureBeingBuilt { get; }

		protected override void AddField(IDynamicStructureField field)
		{
			var fieldId = field.FieldName.GetCrc64();

			StructureBeingBuilt.fields.Add(fieldId, field);
		}
	}

	#endregion
}
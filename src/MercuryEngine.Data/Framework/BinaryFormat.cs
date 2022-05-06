using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using MercuryEngine.Data.Exceptions;
using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Framework.Adapters;
using MercuryEngine.Data.Framework.Components;

namespace MercuryEngine.Data.Framework;

[PublicAPI]
public abstract class BinaryFormat<T> : IBinaryFormat, IBinaryComponent<T>
where T : BinaryFormat<T>
{
	private readonly List<Field> fields = new();
	private readonly long        size;

	protected BinaryFormat()
	{
		var builder = new BuilderImpl((T) this);

		// ReSharper disable once VirtualMemberCallInConstructor
		Describe(builder);

		IsFixedSize = this.fields.All(f => f.Component is IFixedSizeBinaryComponent);

		if (IsFixedSize)
			this.size = this.fields.Select(f => f.Component).Cast<IFixedSizeBinaryComponent>().Sum(c => c.Size);
	}

	public bool IsFixedSize { get; }

	public long Size => IsFixedSize ? this.size : throw new InvalidOperationException("Format does not have a fixed size");

	protected abstract void Describe(Builder builder);

	public void Read(Stream stream)
	{
		using var reader = new BinaryReader(stream, Encoding.UTF8, true);

		Read(reader);
	}

	private void Read(BinaryReader reader)
	{
		foreach (var (i, field) in this.fields.Pairs())
		{
			var (component, adapter) = field;

			if (!field.Component.Validate(reader.BaseStream))
			{
				throw new DataValidationException(
					reader.BaseStream.Position,
					$"Failed to read field {i} for {GetType().Name}: validation failed for a \"{component.GetType().Name}\" component"
				);
			}

			adapter.Read(reader);
		}
	}

	public void Write(Stream stream)
	{
		using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

		Write(writer);
	}

	private void Write(BinaryWriter writer)
	{
		foreach (var field in this.fields)
			field.Adapter.Write(writer);
	}

	#region IBinaryComponent Explicit Implementation

	bool IBinaryComponent.Validate(Stream stream)
		// We would have to advance the stream to each field before validating it,
		// which may not be possible without actually reading if there are non-
		// fixed-size fields present. Instead, we just return true, and let the
		// validation occur while reading.
		=> true;

	object IBinaryComponent.Read(BinaryReader reader)
	{
		Read(reader);

		return this;
	}

	void IBinaryComponent.Write(BinaryWriter writer, object data)
	{
		if (!ReferenceEquals(data, this))
			throw new ArgumentException($"A {nameof(BinaryFormat<T>)} can only write itself when writing data as a {nameof(IBinaryComponent)}.");

		Write(writer);
	}

	#endregion

	#region IBinaryComponent<T> Explicit Implementation

	T IBinaryComponent<T>.Read(BinaryReader reader)
	{
		Read(reader);

		return (T) this;
	}

	void IBinaryComponent<T>.Write(BinaryWriter writer, T data)
	{
		if (!ReferenceEquals(data, this))
			throw new ArgumentException($"A {nameof(BinaryFormat<T>)} can only write itself when writing data as a {nameof(IBinaryComponent)}.");

		Write(writer);
	}

	#endregion

	#region Helper Types

	private sealed record Field(IBinaryComponent Component, IComponentAdapter Adapter);

	[PublicAPI]
	public abstract class Builder
	{
		private protected Builder() { }

		public abstract Builder Literal(byte[] data);
		public abstract Builder Literal(string text);

		public abstract Builder TerminatedString(Expression<Func<T, string>> expression);

		public abstract Builder Int16(Expression<Func<T, short>> expression);
		public abstract Builder UInt16(Expression<Func<T, ushort>> expression);
		public abstract Builder Int32(Expression<Func<T, int>> expression);
		public abstract Builder UInt32(Expression<Func<T, uint>> expression);
		public abstract Builder Int64(Expression<Func<T, long>> expression);
		public abstract Builder UInt64(Expression<Func<T, ulong>> expression);
		public abstract Builder Float(Expression<Func<T, float>> expression);
		public abstract Builder Double(Expression<Func<T, double>> expression);
		public abstract Builder Decimal(Expression<Func<T, decimal>> expression);

		public abstract Builder Structure<TStructure>(Expression<Func<T, TStructure>> propertyExpression)
		where TStructure : IBinaryFormat, IBinaryComponent<TStructure>;

		public abstract Builder AddField(IBinaryComponent component);

		public abstract Builder AddField<TProperty>(IBinaryComponent<TProperty> component, Expression<Func<T, TProperty>> propertyExpression)
		where TProperty : notnull;
	}

	private sealed class BuilderImpl : Builder
	{
		private readonly T owner;

		public BuilderImpl(T owner)
		{
			this.owner = owner;
		}

		#region Literal

		public override Builder Literal(byte[] data)
			=> AddField(new LiteralComponent(data));

		public override Builder Literal(string text)
			=> AddField(new LiteralComponent(text));

		#endregion

		#region Strings

		public override Builder TerminatedString(Expression<Func<T, string>> expression)
			=> AddField(new TerminatedStringComponent(), expression);

		#endregion

		#region Numeric

		public override Builder Int16(Expression<Func<T, short>> expression)
			=> AddField(new Int16Component(), expression);

		public override Builder UInt16(Expression<Func<T, ushort>> expression)
			=> AddField(new UInt16Component(), expression);

		public override Builder Int32(Expression<Func<T, int>> expression)
			=> AddField(new Int32Component(), expression);

		public override Builder UInt32(Expression<Func<T, uint>> expression)
			=> AddField(new UInt32Component(), expression);

		public override Builder Int64(Expression<Func<T, long>> expression)
			=> AddField(new Int64Component(), expression);

		public override Builder UInt64(Expression<Func<T, ulong>> expression)
			=> AddField(new UInt64Component(), expression);

		public override Builder Float(Expression<Func<T, float>> expression)
			=> AddField(new FloatComponent(), expression);

		public override Builder Double(Expression<Func<T, double>> expression)
			=> AddField(new DoubleComponent(), expression);

		public override Builder Decimal(Expression<Func<T, decimal>> expression)
			=> AddField(new DecimalComponent(), expression);

		#endregion

		#region Sub-Structure

		public override Builder Structure<TStructure>(Expression<Func<T, TStructure>> propertyExpression)
		{
			var adapter = new SubComponentAdapter<T, TStructure>(this.owner, propertyExpression);

			this.owner.fields.Add(new Field(adapter, adapter));
			return this;
		}

		#endregion

		public override Builder AddField(IBinaryComponent component)
		{
			var adapter = new NullComponentAdapter(component);

			this.owner.fields.Add(new Field(component, adapter));
			return this;
		}

		public override Builder AddField<TProperty>(IBinaryComponent<TProperty> component, Expression<Func<T, TProperty>> propertyExpression)
		{
			var adapter = new PropertyComponentAdapter<T, TProperty>(component, this.owner, propertyExpression);

			this.owner.fields.Add(new Field(component, adapter));
			return this;
		}
	}

	#endregion
}
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using MercuryEngine.Data.Framework.Components;
using MercuryEngine.Data.Utility;

namespace MercuryEngine.Data.Framework.Adapters;

/// <summary>
/// An implementation of both <see cref="IBinaryComponent"/> and <see cref="IComponentAdapter"/> that
/// handles reading and writing a <see cref="IBinaryFormat"/> that is a child (sub-structure) of another
/// <see cref="IBinaryFormat"/>.
/// </summary>
/// <typeparam name="TParent">The type of the parent structure.</typeparam>
/// <typeparam name="TComponent">The type of the sub-structure.</typeparam>
[PublicAPI]
public class SubComponentAdapter<TParent, TComponent> : BinaryComponent<TComponent>, IComponentAdapter<TComponent>
where TParent : class
where TComponent : IBinaryFormat, IBinaryComponent<TComponent>
{
	private readonly PropertyInfo propertyInfo;

	public SubComponentAdapter(TParent parent, Expression<Func<TParent, TComponent>> propertyExpression)
	{
		Parent = parent;

		this.propertyInfo = ExpressionUtility.GetProperty(propertyExpression);
	}

	public TParent Parent { get; }

	protected TComponent Value => (TComponent) this.propertyInfo.GetValue(Parent)!;

	public override bool IsFixedSize
		=> Value.IsFixedSize;

	public override bool Validate(Stream stream)
		=> Value.Validate(stream);

	public override TComponent Read(BinaryReader reader)
		=> Value.Read(reader);

	public override void Write(BinaryWriter writer, TComponent data)
		=> Value.Write(writer, data);

	void IComponentAdapter.Read(BinaryReader reader)
		=> Value.Read(reader);

	public void Write(BinaryWriter writer)
		=> Value.Write(writer.BaseStream);

	TComponent IComponentAdapter<TComponent>.Component => Value;
}
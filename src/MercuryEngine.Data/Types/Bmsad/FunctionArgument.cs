﻿using System.Linq.Expressions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bmsad;

public class FunctionArgument : DataStructure<FunctionArgument>
{
	private static readonly Expression<Func<FunctionArgument, char>>        TypeExpression   = m => m.Type;
	private static readonly Expression<Func<FunctionArgument, SwitchField>> SwitchExpression = m => m.Switch;

	public FunctionArgument()
	{
		Switch = SwitchField.FromProperty(this, TypeExpression, sw => {
			sw.AddCase('s', StringField);
			sw.AddCase('f', FloatField);
			sw.AddCase('b', BooleanField);
			sw.AddCase('i', IntegerField);
		});
	}

	public char Type { get; set; } = 'b';

	public object Value
	{
		get => Type switch {
			's' => StringField.Value,
			'f' => FloatField.Value,
			'b' => BooleanField.Value,
			'i' => IntegerField.Value,
			_   => throw new NotSupportedException($"Function argument has unknown type '{Type}'"),
		};
		set
		{
			ArgumentNullException.ThrowIfNull(value);

			switch (value)
			{
				case string s:
					StringField.Value = s;
					break;
				case float f:
					FloatField.Value = f;
					break;
				case bool b:
					BooleanField.Value = b;
					break;
				case int i:
					IntegerField.Value = i;
					break;
				default:
					throw new ArgumentException($"Unsupported value type \"{value.GetType().FullName}\"", nameof(value));
			}
		}
	}

	private SwitchField Switch { get; }

	private TerminatedStringField StringField  { get; } = new();
	private FloatField            FloatField   { get; } = new();
	private BooleanField          BooleanField { get; } = new();
	private Int32Field            IntegerField { get; } = new();

	protected override void Describe(DataStructureBuilder<FunctionArgument> builder)
	{
		builder.Property(TypeExpression);
		builder.RawProperty(SwitchExpression);
	}
}
namespace MercuryEngine.Data.Framework.Components;

public abstract class BinaryComponent : IBinaryComponent
{
	public abstract bool IsFixedSize { get; }

	public abstract bool Validate(Stream stream);
	public abstract object Read(BinaryReader reader);
	public abstract void Write(BinaryWriter writer, object data);
}

public abstract class BinaryComponent<T> : IBinaryComponent<T>
where T : notnull
{
	public abstract bool IsFixedSize { get; }

	public abstract bool Validate(Stream stream);
	public abstract T Read(BinaryReader reader);
	public abstract void Write(BinaryWriter writer, T data);

	object IBinaryComponent.Read(BinaryReader reader)
		=> Read(reader);

	void IBinaryComponent.Write(BinaryWriter writer, object data)
	{
		if (data is null)
			throw new ArgumentNullException(nameof(data));

		if (data is not T castData)
		{
			var expected = typeof(T).FullName;
			var actual = data.GetType().FullName;

			throw new ArgumentException($"The provided data was not of the correct type. Expected \"{expected}\" but got \"{actual}\".", nameof(data));
		}

		Write(writer, castData);
	}
}
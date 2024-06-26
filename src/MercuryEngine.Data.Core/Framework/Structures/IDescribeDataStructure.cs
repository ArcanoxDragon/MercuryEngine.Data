using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Core.Framework.Structures;

[PublicAPI]
public interface IDescribeDataStructure<T>
where T : DataStructure<T>, IDescribeDataStructure<T>
{
	static abstract void Describe(DataStructureBuilder<T> builder);
}
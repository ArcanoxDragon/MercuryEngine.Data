using System.Text.Json.Serialization;

namespace MercuryEngine.Data.Core.Framework.Mapping;

public interface IDataMapperAware
{
	[JsonIgnore]
	DataMapper? DataMapper { get; set; }
}
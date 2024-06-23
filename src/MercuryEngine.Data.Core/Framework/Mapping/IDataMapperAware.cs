namespace MercuryEngine.Data.Core.Framework.Mapping;

public interface IDataMapperAware
{
	DataMapper? DataMapper { get; set; }
}

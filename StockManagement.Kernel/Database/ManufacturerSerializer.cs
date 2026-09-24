using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace StockManagement.Kernel.Database;


/// <summary>
/// Stores <see cref="Model.StockItem.Manufacturer"/> as a name and still reads the numbers of the former enum.
/// </summary>
/// <remarks>Documents are rewritten as names the next time they are saved.</remarks>
internal sealed class ManufacturerSerializer : SerializerBase<string>
{
	private static readonly string[] LegacyNames = [string.Empty, "Samasz", "Metallfach", "Hattat"];


	public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
	{
		var reader = context.Reader;
		switch (reader.GetCurrentBsonType())
		{
			case BsonType.Int32:
				var number = reader.ReadInt32();
				return number >= 0 && number < LegacyNames.Length ? LegacyNames[number] : string.Empty;
			case BsonType.Null:
				reader.ReadNull();
				return string.Empty;
			default:
				return reader.ReadString();
		}
	}

	public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
	{
		context.Writer.WriteString(value ?? string.Empty);
	}
}

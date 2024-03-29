﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public class Location : NotificationBase
{
	private IsleType _isle;
	private ShelfType _shelf;


	public Location (IsleType isle = IsleType.None, ShelfType shelf = ShelfType.None)
	{
		_isle = isle;
		_shelf = shelf;
	}

	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; internal set; }

	public IsleType Isle
	{
		get { return _isle; }
		internal set { this.SetField(ref _isle, value); }
	}

	public ShelfType Shelf
	{
		get { return _shelf; }
		internal set { this.SetField(ref _shelf, value); }
	}
}
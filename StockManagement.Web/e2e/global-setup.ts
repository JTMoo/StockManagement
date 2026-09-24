import { MongoClient } from "mongodb";
import { databaseName, mongoUrl } from "../playwright.config";

// No stock item endpoint yet, so seed the collection the Kernel reads.
export default async function globalSetup()
{
	const client = await MongoClient.connect(mongoUrl);
	const database = client.db(databaseName);
	await database.dropDatabase();
	await database.collection("StockManagement.Kernel.Model.StockItem").insertMany([
		{ Name: "Screw", Code: "A1", Amount: 10, Description: "M6", Location: "A-1", Price: 5000.0, Factor: 0.0, Manufacturer: "", Miscellaneous: "" },
		{ Name: "Nut", Code: "B2", Amount: 1, Description: "M6", Location: "B-2", Price: 1000.0, Factor: 0.0, Manufacturer: "", Miscellaneous: "" }
	]);
	await client.close();
}

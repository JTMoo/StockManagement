using StockManagement.Auth.Core;

namespace StockManagement.Tests.Auth;


[TestClass]
public sealed class PasswordHasherTests
{
	[TestMethod]
	public void Verify_CorrectPassword_ReturnsTrue()
	{
		// Arrange
		var hash = PasswordHasher.Hash("correct horse battery staple");

		// Act
		var result = PasswordHasher.Verify("correct horse battery staple", hash);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Verify_WrongPassword_ReturnsFalse()
	{
		// Arrange
		var hash = PasswordHasher.Hash("correct horse battery staple");

		// Act
		var result = PasswordHasher.Verify("wrong password", hash);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Hash_SamePasswordTwice_ProducesDifferentHashes()
	{
		// Act
		var first = PasswordHasher.Hash("correct horse battery staple");
		var second = PasswordHasher.Hash("correct horse battery staple");

		// Assert
		Assert.AreNotEqual(first, second);
	}

	[TestMethod]
	public void Verify_MalformedHash_ReturnsFalse()
	{
		// Act
		var result = PasswordHasher.Verify("anything", "not-a-valid-hash");

		// Assert
		Assert.IsFalse(result);
	}
}

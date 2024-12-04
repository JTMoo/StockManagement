using Moq;
using StockManagement.Kernel.Commands;

namespace StockManagement.Tests.Kernel;


[TestClass]
public sealed class CommandQueueTests
{
	[TestMethod]
	public void Add_InsertNull_ReturnsFalse()
	{
		// Assign
		var cmdQueue = new CommandQueue();

		// Act
		var result = cmdQueue.Add(null);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Add_QueueNull_ReturnsFalse()
	{
		// Assign
		var cmdQueue = new CommandQueue();
		var field = cmdQueue.GetType().GetField("_queue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field.SetValue(cmdQueue, null);

		var cmd = new Mock<ICommand>();

		// Act
		var result = cmdQueue.Add(cmd.Object);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Add_ValidCommand_ReturnsTrue()
	{
		// Assign
		var cmdQueue = new CommandQueue();
		var cmd = new Mock<ICommand>();

		// Act
		var result = cmdQueue.Add(cmd.Object);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Add_ValidCommand_CommandInQueue()
	{
		// Assign
		var cmdQueue = new CommandQueue();
		var field = cmdQueue.GetType().GetField("_queue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cmdQueue) as List<ICommand>;
		var cmd = new Mock<ICommand>();

		// Act
		cmdQueue.Add(cmd.Object);

		// Assert
		Assert.AreEqual(cmd.Object, field.First());
	}
}

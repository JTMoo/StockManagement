using StockManagement.Kernel.Commands;

namespace StockManagement.Kernel.Tests
{
    [TestClass]
    public class CommandQueueTests
    {
        [TestMethod]
        public void Add_NullAdded_DoesntAddToQueue()
        {
            // Arrange
            var queue = new CommandQueue();

            // Act
            var result = queue.Add(null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Pop_EmptyQueue_ReturnsNull()
        {
            // Arrange
            var queue = new CommandQueue();

            // Act
            var result = queue.Pop();

            // Assert
            Assert.IsNull(result);
        }
    }
}

using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace HikeManager.Tests.TestHelpers
{
    [CollectionDefinition("DatabaseCollection")]
    public class DatabaseTestCollection
    {
    }
}


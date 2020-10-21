using LibraryCore.ControllerHelpers;
using LibraryCore.Models;
using LibraryCore.Models.Repositories;
using Xunit;
using Assert = Xunit.Assert;

namespace LibraryCoreTests.Models.Repositories
{
    public class HoldingRepositoryTest
    {
        InMemoryRepository<Holding> repo;

        public HoldingRepositoryTest()
        {
            repo = new InMemoryRepository<Holding>();
            repo.Clear();
        }

        [Fact]
        public void FindByBarcodeReturnsNullWhenNotFound()
        {
            Assert.Null(HoldingsControllerUtil.FindByBarcode(repo, "AA:1"));
        }

        [Fact]
        public void FindByBarcodeReturnsHoldingMatchingClassificationAndCopy()
        {
            var holding = new Holding { Classification = "AA123", CopyNumber = 2 };

            repo.Create(holding);

            Assert.Equal(holding, HoldingsControllerUtil.FindByBarcode(repo, "AA123:2"));
        }
    }
}

using Library.ControllerHelpers;
using Library.Models;
using Library.Models.Repositories;
using Xunit;
using Assert = Xunit.Assert;

namespace LibraryCoreTests.ControllerHelpers
{
    public class HoldingsControllerUtilTest
    {
        IRepository<Holding> holdingRepo;

        public HoldingsControllerUtilTest()
        {
            holdingRepo = new InMemoryRepository<Holding>();
        }

        public class NextAvailableCopyNumber : HoldingsControllerUtilTest
        {
            [Fact]
            public void NextAvailableCopyNumberIncrementsCopyNumberUsingCount()
            {
                holdingRepo.Create(new Holding("AB123:1"));
                holdingRepo.Create(new Holding("AB123:2"));
                holdingRepo.Create(new Holding("XX123:1"));

                var copyNumber = HoldingsControllerUtil.NextAvailableCopyNumber(holdingRepo, "AB123");

                Assert.Equal(3, copyNumber);
            }
        }

        public class FindByBarcodeOrBarcodeDetailsTest : HoldingsControllerUtilTest
        {
            int idForAB123_2;
            int idForXX123_1;

            public FindByBarcodeOrBarcodeDetailsTest()
            {
                holdingRepo.Create(new Holding("AB123:1"));
                idForAB123_2 = holdingRepo.Create(new Holding("AB123:2"));
                idForXX123_1 = holdingRepo.Create(new Holding("XX123:1"));
            }

            [Fact]
            public void ByBarcodeReturnsMatchingHolding()
            {
                Assert.Equal(idForAB123_2, HoldingsControllerUtil.FindByBarcode(holdingRepo, "AB123:2").Id);
            }

            [Fact]
            public void ByClassificationAndCopyReturnsMatchingHolding()
            {
                Assert.Equal(idForXX123_1, HoldingsControllerUtil.FindByClassificationAndCopy(holdingRepo, "XX123", 1).Id);
            }
        }
    }
}

using System.Linq;
using LibraryCore.Models;
using LibraryCore.Models.Repositories;

namespace LibraryCore.ControllerHelpers
{
    public class HoldingsControllerUtil
    {
        public static Holding FindByClassificationAndCopy(IRepository<Holding> holdingRepo, string classification, int copyNumber)
        {
            return holdingRepo
                .FindBy((h => h.Classification == classification && h.CopyNumber == copyNumber ))
                .FirstOrDefault();
        }

        public static Holding FindByBarcode(IRepository<Holding> repo, string barcode)
        {
            return FindByClassificationAndCopy(repo, Holding.ClassificationFromBarcode(barcode), Holding.CopyNumberFromBarcode(barcode));
        }

        public static int NextAvailableCopyNumber(IRepository<Holding> holdingRepo, string classification)
        {
            return holdingRepo.FindBy(h => h.Classification == classification).Count() + 1;
        }
    }
}
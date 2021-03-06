﻿using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace LibraryCore.Models
{
    [Serializable]
    public class Holding: Identifiable
    {
        public const int NoPatron = -1;

        public Holding()
            : this("", 1, Branch.CheckedOutId)
        {
        }

        public Holding(string barcode)
            : this(ClassificationFromBarcode(barcode), CopyNumberFromBarcode(barcode), Branch.CheckedOutId)
        {
        }

        public override string ToString()
        {
            return $"{Classification}:{CopyNumber}:{BranchId}";
        }

        public Holding(string classification, int copyNumber, int branchId)
        {
            CheckOutTimestamp = null;
            LastCheckedIn = null;
            DueDate = null;
            Classification = classification;
            CopyNumber = copyNumber;
            BranchId = branchId;
        }

        public static bool IsBarcodeValid(string barcode)
        {
            return new Regex("^.+:[1-9]\\d*$").IsMatch(barcode);
        }

        public int Id { get; set; }

        protected bool Equals(Holding other)
        {
            return Classification == other.Classification && CopyNumber == other.CopyNumber && BranchId == other.BranchId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Holding) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Classification != null ? Classification.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ CopyNumber;
                hashCode = (hashCode * 397) ^ BranchId;
                return hashCode;
            }
        }

        public string Classification { get; set; }

        [Display(Name = "Copy #")]
        public int CopyNumber { get; set; }

        public DateTime? CheckOutTimestamp { get; set; }
        public DateTime? LastCheckedIn { get; set; }

        [Display(Name = "Date Due")]
        public DateTime? DueDate { get; set; }

        public int BranchId { get; set; }

        public int HeldByPatronId { get; set; }

        [NotMapped]
        public CheckoutPolicy CheckoutPolicy { get; set; }

        [NotMapped, Display(Name = "Bar Code")]
        public string Barcode
        {
            get { return GenerateBarcode(Classification, CopyNumber); }
        }

        [NotMapped]
        public bool IsCheckedOut
        {
            get { return BranchId == Branch.CheckedOutId; }
        }

        public IEnumerable BranchesViewList { get; set; }

        public void CheckIn(DateTime timestamp, int toBranchId)
        {
            LastCheckedIn = timestamp;
            CheckOutTimestamp = null;
            HeldByPatronId = NoPatron;
            BranchId = toBranchId;
        }

        public void CheckOut(DateTime timestamp, int patronId, CheckoutPolicy checkoutPolicy)
        {
            CheckOutTimestamp = timestamp;
            HeldByPatronId = patronId;
            CheckoutPolicy = checkoutPolicy;
            CalculateDueDate();
            BranchId = Branch.CheckedOutId;
        }

        private void CalculateDueDate()
        {
            DueDate = CheckOutTimestamp.Value.AddDays(CheckoutPolicy.MaximumCheckoutDays());
        }

        public static string GenerateBarcode(string classification, int copyNumber)
        {
            return $"{classification}:{copyNumber}";
        }

        public static string ClassificationFromBarcode(string barcode)
        {
            var colonIndex = barcode.IndexOf(':');
            return barcode.Substring(0, colonIndex);
        }

        public static int CopyNumberFromBarcode(string barcode)
        {
            var colonIndex = barcode.IndexOf(':');
            return int.Parse(barcode.Substring(colonIndex + 1));
        }

        public int DaysLate()
        {
            var daysLate = LastCheckedIn.Value.Subtract(DueDate.Value).Days;
            return Math.Max(0, daysLate);
        }
    }
}

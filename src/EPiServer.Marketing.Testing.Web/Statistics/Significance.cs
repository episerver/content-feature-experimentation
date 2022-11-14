﻿using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Core.DataClass;

namespace EPiServer.Marketing.Testing.Web.Statistics
{
    public class SignificanceResults
    {
        public bool IsSignificant { get; set; }

        public double ZScore { get; set; }

        public Guid WinningVariantId { get; set; }
       
    }

    public static class Significance
    {
        // Confidence level and corresponding Z-score used to determine significance
        private static readonly Dictionary<double, double> ZScores = new Dictionary<double, double>() { { 90, 1.645 }, { 95, 1.960 }, { 98, 2.326 }, { 99, 2.576 } };

        public static SignificanceResults CalculateIsSignificant(IMarketingTest test)
        {
            foreach (var variant in test.Variants)
            {
                if (variant.Views == 0)
                {
                    return new SignificanceResults()
                    {
                        IsSignificant = false,
                        ZScore = 0
                    };
                }
            }

            var originalConversionRate = (double) test.Variants[0].Conversions/test.Variants[0].Views;
            var originalStandardError =
                Math.Sqrt(originalConversionRate*(1 - originalConversionRate)/test.Variants[0].Views);

            var variantConversionRate = (double) test.Variants[1].Conversions/test.Variants[1].Views;
            var variantStandardError =
                Math.Sqrt(variantConversionRate*(1 - variantConversionRate)/test.Variants[1].Views);

            var standardErrorOfDifference =
                Math.Sqrt(Math.Pow(originalStandardError, 2) + Math.Pow(variantStandardError, 2));

            var calculatedZScore = Math.Abs(variantConversionRate - originalConversionRate)/
                                    standardErrorOfDifference;

            var winningVariantId = Guid.Empty;

            if (originalConversionRate > variantConversionRate)
            {
                winningVariantId = test.Variants[0].Id;
            }
            else if (variantConversionRate > originalConversionRate)
            {
                winningVariantId = test.Variants[1].Id;
            }
            return new SignificanceResults()
            {
                IsSignificant = calculatedZScore > ZScores[test.ConfidenceLevel],
                ZScore = calculatedZScore,
                WinningVariantId = winningVariantId
            };

        }
    }
}

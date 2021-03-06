﻿// Accord Unit Tests
// The Accord.NET Framework
// http://accord-framework.net
//
// Copyright © César Souza, 2009-2017
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

namespace Accord.Tests.Statistics
{
    using Accord.Statistics.Analysis;
    using NUnit.Framework;
    using Accord.Math;
    using System;
    using Accord.Statistics.Filters;
    using System.Data;

    [TestFixture]
    public class GeneralConfusionMatrixTest
    {


        [Test]
        public void GeneralConfusionMatrixConstructorTest()
        {
            int classes = 3;

            int[] expected = { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2 };
            int[] predicted = { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 };

            GeneralConfusionMatrix target = new GeneralConfusionMatrix(classes, expected, predicted);


            Assert.AreEqual(3, target.Classes);
            Assert.AreEqual(12, target.Samples);

            int[,] expectedMatrix =
            {
                { 4, 0, 0 },
                { 0, 4, 0 },
                { 0, 4, 0 },
            };

            int[,] actualMatrix = target.Matrix;

            Assert.IsTrue(expectedMatrix.IsEqual(actualMatrix));
        }


        [Test]
        public void class_confusion_matrices()
        {
            int[] expected = { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2 };
            int[] predicted = { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 };

            var target = new GeneralConfusionMatrix(expected, predicted);

            Assert.AreEqual(3, target.Classes);
            Assert.AreEqual(12, target.Samples);

            ConfusionMatrix actual1 = target.PerClassMatrices[0];
            ConfusionMatrix actual2 = target.PerClassMatrices[1];
            ConfusionMatrix actual3 = target.PerClassMatrices[2];

            ConfusionMatrix expected1 = new ConfusionMatrix(predicted, expected, positiveValue: 0);
            ConfusionMatrix expected2 = new ConfusionMatrix(predicted, expected, positiveValue: 1);
            ConfusionMatrix expected3 = new ConfusionMatrix(predicted, expected, positiveValue: 2);

            Assert.IsTrue(actual1.Matrix.IsEqual(expected1.Matrix));
            Assert.IsTrue(actual2.Matrix.IsEqual(expected2.Matrix));
            Assert.IsTrue(actual3.Matrix.IsEqual(expected3.Matrix));
        }

        [Test]
        public void class_confusion_matrices_larger()
        {
            int[] expected = { 0, 0, 0, 1, 1, 1, 1, 1, 2, 4, 4, 3, 2, 2 };
            int[] predicted = { 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1 };

            var target = new GeneralConfusionMatrix(expected, predicted);

            Assert.AreEqual(5, target.Classes);
            Assert.AreEqual(14, target.Samples);

            ConfusionMatrix actual1 = target.PerClassMatrices[0];
            ConfusionMatrix actual2 = target.PerClassMatrices[1];
            ConfusionMatrix actual3 = target.PerClassMatrices[2];

            ConfusionMatrix expected1 = new ConfusionMatrix(predicted, expected, positiveValue: 0);
            ConfusionMatrix expected2 = new ConfusionMatrix(predicted, expected, positiveValue: 1);
            ConfusionMatrix expected3 = new ConfusionMatrix(predicted, expected, positiveValue: 2);

            Assert.IsTrue(actual1.Matrix.IsEqual(expected1.Matrix));
            Assert.IsTrue(actual2.Matrix.IsEqual(expected2.Matrix));
            Assert.IsTrue(actual3.Matrix.IsEqual(expected3.Matrix));
        }

        [Test]
        public void gh669()
        {
            // Example for https://github.com/accord-net/framework/issues/669
            string[] expectedLabels = { "A", "A", "B", "C", "A", "B", "B" };
            string[] predictedLabels = { "A", "B", "C", "C", "A", "C", "B" };

            // Create a codification object to translate char into symbols
            var codification = new Codification("Labels", expectedLabels);
            int[] expected = codification.Transform(expectedLabels);   // ground truth data
            int[] predicted = codification.Transform(predictedLabels); // predicted from OCR

            // Create a new confusion matrix for multi-class problems
            var cm = new GeneralConfusionMatrix(expected, predicted);

            // Obtain relevant measures
            int[,] matrix = cm.Matrix;
            int[] error = cm.PerClassMatrices.Apply(x => x.Errors);
            double[] recall = cm.PerClassMatrices.Apply(x => x.Recall);
            int[] total = cm.PerClassMatrices.Apply(x => x.Samples);
            int[] tp = cm.PerClassMatrices.Apply(x => x.TruePositives);
            int[] tn = cm.PerClassMatrices.Apply(x => x.TrueNegatives);
            int[] fp = cm.PerClassMatrices.Apply(x => x.FalsePositives);
            int[] fn = cm.PerClassMatrices.Apply(x => x.FalseNegatives);
            double[] precision = cm.PerClassMatrices.Apply(x => x.Precision);
            double[] fscore = cm.PerClassMatrices.Apply(x => x.FScore);

            // Create a matrix with all measures
            double[,] values = matrix.ToDouble()
                .InsertColumn(error)
                .InsertColumn(recall)
                .InsertColumn(total)
                .InsertColumn(tp)
                .InsertColumn(tn)
                .InsertColumn(tp)
                .InsertColumn(fn)
                .InsertColumn(precision)
                .InsertColumn(fscore);

            // Name of each of the columns in order to create a data table
            string[] columnNames = codification.Columns[0].Values.Concatenate(
                "Error", "Recall", "Total", "TP", "TN", "FP", "FN", "Precision", "F-Score");

            // Create a table from the matrix and columns
            DataTable table = values.ToTable(columnNames);


            string[] actualNames;
            double[,] actual = table.ToMatrix(out actualNames);

            double[,] expectedMatrix = new double[,]
            {
                { 2, 1, 0, 1, 0.666666666666667, 7, 2, 4, 2, 1, 1, 0.8 },
                { 0, 1, 2, 3, 0.333333333333333, 7, 1, 3, 1, 2, 0.5, 0.4 },
                { 0, 0, 1, 2, 1, 7, 1, 4, 1, 0, 0.333333333333333, 0.5 }
            };

            //string str = actual.ToCSharp();

            Assert.AreEqual(new[] { "A", "B", "C", "Error", "Recall", "Total", "TP", "TN", "FP", "FN", "Precision", "F-Score" }, actualNames);
            Assert.IsTrue(expectedMatrix.IsEqual(actual, 1e-5));
        }

        [Test]
        public void gh669_predicted_label_not_in_expected_set()
        {
            // Example for https://github.com/accord-net/framework/issues/669
            string[] expectedLabels = { "A", "A", "B", "C", "A", "B", "B", "B" };
            string[] predictedLabels = { "A", "B", "C", "C", "A", "C", "B", "F" };

            // Create a codification object to translate char into symbols
            var codification = new Codification("Labels", expectedLabels.Concatenate(predictedLabels));
            int[] expected = codification.Transform(expectedLabels);   // ground truth data
            int[] predicted = codification.Transform(predictedLabels); // predicted from OCR

            // Create a new confusion matrix for multi-class problems
            var cm = new GeneralConfusionMatrix(expected, predicted);

            Assert.AreEqual(4, cm.Classes);
            Assert.AreEqual(8, cm.Samples);
            Assert.AreEqual(0.5, cm.Accuracy);
            Assert.AreEqual(4, cm.PerClassMatrices.Length);
        }


        [Test]
        public void GeneralConfusionMatrixConstructorTest2()
        {
            int[,] matrix =
            {
                { 4, 0, 0 },
                { 0, 4, 4 },
                { 0, 0, 0 },
            };

            GeneralConfusionMatrix target = new GeneralConfusionMatrix(matrix);


            Assert.AreEqual(3, target.Classes);
            Assert.AreEqual(12, target.Samples);
            Assert.AreEqual(matrix, target.Matrix);
            Assert.AreEqual(0, target.GeometricAgreement);
        }

        [Test]
        public void KappaTest()
        {
            int[,] matrix =
            {
                { 29,  6,  5 },
                {  8, 20,  7 },
                {  1,  2, 22 },
            };

            GeneralConfusionMatrix target = new GeneralConfusionMatrix(matrix);


            Assert.AreEqual(3, target.Classes);
            Assert.AreEqual(100, target.Samples);

            Assert.AreEqual(0.563, target.Kappa, 1e-3);
            Assert.AreEqual(23.367749664961245, target.GeometricAgreement);
        }

        [Test]
        public void KappaTest2()
        {
            int[,] matrix =
            {
                { 24, 14 },
                {  8, 24 },
            };

            GeneralConfusionMatrix target = new GeneralConfusionMatrix(matrix);


            Assert.AreEqual(2, target.Classes);
            Assert.AreEqual(70, target.Samples);

            double[,] p =
            {
                { 0.343, 0.200 },
                { 0.114, 0.343 }
            };

            Assert.IsTrue(p.IsEqual(target.ProportionMatrix, 1e-3));

            Assert.AreEqual(0.6857143, target.OverallAgreement, 1e-5);
            Assert.AreEqual(0.4963265, target.ChanceAgreement, 1e-5);
            Assert.AreEqual(0.376013, target.Kappa, 1e-5);
            Assert.AreEqual(0.1087717, target.StandardError, 1e-5);
            Assert.AreEqual(24, target.GeometricAgreement, 1e-5);
        }

        [Test]
        public void KappaTest4()
        {
            // Example from Congalton

            int[,] table = // Analyst #1 (page 108)
            {
                { 65,  4, 22, 24 },
                {  6, 81,  5,  8 },
                {  0, 11, 85, 19 },
                {  4,  7,  3, 90 },
            };

            GeneralConfusionMatrix target = new GeneralConfusionMatrix(table);

            Assert.AreEqual(target.RowTotals[0], 115);
            Assert.AreEqual(target.RowTotals[1], 100);
            Assert.AreEqual(target.RowTotals[2], 115);
            Assert.AreEqual(target.RowTotals[3], 104);

            Assert.AreEqual(target.ColumnTotals[0], 75);
            Assert.AreEqual(target.ColumnTotals[1], 103);
            Assert.AreEqual(target.ColumnTotals[2], 115);
            Assert.AreEqual(target.ColumnTotals[3], 141);


            Assert.AreEqual(0.65, target.Kappa, 1e-2);
            Assert.IsFalse(Double.IsNaN(target.Kappa));


            double var = target.Variance;
            double var0 = target.VarianceUnderNull;
            double varD = Accord.Statistics.Testing.KappaTest.DeltaMethodKappaVariance(target);

            Assert.AreEqual(0.0007778, varD, 1e-7);
            Assert.AreEqual(0.00076995084473426684, var, 1e-10);
            Assert.AreEqual(0.00074886435981842887, var0, 1e-10);

            Assert.IsFalse(double.IsNaN(var));
            Assert.IsFalse(double.IsNaN(var0));
            Assert.IsFalse(double.IsNaN(varD));
        }

        [Test]
        public void KappaTest5()
        {
            // Example from University of York Department of Health Sciences,
            // Measurement in Health and Disease, Cohen's Kappa
            // http://www-users.york.ac.uk/~mb55/msc/clinimet/week4/kappash2.pdf

            // warning: the paper seems to use an outdated variance formula for kappa.


            int[,] matrix =
            {
                { 61,  2 },
                {  6, 25 },
            };

            GeneralConfusionMatrix target = new GeneralConfusionMatrix(matrix);


            Assert.AreEqual(2, target.Classes);
            Assert.AreEqual(94, target.Samples);

            Assert.AreEqual(0.801, target.Kappa, 1e-4);
            Assert.AreEqual(0.067, target.StandardError, 1e-3);
        }


        [Test]
        public void KappaVarianceTest1()
        {
            // Example from Ientilucci, Emmett (2006). "On Using and Computing the Kappa Statistic".
            // Available on: http://www.cis.rit.edu/~ejipci/Reports/On_Using_and_Computing_the_Kappa_Statistic.pdf 

            // Note: Congalton's method uses the Delta Method for approximating the Kappa variance.

            {
                int[,] matrix = // Matrix A (page 1)
                {
                    { 317,  23,  0,  0 },
                    {  61, 120,  0,  0 },
                    {   2,   4, 60,  0 },
                    {  35,  29,  0,  8 },
                };

                GeneralConfusionMatrix a = new GeneralConfusionMatrix(matrix);

                // Method A row totals (page 2)
                Assert.AreEqual(340, a.RowTotals[0]);
                Assert.AreEqual(181, a.RowTotals[1]);
                Assert.AreEqual(66, a.RowTotals[2]);
                Assert.AreEqual(72, a.RowTotals[3]);

                // Method A col totals (page 2)
                Assert.AreEqual(415, a.ColumnTotals[0]);
                Assert.AreEqual(176, a.ColumnTotals[1]);
                Assert.AreEqual(60, a.ColumnTotals[2]);
                Assert.AreEqual(8, a.ColumnTotals[3]);

                // Number of samples for A (page 2)
                Assert.AreEqual(659, a.Samples);
                Assert.AreEqual(4, a.Classes);

                // Po for A (page 2)
                Assert.AreEqual(0.7663, a.OverallAgreement, 1e-4);
                Assert.IsFalse(double.IsNaN(a.OverallAgreement));

                // Pc for A (page 3)
                Assert.AreEqual(0.4087, a.ChanceAgreement, 1e-5);
                Assert.IsFalse(double.IsNaN(a.ChanceAgreement));



                // Kappa value k_hat for A (page 3)
                Assert.AreEqual(0.605, a.Kappa, 1e-3);
                Assert.IsFalse(double.IsNaN(a.Kappa));

                double var = a.Variance;
                double var0 = a.VarianceUnderNull;
                double varD = Accord.Statistics.Testing.KappaTest.DeltaMethodKappaVariance(a);

                // Variance value var_k for A (page 4)
                Assert.AreEqual(0.00073735, varD, 1e-8);


                Assert.AreEqual(0.00071760415564207924, var, 1e-10);
                Assert.AreEqual(0.00070251065008366978, var0, 1e-10);

                Assert.IsFalse(double.IsNaN(var));
                Assert.IsFalse(double.IsNaN(var0));
                Assert.IsFalse(double.IsNaN(varD));
            }

            {
                int[,] matrix = // Matrix B
                {
                    { 377,  79,  0,  0 },
                    {   2,  72,  0,  0 },
                    {  33,   5, 60,  0 },
                    {   3,  20,  0,  8 },
                };

                GeneralConfusionMatrix b = new GeneralConfusionMatrix(matrix);

                // Method B row totals (page 2)
                Assert.AreEqual(456, b.RowTotals[0]);
                Assert.AreEqual(74, b.RowTotals[1]);
                Assert.AreEqual(98, b.RowTotals[2]);
                Assert.AreEqual(31, b.RowTotals[3]);

                // Method B col totals (page 2)
                Assert.AreEqual(415, b.ColumnTotals[0]);
                Assert.AreEqual(176, b.ColumnTotals[1]);
                Assert.AreEqual(60, b.ColumnTotals[2]);
                Assert.AreEqual(8, b.ColumnTotals[3]);


                // Number of samples for B (page 2)
                Assert.AreEqual(659, b.Samples);
                Assert.AreEqual(4, b.Classes);

                // Po for B (page 2)
                Assert.AreEqual(0.7845, b.OverallAgreement, 1e-4);
                Assert.IsFalse(double.IsNaN(b.OverallAgreement));

                // Pc for B (page 3)
                Assert.AreEqual(0.47986, b.ChanceAgreement, 1e-5);
                Assert.IsFalse(double.IsNaN(b.ChanceAgreement));


                // Kappa value k_hat for B (page 3)
                Assert.AreEqual(0.586, b.Kappa, 1e-3);
                Assert.IsFalse(double.IsNaN(b.Kappa));


                double var = b.Variance;
                double var0 = b.VarianceUnderNull;
                double varD = Accord.Statistics.Testing.KappaTest.DeltaMethodKappaVariance(b);

                // Variance value var_k for A (page 4)
                Assert.AreEqual(0.00087457, varD, 1e-8);


                Assert.AreEqual(0.00083016849579382347, var, 1e-10);
                Assert.AreEqual(0.00067037111046188824, var0, 1e-10);

                Assert.IsFalse(double.IsNaN(var));
                Assert.IsFalse(double.IsNaN(var0));
                Assert.IsFalse(double.IsNaN(varD));
            }
        }

        [Test]
        public void KappaVarianceTest2()
        {
            // Example from http://vassarstats.net/kappa.html

            // Checked against http://graphpad.com/quickcalcs/Kappa2.cfm     (OK)

            int[,] matrix =
            {
                { 44,  5,  1 },
                {  7, 20,  3 },
                {  9,  5,  6 },
            };

            GeneralConfusionMatrix a = new GeneralConfusionMatrix(matrix);

            Assert.AreEqual(a.RowTotals[0], 50);
            Assert.AreEqual(a.RowTotals[1], 30);
            Assert.AreEqual(a.RowTotals[2], 20);

            Assert.AreEqual(a.ColumnTotals[0], 60);
            Assert.AreEqual(a.ColumnTotals[1], 30);
            Assert.AreEqual(a.ColumnTotals[2], 10);


            Assert.AreEqual(0.4915, a.Kappa, 1e-4);
            Assert.IsFalse(double.IsNaN(a.Kappa));

            double var = a.Variance;
            double var0 = a.VarianceUnderNull;
            double varD = Accord.Statistics.Testing.KappaTest.DeltaMethodKappaVariance(a);

            double se = System.Math.Sqrt(var);
            double se0 = System.Math.Sqrt(var0);
            double seD = System.Math.Sqrt(varD);

            Assert.AreEqual(0.072, a.StandardError, 0.0005);

            Assert.AreEqual(0.073534791185213152, seD, 1e-10);
            Assert.AreEqual(0.073509316753225237, se0, 1e-10);

            Assert.IsFalse(double.IsNaN(se));
            Assert.IsFalse(double.IsNaN(se0));
            Assert.IsFalse(double.IsNaN(seD));
        }

        [Test]
        public void KappaVarianceTest3()
        {
            // Example from J. L. Fleiss, J. Cohen, B. S. Everitt, "Large sample
            //  standard errors of kappa and weighted kappa" Psychological Bulletin (1969)
            //  Volume: 72, Issue: 5, American Psychological Association, Pages: 323-327

            // This was the paper which presented the finally correct
            // large sample variance for Kappa after so many attempts.


            double[,] matrix =
            {
                { 0.53,  0.05,  0.02 },
                { 0.11,  0.14,  0.05 },
                { 0.01,  0.06,  0.03 },
            };

            GeneralConfusionMatrix a = new GeneralConfusionMatrix(matrix, 200);

            Assert.AreEqual(a.RowProportions[0], .60, 1e-10);
            Assert.AreEqual(a.RowProportions[1], .30, 1e-10);
            Assert.AreEqual(a.RowProportions[2], .10, 1e-10);

            Assert.AreEqual(a.ColumnProportions[0], .65, 1e-10);
            Assert.AreEqual(a.ColumnProportions[1], .25, 1e-10);
            Assert.AreEqual(a.ColumnProportions[2], .10, 1e-10);


            Assert.AreEqual(0.429, a.Kappa, 1e-3);
            Assert.IsFalse(double.IsNaN(a.Kappa));


            Assert.AreEqual(0.002885, a.Variance, 1e-6);
            Assert.AreEqual(0.003082, a.VarianceUnderNull, 1e-6);


            Assert.IsFalse(double.IsNaN(a.Variance));
            Assert.IsFalse(double.IsNaN(a.VarianceUnderNull));
        }

        [Test]
        public void TotalTest()
        {
            int[,] matrix =
            {
                { 1, 2, 3 },
                { 4, 5, 6 },
                { 7, 8, 9 },
            };

            GeneralConfusionMatrix target = new GeneralConfusionMatrix(matrix);

            int[] colTotals = target.ColumnTotals;
            int[] rowTotals = target.RowTotals;

            Assert.AreEqual(1 + 2 + 3, rowTotals[0]);
            Assert.AreEqual(4 + 5 + 6, rowTotals[1]);
            Assert.AreEqual(7 + 8 + 9, rowTotals[2]);

            Assert.AreEqual(1 + 4 + 7, colTotals[0]);
            Assert.AreEqual(2 + 5 + 8, colTotals[1]);
            Assert.AreEqual(3 + 6 + 9, colTotals[2]);
        }

        [Test]
        public void GeometricAgreementTest()
        {
            int[,] matrix =
            {
                { 462,  241 },
                { 28,    59 },
            };

            GeneralConfusionMatrix target = new GeneralConfusionMatrix(matrix);

            double actual = target.GeometricAgreement;
            double expected = Math.Sqrt(462 * 59);
            Assert.AreEqual(expected, actual, 1e-10);
            Assert.IsFalse(Double.IsNaN(actual));
        }

        [Test]
        public void ChiSquareTest()
        {
            int[,] matrix =
            {
                {  10,      9,      5,      7,      8     },
                {   1,      2,      0,      1,      2     },
                {   0,      0,      1,      0,      1     },
                {   1,      0,      0,      3,      0     },
                {   0,      2,      0,      0,      2     },
            };

            GeneralConfusionMatrix target = new GeneralConfusionMatrix(matrix);

            double actual = target.ChiSquare;

            Assert.AreEqual(19.43, actual, 0.01);
            Assert.IsFalse(Double.IsNaN(actual));
        }

        [Test]
        public void ChiSquareTest2()
        {
            int[,] matrix =
            {
                { 296, 2, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                {   0, 293, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 1, 1, 0, 0, 1 },
                {   1, 0, 274, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20 },
                {   1, 0, 0, 278, 0, 1, 0, 0, 7, 4, 2, 1, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 1, 2 },
                {   0, 1, 0, 0, 290, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 7 },
                {   0, 4, 1, 7, 2, 263, 0, 0, 0, 2, 0, 3, 0, 0, 0, 1, 0, 0, 0, 13, 0, 0, 0, 0, 0, 0, 4 },
                {   5, 7, 1, 29, 1, 20, 0, 0, 10, 10, 49, 28, 1, 6, 0, 4, 1, 29, 0, 21, 15, 9, 3, 4, 0, 32, 15 },
                {   0, 7, 0, 23, 9, 37, 0, 0, 0, 19, 34, 4, 8, 2, 0, 1, 6, 13, 0, 13, 53, 8, 6, 1, 0, 46, 10 },
                {   0, 0, 0, 0, 0, 0, 0, 0, 298, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0 },
                {   2, 1, 0, 2, 2, 0, 0, 0, 11, 250, 0, 0, 5, 2, 0, 3, 4, 0, 0, 1, 1, 0, 0, 5, 0, 11, 0 },
                {   1, 3, 0, 0, 2, 3, 0, 0, 0, 1, 251, 4, 0, 0, 0, 0, 0, 1, 2, 2, 1, 11, 10, 0, 0, 6, 2 },
                {   0, 0, 0, 4, 0, 2, 0, 0, 0, 0, 2, 291, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
                {   0, 0, 0, 0, 1, 0, 0, 0, 0, 2, 0, 0, 278, 10, 0, 0, 5, 0, 0, 0, 0, 0, 0, 2, 0, 2, 0 },
                {   0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 14, 279, 0, 0, 3, 0, 0, 0, 0, 0, 0, 2, 0, 2, 0 },
                {   0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 292, 0, 0, 0, 0, 0, 1, 0, 0, 3, 0, 0, 1 },
                {   0, 0, 2, 3, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 289, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2, 0 },
                {   0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 1, 3, 0, 0, 289, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0 },
                {   0, 0, 0, 2, 0, 0, 0, 0, 1, 0, 2, 2, 0, 0, 0, 0, 0, 276, 0, 2, 10, 0, 0, 0, 0, 4, 1 },
                {   4, 0, 0, 0, 7, 2, 0, 0, 1, 2, 0, 0, 3, 0, 2, 0, 0, 0, 274, 0, 0, 0, 0, 2, 0, 2, 1 },
                {   0, 0, 0, 0, 0, 25, 0, 0, 0, 0, 1, 0, 0, 0, 0, 8, 0, 0, 0, 262, 0, 0, 2, 0, 0, 1, 1 },
                {   0, 2, 0, 0, 0, 0, 0, 0, 1, 0, 2, 0, 0, 0, 0, 0, 0, 10, 0, 2, 278, 2, 0, 0, 0, 3, 0 },
                {   0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4, 273, 4, 0, 0, 2, 1 },
                {   0, 1, 1, 0, 1, 0, 0, 0, 0, 3, 7, 1, 1, 0, 0, 0, 0, 0, 0, 1, 2, 2, 277, 0, 0, 2, 1 },
                {   0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 7, 5, 0, 6, 4, 0, 0, 0, 1, 0, 0, 259, 0, 10, 0 },
                {   14, 13, 0, 14, 0, 28, 0, 0, 6, 94, 10, 2, 1, 10, 0, 8, 7, 0, 0, 16, 0, 16, 24, 5, 0, 32, 0 },
                {   0, 2, 1, 9, 0, 1, 0, 0, 0, 22, 7, 3, 11, 0, 1, 5, 1, 6, 0, 2, 4, 1, 2, 7, 0, 214, 1 },
                {   0, 1, 13, 0, 5, 0, 0, 0, 0, 0, 0, 0, 1, 0, 3, 0, 0, 0, 3, 1, 1, 0, 0, 0, 0, 0, 272 },
            };

            GeneralConfusionMatrix target = new GeneralConfusionMatrix(matrix);

            double actual = target.ChiSquare;
            Assert.IsTrue(Double.IsNaN(actual));

            Assert.AreEqual(0, target.GeometricAgreement);
            Assert.AreEqual(matrix.Diagonal().Sum() / (double)target.Samples, target.OverallAgreement);
        }


        [Test]
        public void example()
        {
            // Example for https://github.com/accord-net/framework/issues/669
            // >>>>> Note: Suggestions on how to improve this API are welcome!
            // If you find the method below too confusing, please suggest new
            // ways on how the same could be accomplished, even if they would
            // require new APIs.

            string[] expectedLabels = { "A", "A", "B", "C", "A", "B", "B" };
            string[] predictedLabels = { "A", "B", "C", "C", "A", "C", "B" };

            // Create a codification object to translate char into symbols
            var codification = new Codification("Labels", expectedLabels);
            int[] expected = codification.Transform(expectedLabels);   // ground truth data
            int[] predicted = codification.Transform(predictedLabels); // predicted from OCR

            // Create a new confusion matrix for multi-class problems
            var cm = new GeneralConfusionMatrix(expected, predicted);

            int[] rowErrors = cm.RowErrors;
            int[] colErrors = cm.ColumnErrors;

            double[] rowPrecision = cm.Precision;
            double[] colRecall = cm.Recall;

            int[] colTotal = cm.ColumnTotals;
            int[] rowTotal = cm.RowTotals;

            // Obtain relevant measures
            int[,] matrix = cm.Matrix;
            int[] tp = cm.PerClassMatrices.Apply(x => x.TruePositives);
            int[] tn = cm.PerClassMatrices.Apply(x => x.TrueNegatives);
            int[] fp = cm.PerClassMatrices.Apply(x => x.FalsePositives);
            int[] fn = cm.PerClassMatrices.Apply(x => x.FalseNegatives);
            double[] precision2 = cm.PerClassMatrices.Apply(x => x.Precision);
            double[] recall2 = cm.PerClassMatrices.Apply(x => x.Recall);
            double[] fscore = cm.PerClassMatrices.Apply(x => x.FScore);

            object[,] column01 = Matrix.ColumnVector(codification.Columns[0].Values).ToObject();
            object[,] columns2_to_4 = matrix.ToObject();
            object[,] column05 = Matrix.ColumnVector(colErrors).ToObject();
            object[,] column06 = Matrix.ColumnVector(colRecall).ToObject();
            object[,] column07 = Matrix.ColumnVector(colTotal).ToObject();
            object[,] column08 = Matrix.ColumnVector(tp).ToObject();
            object[,] column09 = Matrix.ColumnVector(tn).ToObject();
            object[,] column10 = Matrix.ColumnVector(tp).ToObject();
            object[,] column11 = Matrix.ColumnVector(fn).ToObject();
            object[,] column12 = Matrix.ColumnVector(precision2).ToObject();
            object[,] column13 = Matrix.ColumnVector(recall2).ToObject();
            object[,] column14 = Matrix.ColumnVector(fscore).ToObject();

            object[,] values = Matrix.Concatenate(
                column01,
                columns2_to_4,
                column05,
                column06,
                column07,
                column08,
                column09,
                column10,
                column11,
                column12,
                column13,
                column14
            );

            object[] row05 = Matrix.Concatenate<object>("Error", colErrors.ToObject());
            object[] row06 = Matrix.Concatenate<object>("Precision", rowPrecision.ToObject());
            object[] row07 = Matrix.Concatenate<object>("Total", colTotal.ToObject());

            values = values.InsertRow(row05)
                .InsertRow(row06)
                .InsertRow(row07);


            // Name of each of the columns in order to create a data table
            string[] columnNames = "Label".Concatenate(codification.Columns[0].Values.Concatenate(
                "Error", "Recall", "Total", "TP", "TN", "FP", "FN", "Precision2", "Recall2", "F-Score"));

            // Create a table from the matrix and columns
            DataTable table = values.ToTable(columnNames);


            string[] actualNames;
            object[,] actualTable = table.ToMatrix<object>(out actualNames);

            actualTable = actualTable.InsertRow(columnNames, 0);

            string str = actualTable.ToCSharp();

            object[,] expectedTable =
            {
                { "Label",    "A", "B", "C", "Error", "Recall", "Total", "TP", "TN", "FP", "FN",    "Precision2",     "Recall2",       "F-Score" },
                {   "A",       2,   1,   0,    0,        0,       2,      2,    4,    2,    1,  1.000000000000000, 66666666666666663,     0.8   },
                {   "B",       0,   1,   2,    1,        0,       2,      1,    3,    1,    2,  0.500000000000000, 0.333333333333333,     0.4   },
                {   "C",       0,   0,   1,    2,        1,       3,      1,    4,    1,    0,  0.333333333333333, 1.000000000000000,     0.5   },
                { "Error",     0,   1,   2,   null,     null,    null,   null, null, null, null,        null,           null,             null  },
                { "Precision", 1,   0,   0,   null,     null,    null,   null, null, null, null,        null,           null,             null  },
                { "Total",     2,   2,   3,   null,     null,    null,   null, null, null, null,        null,           null,             null  }
            };

            Assert.IsTrue(expectedTable.IsEqual(actualTable, atol: 1e-10));
        }

    }
}
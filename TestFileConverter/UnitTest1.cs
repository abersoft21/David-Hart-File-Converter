using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using David_Hart_File_Converter;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace TestFileConverter
{
    [TestClass]
    public class UnitTest1
    {
        private IntermediateStructure inFile;
        private string[] standardCsvHeader;
        private string[] groupedCsvHeader;


        [TestInitialize]
        public void Init()
        {
            inFile = new IntermediateStructure();

            standardCsvHeader = new string[3] { "Name", "Street", "Town" };
            groupedCsvHeader = new string[3] { "Name", "Address_Street", "Address_Town" };
        }

        [TestMethod]
        public void TestTwoDimensionalHeader()
        {
            inFile.ParseCsvHeaders(standardCsvHeader);

            var actual = inFile.ListGroupHeaders;

            var expected = standardCsvHeader.Select(sh =>
                new KeyValuePair<string, List<string>>(sh, null))
                .ToList();

            CollectionAssert.AreEqual((ICollection)expected, (ICollection)actual);

        }
        [TestMethod]
        public void TestStructuredHeader()
        {
            inFile.ParseCsvHeaders(groupedCsvHeader);

            var actual = inFile.ListGroupHeaders;

            var name = new KeyValuePair<string, List<string>>(groupedCsvHeader[0], null);
            var address = new KeyValuePair<string, List<string>>("Address", new List<string>() { "Street", "Town" });

            var expected = new List<KeyValuePair<string, List<string>>>();
            expected.Add(name);
            expected.Add(address);

            Assert.AreEqual(expected[0], actual[0]);
            Assert.AreEqual(expected[01].Key, actual[1].Key); // address group
            Assert.AreEqual(expected[01].Value.First(), actual[01].Value.First()); // address group Street
            Assert.AreEqual(expected[01].Value.Last(), actual[01].Value.Last()); // address group Town
        }

    }
}

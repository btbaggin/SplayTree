using System;
using SplayTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SplayTree
{
    class SplayTreeTests
    {
        [TestClass]
        public class Tests
        {
            [TestMethod]
            public void CountTest()
            {
                SplayTree<int> s = new SplayTree<int>();
                s.Add(5);
                Assert.AreEqual(s.Count, 1);
                s.Add(1);
                Assert.AreEqual(s.Count, 2);
                s.Add(2);
                Assert.AreEqual(s.Count, 3);
            }

            [TestMethod]
            public void ContainsTest()
            {
                SplayTree<int> s = new SplayTree<int>();
                s.Add(5);
                s.Add(1);
                s.Add(2);
                Assert.IsTrue(s.Contains(5));
                Assert.IsFalse(s.Contains(3));
            }

            [TestMethod]
            public void MinimumMaximumTest()
            {
                SplayTree<int> s = new SplayTree<int>();
                s.Add(5);
                s.Add(1);
                s.Add(2);
                Assert.AreEqual(s.Maximum(), 5);
                Assert.AreEqual(s.Minimum(), 1);
            }


            [TestMethod]
            public void EnumTest()
            {
                SplayTree<int> s = new SplayTree<int>();
                s.Add(5);
                s.Add(1);
                s.Add(2);
                s.Add(10);
                s.Add(3);
                s.Add(-2);

                int prev = -9999;
                foreach (int i in s)
                {
                    Assert.IsTrue(prev < i);
                    prev = i;
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Kebler.Models;
using NUnit.Framework;

namespace Kebler.Test
{
    public class CatTest
    {
        [Test]
        public void CatEquals_true()
        {
            var first = new FolderCategory("/download/");
            var second = new FolderCategory("/download/");

            var result = first.Equals(second);
            Assert.IsTrue(result);
            Assert.Pass();
        }

        [Test]
        public void CatEqualsSep_true()
        {
            var first = new FolderCategory("/download");
            var second = new FolderCategory("/download/");

            var result = first.Equals(second);
            Assert.IsTrue(result);
            Assert.Pass();
        }

        [Test]
        public void CatEqualsSep_false()
        {
            var first = new FolderCategory("/download/download");
            var second = new FolderCategory("/download/");

            var result = first.Equals(second);
            Assert.IsFalse(result);
            Assert.Pass();
        }

        [Test]
        public void CatEquals_false()
        {
            var first = new FolderCategory("/download/");
            var second = new FolderCategory("pub/download/");

            var result = first.Equals(second);
            Assert.IsFalse(result);
            Assert.Pass();
        }

        [Test]
        public void CatExcept()
        {
            var first = new List<FolderCategory>
            {
                new FolderCategory("/download/"), //
                new FolderCategory("/download/Films"),
                new FolderCategory("/download/Films/4K"), //
                new FolderCategory("/download/Serials"),
                new FolderCategory("/download/data/Films"), //
                new FolderCategory("/download/data/Films/download") //
            };

            var second = new List<FolderCategory>
            {
                new FolderCategory("/download/data/Films"),
                new FolderCategory("/download/Films/4K"),
                new FolderCategory("/download/"),
                new FolderCategory("/download/data/Films/download")
            };


            var result = first.Except(second).ToList();
            var f = result[0].Equals(first[1]);
            var s = result[1].Equals(first[3]);

            Assert.IsTrue(result.Count() == 2);
            Assert.IsTrue(f);
            Assert.IsTrue(s);


            Assert.Pass();
        }
    }
}
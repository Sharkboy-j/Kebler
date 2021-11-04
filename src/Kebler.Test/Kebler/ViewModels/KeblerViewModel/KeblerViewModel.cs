using NUnit.Framework;
using System;
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Kebler.Test.Kebler.ViewModels.KeblerViewModel
{
    internal class KeblerViewModel
    {

        //private readonly global::Kebler.ViewModels.KeblerViewModel keblerViewModel;

        [Test]
        public void Test_InitViwModel_ShouldThrow()
        {
            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                var model = new global::Kebler.ViewModels.KeblerViewModel(default!);
            });
        }
    }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
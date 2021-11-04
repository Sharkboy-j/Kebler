using Kebler.Models;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Kebler.Test.Kebler.ViewModels.KeblerViewModel
{
    public class TitleHeaderMenuActionsTest
    {
        private readonly global::Kebler.ViewModels.KeblerViewModel KeblerViewModel;

        public TitleHeaderMenuActionsTest()
        {
            KeblerViewModel = new global::Kebler.ViewModels.KeblerViewModel();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Test_InitMenuAction_ShouldThrowWhenOneOfArgsIsNull()
        {
            //Arrange
            void LanguageChanged(object sender, RoutedEventArgs e) { }

            //Assert
            Assert.Throws<ArgumentNullException>(() => KeblerViewModel.InitMenu(new[] { new CultureInfo(1), }, default!, default!));
            Assert.Throws<ArgumentNullException>(() => KeblerViewModel.InitMenu(default!, LanguageChanged, default!));
            Assert.Throws<ArgumentNullException>(() => KeblerViewModel.InitMenu(default!, default!, new CultureInfo("EN-us")));
            Assert.Throws<ArgumentNullException>(() => KeblerViewModel.InitMenu(default!, default!, default!));

        }

        [Test, Apartment(ApartmentState.STA)]
        public void Test_InitMenuAction_ShouldReturnMenuItemWithCorrectLanguage()
        {
            //Arrange
            void LanguageChanged(object sender, RoutedEventArgs e) { }

            //Act
            var result = KeblerViewModel.InitMenu(new[] { new CultureInfo("EN-us"), }, LanguageChanged, new CultureInfo("EN-us"));

            //Assert
            Assert.True(result.Count() == 1);
            var menuItem = result.First();

            Assert.True(char.IsUpper(menuItem.Header.ToString().ToCharArray()[0]));
            Assert.True(menuItem.IsCheckable);
            Assert.True(menuItem.IsChecked);
        }




        [Test, Apartment(ApartmentState.STA)]
        public void Test_ReInitServersAction_ShouldThrowWhenOneOfArgsIsNull()
        {
            //Arrange
            void ServerChanged(object sender, RoutedEventArgs e) { }

            //Assert
            Assert.Throws<ArgumentNullException>(() => KeblerViewModel.ReInitServers(new[] { new Server() }, default!));
            Assert.Throws<ArgumentNullException>(() => KeblerViewModel.ReInitServers(default!, ServerChanged));
            Assert.Throws<ArgumentNullException>(() => KeblerViewModel.ReInitServers(default!, default!));

        }


        [Test, Apartment(ApartmentState.STA)]
        public void Test_ReInitServersAction_ShouldReturnMenuItemWithCorrectServer()
        {
            //Arrange
            void ServerChanged(object sender, RoutedEventArgs e) { }
            var server = new Server()
            {
                Id = 1
            };

            //Act
            var result = KeblerViewModel.ReInitServers(new[] {server}, ServerChanged);

            //Assert
            Assert.True(result.Count() == 1);
            var menuItem = result.First();

            Assert.NotNull(menuItem.Header);
            Assert.NotNull(menuItem.Tag);
            Assert.AreEqual(menuItem.Tag, server);
            Assert.True(menuItem.IsCheckable);
        }
    }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.

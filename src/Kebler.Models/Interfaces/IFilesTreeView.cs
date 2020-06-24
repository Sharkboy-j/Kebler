using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Kebler.Models.Interfaces
{
    public interface IFilesTreeView
    {
        uint[] getFilesWantedStatus(bool status);

    }
}

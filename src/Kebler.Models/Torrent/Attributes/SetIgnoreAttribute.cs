using System;

namespace Kebler.Models.Torrent.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class SetIgnoreAttribute : Attribute
    {
    }
}
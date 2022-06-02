using System;

namespace Kebler.Transmission.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class SetIgnoreAttribute : Attribute
    {
    }
}
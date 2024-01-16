using System;

namespace CryptoDepth.Application.Paggination
{
    public class SortableAttribute : Attribute
    {
        public string OrderBy { get; set; }
    }
}

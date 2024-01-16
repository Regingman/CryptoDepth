using System;
using System.ComponentModel.DataAnnotations;

namespace CryptoDepth.Domain.Entities.Base
{
    /// <summary>
    /// Базовый класс GUID
    /// </summary>
    public class BaseEntityGuid 
    {
        [Key]
        public Guid Id { get; set; }
    }
}

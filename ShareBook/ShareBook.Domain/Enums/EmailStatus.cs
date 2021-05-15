using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace ShareBook.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EmailStatus
    {
        [Description("Em fila")]
        Queued,

        [Description("Email enviado")]
        Sent,

        [Description("Cenário inesperado")]
        NotSent
    }
}
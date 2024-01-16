using System.Collections.Generic;
using System.Linq;

namespace CryptoDepth.Domain.Data
{
    public class Result
    {
        public Result(bool succeeded, IEnumerable<string> messages)
        {
            Succeeded = succeeded;
            Messages = messages.ToArray();
        }

        public bool Succeeded { get; set; }
        public string[] Messages { get; set; }

        public static Result Success(List<string> messages = null)
        {
            messages ??= new List<string>();
            messages.Add("Успешно: процедура выполнилась");
            return new Result(true, messages);
        }

        public static Result Failure(string error) => new(false, new List<string> { error });

        public static Result Failure(IEnumerable<string> errors) => new(false, errors);

        public static Result Success(string success) => new(true, new List<string> { success });
    }
}
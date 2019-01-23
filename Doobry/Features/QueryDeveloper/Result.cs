using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Doobry.Features.QueryDeveloper
{
    public class Result
    {
        public Result(int row, string data)
        {
            DocumentId id;
            DocumentId.TryParse(data, out id);
            Id = id;

            Row = row;
            Data = JValue.Parse(data).ToString(Formatting.Indented);
        }

        public DocumentId Id { get; }

        public int Row { get; }

        public string Data { get; }

    }
}
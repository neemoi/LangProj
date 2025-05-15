using System.Text.Json.Serialization;

namespace Application.DtoModels.Functions
{
    public class FunctionWordUpdateDto
    {
        public int Id { get; set; }

        public string? name { get; set; }
    }
}

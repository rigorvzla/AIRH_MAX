namespace LLM_ServerMain.Models
{
    public class ModelResponse
    {
        public string model_response { get; set; }
        public string user_command { get; set; }
        public ModelResponseExtra user_command_extra { get; set; }
    }
}

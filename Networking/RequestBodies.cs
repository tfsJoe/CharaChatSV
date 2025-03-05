using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CharaChatSV
{
    /* The props are defined by the parameters of the API endpoints, which may differ depending on which model is used.
     See https://platform.openai.com/docs/guides/chat/introduction
     The classes themselves are to be used with IModHelper.Data.ReadJsonFile. The prop names must match the JSON.
     This is why their capitalization does not follow C# conventions. They must also be properties, not fields.
     Disabling many related warnings. */
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
    // Resharper disable MemberCanBePrivate.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable ClassNeverInstantiated.Global

    public abstract class RequestBody
    {
        public string model { get; set; }
        public float temperature { get; set; }
        public int max_tokens { get; set; }
        public float top_p { get; set; }
        public float frequency_penalty { get; set; }
        public float presence_penalty { get; set; }
        
        private readonly StringBuilder sb = new();
        
        public override string ToString()
        {
            sb.Clear();
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(this))
            {
                sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append('\n');
            }
            return sb.ToString();
        }
    }

    public sealed class DaVinciRequestBody : RequestBody
    {
        public string prompt { get; set; }
    }

    public class GptMessage
    {
        public string role { get; set; }
        public string content { get; set; }

        public enum Role
        {
            developer,
            user,
            assistant
        }
        
        public GptMessage() {}

        public GptMessage(Role role, string content)
        {
            this.role = role.ToString();
            this.content = content;
        }
    }
    
    public sealed class GptRequestBody : RequestBody
    {
        public List<GptMessage> messages { get; set; }
    }
}

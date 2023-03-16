using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace StardewChatter
{
    /* The props are defined by the parameters of the API endpoints, which may differ depending on which model is used.
     See https://platform.openai.com/docs/guides/chat/introduction
     The classes themselves are to be used with IModHelper.Data.ReadJsonFile. The prop names must match the JSON. */
    
    public abstract class RequestBody
    {
        public string model { get; set; }
        public float temperature { get; set; }
        public int max_tokens { get; set; }
        public float top_p { get; set; }
        public float frequency_penalty { get; set; }
        public float presence_penalty { get; set; }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
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

    public class TurboMessage
    {
        public string role { get; set; }
        public string content { get; set; }

        public enum Role
        {
            system,
            user,
            assistant
        }
        
        public TurboMessage() {}

        public TurboMessage(Role role, string content)
        {
            this.role = role.ToString();
            this.content = content;
        }
    }
    
    public sealed class TurboRequestBody : RequestBody
    {
        public List<TurboMessage> messages { get; set; }
    }
}

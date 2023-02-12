using System.ComponentModel;
using System.Linq;
using System.Text;

namespace StardewChatter
{
    public sealed class Gpt3RequestBody
    {
        public string model { get; set; }
        public string prompt { get; set; }
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
}

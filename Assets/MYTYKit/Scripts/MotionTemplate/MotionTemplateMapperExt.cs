using System.Linq;
using Newtonsoft.Json.Linq;

namespace MYTYKit.MotionTemplates
{
    public static class MotionTemplateMapperExt
    {
        public static JObject SerializeToJObject(this MotionTemplateMapper mapper)
        {
            var names = mapper.GetNames();
            return JObject.FromObject(new
            {
                templates = names.Select(name => (name, type: mapper.GetTemplate(name).GetType().Name))
            });
        }
    }
}
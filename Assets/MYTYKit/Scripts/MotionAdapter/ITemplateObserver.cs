using MYTYKit.MotionTemplates;

namespace MYTYKit.MotionAdapters
{
    public interface ITemplateObserver
    {
        public void TemplateUpdated();
        public void ListenToMotionTemplate();
    }
}
namespace MYTYKit.MotionTemplates
{
    public static class MotionSourceHelper
    {
        public static void SetupMotionCategory(this MotionSource motionSource)
        {
            motionSource.Clear();
            var transform = motionSource.transform;
            var childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i).gameObject;
                var categoryName = child.name;

                for (int j = 0; j < child.transform.childCount; j++)
                {
                    var bridge = child.transform.GetChild(j).GetComponent<MotionTemplateBridge>();
                    if (bridge == null) continue;
                    motionSource.AddMotionTemplateBridge(categoryName, bridge);
                }
            }

        }

    }
}
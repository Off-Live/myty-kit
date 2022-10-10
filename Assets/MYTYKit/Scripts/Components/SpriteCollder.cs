using UnityEngine;

namespace MYTYKit.Components
{
    public class SpriteCollder : MonoBehaviour
    {
        // Start is called before the first frame update
        public SpriteRenderer sprite;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (sprite != null)
            {
                gameObject.transform.position = sprite.bounds.center;
            }
        }
    }
}

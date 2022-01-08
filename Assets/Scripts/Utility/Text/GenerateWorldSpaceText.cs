using UnityEngine;

namespace Utility.Text
{
    public static class GenerateWorldSpaceText
    {
        public static void CreateWorldSpaceTextPopup(string text, Vector3 location, float speed, float duration,
            Color color, float scale = 1)

        {
            var textPopup = ObjectPooling.Instance.RequestComponentFromPool<TextPopup>();

            textPopup.textMeshComponent.text = text;
            textPopup.duration = duration;
            textPopup.speed = speed;
            textPopup.textMeshComponent.color = color;
            //obj.SetActive(true);
            textPopup.gameObject.SetActive(true);
            var transform = textPopup.transform;
            
            transform.localScale *= scale;
            //var position = obj.transform.position;
            var position = location;
            transform.position = new Vector3(position.x, position.y + 3,
                position.z);
        }
    }
    

    

}

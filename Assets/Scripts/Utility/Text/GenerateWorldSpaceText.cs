using UnityEngine;

namespace Utility.Text
{
    public static class GenerateWorldSpaceText
    {
        public static TextPopup CreateWorldSpaceTextPopup(string text, Vector3 location, float speed, float duration,
            Color color, float scale = 1, float yHeightOffset = 3, bool randomiseOffsetPosition = false)

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
            var randomOffsetX = Random.Range(-2f, 2f);
            var original = yHeightOffset;
            yHeightOffset += Random.Range(-2f, 2f);
            var randomOffsetZ = Random.Range(-2f, 2f);
            if (!randomiseOffsetPosition)
            {
                yHeightOffset = original;
                randomOffsetX = 0;
                randomOffsetZ = 0;
            }
            transform.position = new Vector3(position.x+randomOffsetX, position.y + yHeightOffset,
                position.z+randomOffsetZ);
            return textPopup;
        }
    }
    

    

}

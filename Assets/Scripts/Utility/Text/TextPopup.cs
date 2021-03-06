using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Utility.Text
{
    public class TextPopup : MonoBehaviour
    {
        public TextMeshPro textMeshComponent;
        public float duration = 3f;
        public float speed = 1f;
        private WaitForSeconds _timer;
        private void OnEnable()
        {
            _timer = new WaitForSeconds(duration);
            ObjectPooling.Instance.PoolObject(this,false);
            StartCoroutine(DisableTimer());
        }
        private void OnDisable()
        {
            ObjectPooling.Instance.PoolObject(this);
        }
        
        void Update()
        {
            transform.Translate(Vector3.up * (Time.deltaTime * speed));
        }

        private IEnumerator DisableTimer()
        {
            yield return _timer;
            gameObject.SetActive(false);
        }
    }
}